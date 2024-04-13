using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTTGC.Prat.Common.Response;

public class PatentClusterSignedUrlsData
{
    public List<string> Urls { get; set; } = new();

    public DateTimeOffset Expiry { get; set; } = DateTimeOffset.MinValue;
}
