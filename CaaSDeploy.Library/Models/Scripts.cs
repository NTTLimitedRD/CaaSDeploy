namespace CaasDeploy.Library.Models
{
    /// <summary>
    /// Represents a resource scripts block.
    /// </summary>
    public class Scripts
    {
        /// <summary>
        /// Gets or sets the path to the bundle file.
        /// </summary>
        public string bundleFile { get; set; }

        /// <summary>
        /// Gets or sets the on deploy.
        /// </summary>
        public string onDeploy { get; set; }
    }
}
