using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement
{
    internal class PartialViewMacroRepository : PartialViewRepository, IPartialViewMacroRepository
    {
        public PartialViewMacroRepository(IIOHelper ioHelper, IHostingEnvironment hostingEnvironment, ILoggerFactory factory)
            : base(ioHelper, hostingEnvironment, factory,
                hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.MacroPartials),
                hostingEnvironment.ToAbsolute(Constants.SystemDirectories.MacroPartials))
        { }

        protected override PartialViewType ViewType => PartialViewType.PartialViewMacro;
    }
}
