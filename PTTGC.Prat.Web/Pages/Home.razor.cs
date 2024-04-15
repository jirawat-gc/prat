using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PTTGC.Prat.Common;
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

    public bool IsDemo { get; set; } = true;

    /// <summary>
    /// Current workspace session
    /// </summary>
    public Workspace Workspace { get; set; } = new()
    {
        Id = Guid.Parse("00000000-1111-1111-1111-000000000000"),
        OwnerId = "00000000-2222-2222-2222-000000000000",
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

    /// <summary>
    /// Currently Analying Patent
    /// </summary>
    public Patent AnalyzingPatent { get; set; } = new();

    public Patent ViewingPatent { get; set; } = new();

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

            this.LogMessage = "Loading Patent Cluster Data";

            await ProjectData.Default.EnsurePatentClustersLoaded();

            await ProjectData.Default.LoadDemoWorkspace();

            this.Workspace = ProjectData.Default.DemoWorkspace;

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

            this.Workspace.AIResponse = prompResponse;
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

        this.Workspace.AIEmbeddingVector = new double[768];
        try
        {
            this.LogMessage = "Getting Similarity Search Vector...";
            this.Workspace.AIEmbeddingVector = await PratBackend.Default.Embeddings(new EmbeddingRequest()
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

        this.LogMessage = "Finding Similar Patents using Vector Similarity...";
        try
        {
            this.Workspace.SimilarPatent = await PratBackend.Default.FindSimilarPatent(new SimilaritySearchRequest()
            {
                VectorBase64 = this.Workspace.AIEmbeddingVector
            });
        }
        catch (Exception ex)
        {
            this.IsBusy = false;
            _ = this.JS.InvokeVoidAsync("alert", $"Issue communicating with servers, please try again.\r\n\r\n{ex.Message}");
            return;
        }

        this.LogMessage = "Looking for Patent Cluster...";
        var featureRequest = new FindClusterRequest(this.Workspace.AIEmbeddingVector, this.Workspace.AIPatentFlags );
        var modelInput = JsonConvert.SerializeObject(featureRequest, Formatting.None);
        var result = await this.PythonPredictor.InvokeAsync<string>("runPredictor", modelInput);

        var jso = JObject.Parse(result);
        var coords = new double[] {
                (double)jso["result"]["visualization_coord"]["0"],
                (double)jso["result"]["visualization_coord"]["1"],
                (double)jso["result"]["visualization_coord"]["2"]
            };

        this.Workspace.AIPredictedVisualizationCoords = coords;

        if (this.IsDemo)
        {
            this.Workspace.AIPredictedCluster = "DEMO";
            this.Workspace.MatchingCluster = new PatentCluster()
            {
                PatentApplicationIds = new List<string>()
                {
                    "0601000506",
                    "0701000705",
                    "1501005841",
                    "1501005841",
                    "2101002233",
                    "2001006704",
                    "0401000323"
                },
                ClusterLabel = "DEMO",
            };
        }
        else
        {
            this.Workspace.AIPredictedCluster = jso["result"]["cluster"].ToString();

            await ProjectData.Default.EnsurePatentClustersLoaded();
            this.Workspace.MatchingCluster = ProjectData.Default.PatentClusters.FirstOrDefault(pc => pc.ClusterLabel == this.Workspace.AIPredictedCluster);
        }

        await this.SaveWorkspace();

        this.IsBusy = false;
        this.LogMessage = null;
    }

    public void SetActiveCluster( string clusterId )
    {

    }

    public async Task SaveWorkspace()
    {
        this.IsBusy = true;
        this.LogMessage = "Saving Workspace...";

        try
        {
            await PratBackend.Default.SubmitWorkspace(this.Workspace);
        }
        catch (Exception ex)
        {
            this.IsBusy = false;
            _ = this.JS.InvokeVoidAsync("alert", $"Issue communicating with servers, workspace is not saved.\r\n\r\n{ex.Message}");
            return;
        }

        this.LogMessage = null;
        this.IsBusy = false;
    }

    private Task PromptBackground(Patent p, JObject context, string promptKey)
    {
        return Task.Run(async () =>
        {
            var result = await PratBackend.Default.Prompt(new PromptRequest()
            {
                PromptContext = context,
                PromptKey = promptKey
            });

            p.Analysis.PromptResponses[promptKey] = result;
        });
    }

    /// <summary>
    /// Meant to be called by UI to perform analysis on all patents
    /// </summary>
    /// <returns></returns>
    public async Task BeginAnalysis()
    {
        this.IsBusy = true;

        var existing = this.Workspace.PatentsToAnalyze.Select(p => p.ApplicationId).ToHashSet();

        foreach (var id in this.Workspace.MatchingCluster.PatentApplicationIds)
        {
            if (existing.Contains( id ) == false)
            {
                this.Workspace.PatentsToAnalyze.Add(new Patent() { ApplicationId = id });
            }
        }

        if (this.IsDemo == false)
        {
            foreach (var p in this.Workspace.SimilarPatent)
            {
                if (existing.Contains(p.ApplicationId) == false)
                {
                    this.Workspace.PatentsToAnalyze.Add(p);
                }
            }

        }

        foreach (var patent in this.Workspace.PatentsToAnalyze)
        {
            if (patent.Analysis.IsAnalysisCompleted)
            {
                continue;
            }

            try
            {
                this.LogMessage = $"AI is Analyzing Patent #{patent.ApplicationId}";
                await this.AnalyzePatent(patent);

                this.LogMessage = $"Saving Analysis...";
                await PratBackend.Default.SubmitWorkspace(this.Workspace);

                this.StateHasChanged();
            }
            catch (Exception ex)
            {
                patent.Analysis.IsAnalysisCompleted = false;
                patent.Analysis.IsAnalyzing = false;

                this.IsBusy = false;
                _ = this.JS.InvokeVoidAsync("alert", $"Issue communicating with servers, please try again.\r\n\r\n{ex.Message}");
                return;
            }
            
        }

        this.LogMessage = null;
        this.IsBusy = false;
    }

    public async Task AnalyzePatentForUI( Patent p)
    {
        this.IsBusy = true;

        try
        {
            await this.AnalyzePatent(p);

            await PratBackend.Default.SubmitWorkspace(this.Workspace);
        }
        catch (Exception ex)
        {
            this.IsBusy = false;
            _ = this.JS.InvokeVoidAsync("alert", $"Issue communicating with servers, please try again.\r\n\r\n{ex.Message}");
            return;
        }
        finally
        {
            p.Analysis.IsAnalyzing = false;
        }

        this.LogMessage = null;
        this.IsBusy = false;
    }

    /// <summary>
    /// Analyze individual patent
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public async Task AnalyzePatent(Patent p)
    {
        p.Analysis.IsAnalyzing = true;
        this.AnalyzingPatent = p;
        this.StateHasChanged();

        if (p.PatentClaims == null)
        {
            try
            {
                await PratBackend.Default.PopulateFromGCS($"patents/{p.ApplicationId}.json.gz", p, true);

                p.Analysis.IsAnalyzing = true;
                this.StateHasChanged();
            }
            catch (Exception)
            {
                p.Analysis.IsAnalyzing = false;
                return;
            }
        }

        var promptContext = JObject.FromObject(p);

        this.LogMessage = $"Extraction Main Claim from #{p.ApplicationId}";
        this.StateHasChanged();
        await this.PromptBackground(p, promptContext, "EXTRACT_MAIN_CLAIM");

        this.LogMessage = $"Extraction Test Result from #{p.ApplicationId}";
        this.StateHasChanged();
        await this.PromptBackground(p, promptContext, "EXTRACT_TEST_RESULT");

        this.LogMessage = $"Extraction Polymer Features from #{p.ApplicationId}";
        this.StateHasChanged();
        await this.PromptBackground(p, promptContext, "EXTRACT_POLYMER_FEATURE");

        p.Analysis.PopulateStandaloneClaim();
        p.Analysis.PopulatePolymerFeatures();
        p.Analysis.PopulateMaterialAttributes();

        p.Analysis.IsAnalyzing = false;
        p.Analysis.IsAnalysisCompleted = true;
        this.LogMessage = $"Analysis of #{p.ApplicationId} completed";
        this.StateHasChanged();

    }

    public void ViewPatent( Patent p)
    {
        this.ViewingPatent = p;
        this.StateHasChanged();

        _ = this.JS.InvokeVoidAsync("showModal", "#viewingPatentModal");
    }

    public void LoadIframe( string iframeId, string src)
    {
        _ = this.JS.InvokeVoidAsync("navigateIframe", iframeId, src);
    }
}
