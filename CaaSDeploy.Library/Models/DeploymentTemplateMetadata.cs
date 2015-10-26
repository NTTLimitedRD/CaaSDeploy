namespace CaasDeploy.Library.Models
{
    /// <summary>
    /// Represents the metadata of a deployment template.
    /// </summary>
    public class DeploymentTemplateMetadata
    {
        /// <summary>
        /// Gets or sets the schema version.
        /// </summary>
        public string schemaVersion { get; set; }

        /// <summary>
        /// Gets or sets the name of the template.
        /// </summary>
        public string templateName { get; set; }

        /// <summary>
        /// Gets or sets the description of the template.
        /// </summary>
        public string templateDescription { get; set; }
    }
}
