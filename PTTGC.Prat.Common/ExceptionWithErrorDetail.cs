using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTTGC.Prat.Common;

public class ExceptionWithErrorDetail : Exception
{
    public ErrorDetail Detail { get; set; }

    public ExceptionWithErrorDetail( ErrorDetail detail)
    { 
        this.Detail = detail;
    }
}
