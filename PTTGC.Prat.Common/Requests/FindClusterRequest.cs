using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTTGC.Prat.Common.Requests;

public class FindClusterRequest
{
    /// <summary>
    /// Embedding Vector
    /// </summary>
    public double[] SummaryEmbeddingVector { get; set; }

    /// <summary>
    /// Innovation flags, as run through Prompt
    /// </summary>
    public Dictionary<string, bool> Flags { get; set; }
}
