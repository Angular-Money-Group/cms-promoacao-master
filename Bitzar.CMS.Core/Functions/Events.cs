using Bitzar.CMS.Extension.CMS;
using Bitzar.CMS.Model;
using MethodCache.Attributes;
using System;

namespace Bitzar.CMS.Core.Functions.Internal
{
    /// <summary>
    /// Class to allow the service to call events
    /// </summary>
    public class Events : IEvents
    {
        /// <summary>
        /// Method to trigger an event in the service accepting an enumerator in the context
        /// </summary>
        /// <typeparam name="T">Typeof the Data to be added in the event</typeparam>
        /// <param name="eventType">Event type name to be used</param>
        /// <param name="data">Data related to the event </param>
        /// <param name="exception"></param>
        [NoCache]
        public void Trigger<T>(Enumerators.EventType eventType, T data, Exception exception = null)
        {
            Trigger<T>(eventType.ToString(), data, exception);
        }

        /// <summary>
        /// Method to trigger an event in the service accepting an enumerator in the context
        /// </summary>
        /// <typeparam name="T">Typeof the Data to be added in the event</typeparam>
        /// <param name="eventType">Event type name to be used</param>
        /// <param name="data">Data related to the event </param>
        /// <param name="exception"></param>
        [NoCache]
        public void Trigger<T>(string eventType, T data, Exception exception = null)
        {
            try
            {
                foreach (var plugin in CMS.Plugins.Available)
                    plugin.Plugin.TriggerEvent(eventType.ToString(), data, exception);
            }
                catch (Exception ex)
            {
                CMS.Log.LogRequest(ex, eventType, data, exception);
                throw ex;
            }
        }
    }
}