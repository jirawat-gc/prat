
using PTTGC.Prat.Common;

namespace PTTGC.Prat.Core
{
    public class Workspace
    {
        /// <summary>
        /// Unique Identifier of this workspace
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Owner of this Workspace
        /// </summary>
        public string OwnerId { get; set; }

        /// <summary>
        /// What user entered as innovation title
        /// </summary>z
        public string InnovationTitle { get; set; }

        /// <summary>
        /// What user entered as innovation description
        /// </summary>
        public string InnovationDescription { get; set; }

        /// <summary>
        /// Flags as set on Frontend
        /// </summary>
        public Dictionary<string, bool> InnovationFlags { get; set; } = new();

        /// <summary>
        /// List of Material Attributes related to this innovation
        /// </summary>
        public List<MaterialAttribute> MaterialAttributes { get; set; } = new();

        /// <summary>
        /// Summary of the text for search by AI
        /// </summary>
        public string AISearchText { get; set; }

        /// <summary>
        /// Embedding Vector created from AI Search Text
        /// </summary>
        public double[] AIEmbeddingVector { get; set; } = new double[0];

        /// <summary>
        /// Flag as processed by AI
        /// </summary>
        public Dictionary<string, string> AIPatentFlags { get; set; } = new();

        /// <summary>
        /// The cluster which matches this innovation
        /// </summary>
        public PatentCluster MatchingCluster { get; set; } = new();

        /// <summary>
        /// Patent that was analyzed as part of this workspace
        /// </summary>
        public List<Patent> AnalyzedPatents { get; set; } = new();
    }
}
