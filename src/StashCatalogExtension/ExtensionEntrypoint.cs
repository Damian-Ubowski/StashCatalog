using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Extensibility;
using StashCatalogExtension.Services;

namespace StashCatalogExtension
{
    /// <summary>
    /// Extension entrypoint for the VisualStudio.Extensibility extension.
    /// </summary>
    [VisualStudioContribution]
    internal class ExtensionEntrypoint : Extension
    {
        /// <inheritdoc/>
        public override ExtensionConfiguration ExtensionConfiguration => new()
        {
            Metadata = new(
                    id: "StashCatalogExtension.f6251360-c7c3-4a5b-8c0c-892d106f087e",
                    version: this.ExtensionAssemblyVersion,
                    publisherName: "Damian Ubowski",
                    displayName: "Stash Catalog",
                    description: "Visual Studio extension that improves Git stash workflow by allowing users to create named Git stashes"),
        };

        /// <inheritdoc />
        protected override void InitializeServices(IServiceCollection serviceCollection)
        {
            base.InitializeServices(serviceCollection);

            // Register our services for dependency injection
            serviceCollection.AddSingleton<GitStashService>();
            serviceCollection.AddSingleton<GitRepositoryService>();
        }
    }
}
