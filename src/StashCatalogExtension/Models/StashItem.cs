namespace StashCatalogExtension.Models
{
    /// <summary>
    /// Represents a Git stash item
    /// </summary>
    public class StashItem
    {
        /// <summary>
        /// Gets or sets the stash index (numerical)
        /// </summary>
        public int Index { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the stash in format "stash@{n}"
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the custom name/description of the stash
        /// </summary>
        public string CustomName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the original stash message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the branch name the stash was created from
        /// </summary>
        public string BranchName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether the stash is a "WIP" (Work in Progress) stash
        /// </summary>
        public bool IsWIP { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the stash was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets the display name of the stash (custom name if available, otherwise the original message)
        /// </summary>
        public string DisplayName => !string.IsNullOrEmpty(CustomName) ? CustomName : Message;
    }
}