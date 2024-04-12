using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTTGC.Prat.Common.Response;

public class GenericResponse<T>
{
    /// <summary>
    /// Data of this response
    /// </summary>
    public T data { get; set; }

    /// <summary>
    /// Detail about error
    /// </summary>
    public int code { get; set; }

    /// <summary>
    /// Detail about error
    /// </summary>
    public string message { get; set; }
}
