using PTTGC.Prat.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PTTGC.Prat.Common;

public class PatentCluster
{
    /// <summary>
    /// DBSCAN's cluster label 
    /// </summary>
    public string ClusterLabel { get; set; }

    /// <summary>
    /// Cluster Centoid in 3D Space
    /// (Use PCA to reduce features into 3D)
    /// </summary>
    public Vector3 ClusterCentoid { get; set; }

    /// <summary>
    /// Ids of Patents in this cluster
    /// </summary>
    public List<string> PatentApplicationIds { get; set; }

    /// <summary>
    /// Patents in this cluster
    /// </summary>
    public List<Patent> Patents { get; set; } = new();
}
