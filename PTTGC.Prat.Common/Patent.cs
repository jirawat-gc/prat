using PTTGC.Prat.Common;

namespace PTTGC.Prat.Core
{
    public class Patent
    {
        public string ApplicationId { get; set; }

        public string Title { get; set; }

        public string Summary { get; set; }

        /// <summary>
        /// Full Patent Claim Text
        /// </summary>
        public string PatentClaims { get; set; }

        /// <summary>
        /// Link to see the actual patent file
        /// </summary>
        public string PatentDetailUrl { get; set; }

        /// <summary>
        /// Patent Flags
        /// </summary>
        public Dictionary<string, bool> Flags { get; set; } = new();

        /// <summary>
        /// Cluster which this patent blongs to
        /// </summary>
        public string ClusterLabel { get; set; }

        public PatentAnalysis Analysis { get; set;} = new();
    }
}
