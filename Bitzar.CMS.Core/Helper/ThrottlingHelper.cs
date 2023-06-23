using System;
using System.Web;
using System.Web.Caching;

namespace Bitzar.CMS.Core.Helper
{
    /// <summary>
    /// Time Unity to throttle the request
    /// </summary>
    public enum TimeUnit
    {
        Instantly = 1,
        Minutely = Instantly * 60,
        Hourly = Minutely * 60,
        Daily = Hourly * 24
    }

    /// <summary>
    /// Class counter to be added in the cache and keep the expiration
    /// </summary>
    internal class CacheCounter
    {
        public int Value = 0;
        public DateTime Expiration { get; private set; }

        public CacheCounter(int expiration)
        {
            this.Expiration = DateTime.Now.AddSeconds(expiration);
        }

        public void Increment() 
            => System.Threading.Interlocked.Increment(ref Value);
    }

    /// <summary>
    /// Request throttling attribute to prevent DDoS attacks 
	/// or to make sure no one tries to brute-force-use your api.
    /// </summary>
    public static class ThrottlingHelper
    {
        /// <summary>
        /// Internal method to get the default throttling value for the service
        /// basead on the time unit provided
        /// </summary>
        /// <param name="timeUnit"></param>
        /// <returns></returns>
        public static int DefaultTimeThrottling(TimeUnit timeUnit)
        {
            switch (timeUnit)
            {
                case TimeUnit.Minutely:
                    return Functions.CMS.Configuration.ThrottlingMinute;
                case TimeUnit.Hourly:
                    return Functions.CMS.Configuration.ThrottlingHour;
                case TimeUnit.Daily:
                    return Functions.CMS.Configuration.ThrottlingDay;
                case TimeUnit.Instantly:
                default:
                    return Functions.CMS.Configuration.ThrottlingInstant;
            }
        }

        /// <summary>
        /// Logic to process the request and throttle data in the service
        /// </summary>
        /// <param name="context"></param>
        public static void Validate(TimeUnit timeUnit, string controller, string action)
        {
            // Set the quota if not preset and the multiplier
            var quota = DefaultTimeThrottling(timeUnit);

            // Get current authenticated user
            var user = Functions.CMS.Membership.User;
            if (user != null)
                quota *= Functions.CMS.Configuration.ThrottlingMultiplier;

            // Set other variables        
            var userName = user?.UserName ?? "Visitor";
            var cache = HttpContext.Current.Cache;

            // Set default name for the cache key
            var keyPrefix = $"{userName}-{controller}-{action}";

            // Get the client IP address and set the ip on throttling
            var ipAddress = MvcApplication.GetClientIp();
            var key = $"{keyPrefix}-{timeUnit}-{ipAddress}";

            // Set the counters in the cache
            var counter = (CacheCounter)cache[key] ?? new CacheCounter((int)timeUnit);
            counter.Increment();

            // Check if any of them fail
            if (counter.Value > quota)
            {
                // Save log data
                Functions.CMS.Log.LogRequest(new
                {
                    User = userName,
                    Controller = controller,
                    Action = action,
                    Key = keyPrefix,
                    IP = ipAddress
                });

                throw new AccessViolationException($"You are being throttled and should wait some time to continue. Counter {counter.Value}/{quota} {timeUnit.ToString().ToLower()}.");
            }
            else
            {
                // Update the cache object
                cache.Remove(key);
                cache.Add(key, counter, null, counter.Expiration,
                        Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
            }
        }
    }
}