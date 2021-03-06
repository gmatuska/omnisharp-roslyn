using System;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using OmniSharp.Extensions;
using OmniSharp.Mef;
using OmniSharp.Models;
using OmniSharp.Models.FindSymbols;

namespace OmniSharp.Roslyn.CSharp.Services.Navigation
{
    [OmniSharpHandler(OmniSharpEndpoints.FindSymbols, LanguageNames.CSharp)]
    public class FindSymbolsService : IRequestHandler<FindSymbolsRequest, QuickFixResponse>
    {
        private OmniSharpWorkspace _workspace;

        [ImportingConstructor]
        public FindSymbolsService(OmniSharpWorkspace workspace)
        {
            _workspace = workspace;
        }

        public async Task<QuickFixResponse> Handle(FindSymbolsRequest request = null)
        {
            if (request?.Filter?.Length < request?.MinFilterLength.GetValueOrDefault())
            {
                return new QuickFixResponse { QuickFixes = Array.Empty<QuickFix>() };
            }

            Func<string, bool> isMatch =
                candidate => request != null
                ? candidate.IsValidCompletionFor(request.Filter)
                : true;

            int maxItemsToReturn = (request?.MaxItemsToReturn).GetValueOrDefault();
            var csprojSymbols = await _workspace.CurrentSolution.FindSymbols(isMatch, ".csproj", maxItemsToReturn);
            var projectJsonSymbols = await _workspace.CurrentSolution.FindSymbols(isMatch, ".json", maxItemsToReturn);
            return new QuickFixResponse()
            {
                QuickFixes = csprojSymbols.QuickFixes.Concat(projectJsonSymbols.QuickFixes)
            };
        }
    }
}
