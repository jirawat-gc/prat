
using Newtonsoft.Json.Linq;
using PTTGC.Prat.Common;

namespace PTTGC.Prat.Core
{
    public class Workspace
    {
        /// <summary>
        /// Unique Identifier of this workspace
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Owner of this Workspace
        /// </summary>
        public string OwnerId { get; set; } = "00000000-1001-0000-0000-000000000000";

        /// <summary>
        /// What user entered as innovation title
        /// </summary>z
        public string InnovationTitle { get; set; }

        /// <summary>
        /// What user entered as innovation description
        /// </summary>
        public string InnovationDescription { get; set; }

        public bool InnovationIsPolymer { get; set; }

        public string InnovationPolymerKind { get; set; }

        public string InnovationApplication { get; set; }

        public string InnovationComonomer { get; set; }

        /// <summary>
        /// Current max distance threshold
        /// </summary>
        public double MaxDistance { get; set; } = 0.25;

        /// <summary>
        /// Flags as set on Frontend
        /// </summary>
        public Dictionary<string, bool> InnovationFlags { get; set; } = new();

        /// <summary>
        /// List of Material Attributes related to this innovation
        /// </summary>
        public List<MaterialAttribute> MaterialAttributes { get; set; } = new();

        /// <summary>
        /// Whether the innovation has material test result
        /// </summary>
        public bool InnovationIncludeTestResults { get; set; }

        /// <summary>
        /// Response as received by AI
        /// </summary>
        public string AIResponse { get; set; }

        /// <summary>
        /// Summary of the text for search by AI
        /// </summary>
        public string AISearchText { get; set; }

        /// <summary>
        /// Predicted Cluster (from Python Model)
        /// </summary>
        public string AIPredictedCluster { get; set; }

        /// <summary>
        /// Predicted Cluster Coordinates 
        /// </summary>
        public double[] AIPredictedVisualizationCoords { get; set; } = new double[3];

        /// <summary>
        /// Embedding Vector created from AI Search Text
        /// </summary>
        public VectorEmbedding AIEmbeddingVector { get; set; } = new();

        /// <summary>
        /// Flag as processed by AI
        /// </summary>
        public Dictionary<string, bool> AIPatentFlags { get; set; } = new();

        /// <summary>
        /// The cluster which matches this innovation
        /// </summary>
        public PatentCluster MatchingCluster { get; set; } = new();

        /// <summary>
        /// Ths patents which are similar to current patent
        /// </summary>
        public List<Patent> SimilarPatent { get; set; } = new();

        /// <summary>
        /// Patent that was going to be analyzed as part of this workspace
        /// </summary>
        public List<Patent> PatentsToAnalyze { get; set; } = new();

        /// <summary>
        /// Gets the prompt context from workspace
        /// </summary>
        /// <returns></returns>
        public JObject GetPromptContext()
        {
            var jo = JObject.FromObject(this);
            var whitelist = "InnovationTitle,InnovationDescription,InnovationApplication,InnovationIncludeTestResults,MaterialAttributes,InnovationIsPolymer,InnovationPolymerKind,InnovationComonomer,";

            foreach (var prop in jo.Properties().Select( p => p.Name ).ToList())
            {
                if (whitelist.Contains($"{prop},") == false)
                {
                    jo.Remove(prop);
                }
            }

            return jo;
        }
    }
}
