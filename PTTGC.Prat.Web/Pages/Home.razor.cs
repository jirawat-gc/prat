using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PTTGC.Prat.Common.Requests;
using PTTGC.Prat.Core;
using System.Text;

namespace PTTGC.Prat.Web.Pages;

public partial class Home : ComponentBase
{
    private bool _IsBusy;

    public bool IsBusy
    {
        get => _IsBusy;
        set
        {
            _IsBusy = value;
            this.StateHasChanged();
        }
    }

    private string _LogMessage;

    public string LogMessage
    {
        get => _LogMessage;
        set
        {
            _LogMessage = value;
            this.StateHasChanged();
        }
    }

    /// <summary>
    /// Current workspace session
    /// </summary>
    public Workspace Workspace { get; set; } = new()
    {
        InnovationTitle = "Polyethylene for blown film application with high elasticity property",
        InnovationDescription = "This innovation is about polyethylene polymer with C4 (butene) as comonomer which have the specified test result",
        InnovationPolymerKind = "Polyethylene",
        InnovationApplication = "Film",
        InnovationComonomer = "C4 (butene)",
        InnovationIsPolymer = true,
        InnovationIncludeTestResults = true,
        MaterialAttributes = new()
        {
            new()
            {
                AttributeName = "Density",
                LowerBound = 0.940f,
                UpperBound = 0.950f,
                MeasurementUnit = "g/cm³"
            },
            new()
            {
                AttributeName = "Melt flow rate (MFR; at 190℃, 2.16 kg)",
                LowerBound = 1.0f,
                UpperBound = 3.0f,
                MeasurementUnit = "g/10 min"
            },
            new()
            {
                AttributeName = "Molecular weight (Mw)",
                LowerBound = 100000f,
                MeasurementUnit = "g/㏖"
            },
            new()
            {
                AttributeName = "Molecular weight distribution (MWD or Mw/Mn)",
                LowerBound = 1.5f,
                UpperBound = 3.0f,
                MeasurementUnit = string.Empty
            }
        }
    };

    [Inject]
    public IJSRuntime JS { get; set; }

    private IJSObjectReference? PythonPredictor;

    public MaterialAttribute NewMaterialAttributeItem = new();

    public List<(string attribute, string unit)> _CommonAttributes = new()
    {
        new ("Melt flow rate (MFR; at 190℃, 2.16 kg)", "g/10 min"),
        new ("Molecular weight distribution (MWD or Mw/Mn)", ""),
        new ("Density", "g/cm³"),
        new ("Vicat softening point", "℃"),
        new ("Melting Temperature", "℃"),
        new ("Tensile Strength at Yield", "kg/cm²"),
        new ("Tensile Strength at Break", "kg/cm²"),
        new ("Elongation at Break", "%"),
        new ("Stiffness", "kg/cm²"),
        new ("Flexural Modulus", "kg/cm²"),
        new ("Notched Izod Impact Strength", "kg.cm/cm"),
        new ("Durometer Hardness", "Shore D"),
        new ("ESCR, F50 (Condition B, 10 % Igepal)", "hrs"),
        new ("Lambda", "㎭/s"),
        new ("Molecular weight (Mw)", "g/㏖"),
    };

    private void ChangeMaterialAttribute( MaterialAttribute item, (string attribute, string unit) attr )
    {
        item.AttributeName = attr.attribute;
        item.LowerBound = 0;
        item.UpperBound = 0;
        item.MeasurementUnit = attr.unit;

        this.StateHasChanged();
    }

    private bool IsCustomAttribute( MaterialAttribute item )
    {
        return _CommonAttributes.Any(a => a.attribute == item.AttributeName) == false;
    }

    private string FixFlagKey( string flagKey)
    {
        return new string(flagKey.ToLowerInvariant()
                    .Replace(" ", "_")
                    .Where( c => char.IsLetterOrDigit(c))
                    .ToArray() );
    }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            this.LogMessage = "Loading Python Worker...";

            this.PythonPredictor = await JS.InvokeAsync<IJSObjectReference>("import",
                "/pyoide/py-worker.js");

            this.LogMessage = null;
        }
    }


    public async Task ProcessWorkspace()
    {
        this.IsBusy = true;

        // extract features - using Backend VertexAI Prompt
        string prompResponse = null;
        try
        {
            this.LogMessage = "Analyzing content...";
            prompResponse = await PratBackend.Default.Prompt(new PromptRequest()
            {
                MaxTokens = 2000,
                PromptKey = "EXTRACT_INNOVATION_FEATURES",
                PromptContext = this.Workspace.GetPromptContext(),
                Temperature = 0
            });
        }
        catch (Exception ex)
        {
            this.IsBusy = false;
            _ = this.JS.InvokeVoidAsync("alert", $"Issue communicating with servers, please try again.\r\n\r\n{ex.Message}");
            return;
        }

        // Parse the json response
        try
        {
            prompResponse = prompResponse.Trim();

            if (prompResponse.StartsWith("```json"))
            {
                prompResponse = prompResponse.Substring("```json".Length);
            }

            if (prompResponse.EndsWith("```"))
            {
                prompResponse = prompResponse.Substring(0, prompResponse.LastIndexOf("}") + 1);
            }

            prompResponse = prompResponse.Trim();
            var resultJo = JObject.Parse(prompResponse);

            foreach (var prop in resultJo.Properties())
            {
                if (prop.Name == "summary")
                {
                    this.Workspace.AISearchText = (string)prop.Value;
                }
                else
                {
                    if (prop.Value.Type == JTokenType.Null)
                    {
                        continue;
                    }

                    if (prop.Value.Type == JTokenType.Boolean)
                    {
                        this.Workspace.AIPatentFlags[prop.Name.ToLowerInvariant()] = (bool)prop.Value;
                        continue;
                    }

                    if (prop.Value.Type == JTokenType.String)
                    {
                        this.Workspace.AIPatentFlags[prop.Name.ToLowerInvariant()] = prop.Value.ToString().ToLowerInvariant() == "true";
                        continue;
                    }
                }
            }

        }
        catch (Exception)
        {
            this.IsBusy = false;
            _ = this.JS.InvokeVoidAsync("alert", "AI does not responded in expected format, please try again.");
            return;
        }


        if (this.Workspace.AISearchText == null)
        {
            this.IsBusy = false;
            _ = this.JS.InvokeVoidAsync("alert", "AI does not responded in expected format, please try again.");
            return; // Error
        }

        // get embedding - using Backend VertexAI Embedding with summary text

        double[] embedding = new double[0];
        try
        {
            this.LogMessage = "Getting Similarity Search Vector...";
            embedding = await PratBackend.Default.Embeddings(new EmbeddingRequest()
            {
                Content = this.Workspace.AISearchText!
            });
        }
        catch (Exception ex)
        {
            this.IsBusy = false;
            _ = this.JS.InvokeVoidAsync("alert", $"Issue communicating with servers, please try again.\r\n\r\n{ex.Message}");
            return;
        }

        this.LogMessage = "Looking for Local Patent Cluster...";

        // get the cluster and coords
        var featureRequest = new FindClusterRequest(embedding, this.Workspace.AIPatentFlags );
        var modelInput = JsonConvert.SerializeObject(featureRequest, Formatting.None);
        var result = await this.PythonPredictor.InvokeAsync<string>("runPredictor", modelInput);

        var jso = JObject.Parse(result);

        this.Workspace.AIPredictedCluster = jso["result"]["cluster"].ToString();

        var coords = new double[] {
                (double)jso["result"]["visualization_coord"]["0"],
                (double)jso["result"]["visualization_coord"]["1"],
                (double)jso["result"]["visualization_coord"]["2"]
            };

        this.Workspace.AIPredictedVisualizationCoords = coords;

        this.IsBusy = false;
        this.LogMessage = null;
    }

    public void SetActiveCluster( string clusterId )
    {

    }
}
