namespace PTTGC.Prat.Core
{
    public class MatchingPatent
    {
        public string ApplicationId { get; set; }

        public string Title { get; set; }

        public string Summary { get; set; }

        /// <summary>
        /// Patent Flags
        /// </summary>
        public Dictionary<string, bool> Flags { get;}

        /// <summary>
        /// Cluster which this patent blongs to
        /// </summary>
        public string ClusterNumber { get; set; }


    }
}
