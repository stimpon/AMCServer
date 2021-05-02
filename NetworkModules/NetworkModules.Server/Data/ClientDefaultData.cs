namespace NetworkModules.Server
{
    // Required namespaces
    using NetworkModules.Core;

    /// <summary>
    /// This data will be set for new clients...
    /// </summary>
    public static class ClientDefaultData
    {
        /// <summary>
        /// Gets or sets the default client permissions.
        /// </summary>
        public static Permissions DEFAULT_ClientPermissions { get; set; } = Permissions.N;
    }
}
