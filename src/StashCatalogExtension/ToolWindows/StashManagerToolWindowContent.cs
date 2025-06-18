using Microsoft.VisualStudio.Extensibility.UI;

namespace StashCatalogExtension.ToolWindows
{
    /// <summary>
    /// A remote user control to use as tool window UI content.
    /// </summary>
    internal class StashManagerToolWindowContent : RemoteUserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StashManagerToolWindowContent" /> class.
        /// </summary>
        public StashManagerToolWindowContent()
            : base(dataContext: new StashManagerToolWindowData())
        {
        }
    }
}
