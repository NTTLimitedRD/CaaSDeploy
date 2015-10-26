namespace CaasDeploy.Library.Models
{
    /// <summary>
    /// The status for a <see cref="ResourceLog"/>.
    /// </summary>
    public enum ResourceLogStatus
    {
        /// <summary>
        /// The unknown status
        /// </summary>
        Unknown,

        /// <summary>
        /// The deployed status
        /// </summary>
        Deployed,

        /// <summary>
        /// The failed status
        /// </summary>
        Failed,

        /// <summary>
        /// The updated status
        /// </summary>
        Updated,

        /// <summary>
        /// The used existing status
        /// </summary>
        UsedExisting
    }
}
