
namespace PTTGC.Prat.Core
{
    public class Workspace
    {
        public Guid Id { get; set; }

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
        /// Flag as processed by AI
        /// </summary>
        public Dictionary<string, string> AIPatentFlags { get; set; } = new();

        /// <summary>
        /// Summary of the text for search by AI
        /// </summary>
        public string AISearchText { get; set; }

        
    }
}
