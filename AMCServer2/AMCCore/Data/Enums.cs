/// <summary>
/// Contains all of the public Enums
/// </summary>
namespace AMCCore
{
    /// <summary>
    /// All item types that can be displayed in the explorer view
    /// </summary>
    public enum ExplorerItemTypes
    {
        HDD,
        File,
        Folder
    }

    /// <summary>
    /// For the file explorer, local or remote navigation
    /// </summary>
    public enum NavigationLocations
    {
        /// <summary>
        /// No navigation
        /// </summary>
        None,
        /// <summary>
        /// Local PC navigation
        /// </summary>
        Local,
        /// <summary>
        /// Remote PC navigation
        /// </summary>
        Remote
    }
}
