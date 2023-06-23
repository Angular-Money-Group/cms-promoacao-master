namespace Bitzar.CMS.Data
{
    /// <summary>
    /// Enumerator to define the database provider 
    /// </summary>
    public enum DatabaseProvider
    {
        /// <summary>
        /// Indicates that use SqlServer or SqlAzure as Database Provider
        /// </summary>
        SqlServer,
        /// <summary>
        /// Indicates that use MySql as Database Provider
        /// </summary>
        MySql
    }

    /// <summary>
    /// Indicates the type of the device
    /// </summary>
    public enum DeviceType
    {
        Android = 1,
        IOS = 2,
        Browser = 3,
        Other = 4
    }

    /// <summary>
    /// Created for searching and creating claims
    /// </summary>
    public enum ClaimType
    {
        Role,
        Mail,
        RoleId,
        UserId,
        System,
        UserName,
        LastName,
        FirstName
    }
}
