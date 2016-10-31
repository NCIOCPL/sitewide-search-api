using System.Collections.Generic;
using System.Linq;

namespace NCI.OCPL.Services.SiteWideSearch
{
    /// Container for the the list of potential search terms returned
    /// by the SiteWideSearch.AutosuggestController.
    public class Suggestions
    {
        // The set of potential search items.
        public Suggestion[] Results { get; private set; }

        // The total number of matching search terms available. 
        public long Total { get; private set; }

        public Suggestions(long totalResults, IEnumerable<Suggestion> results)
        {
            Results = results.ToArray();
            Total = totalResults;
        }


    }
}