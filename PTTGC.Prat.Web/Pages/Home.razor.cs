using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PTTGC.Prat.Common;
using PTTGC.Prat.Common.Requests;
using PTTGC.Prat.Core;
using System.Security.Claims;
using System.Text;
using static PTTGC.Prat.Common.PatentAnalysis;
using static System.Net.Mime.MediaTypeNames;

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

    private void ChangeMaterialAttribute(MaterialAttribute item, (string attribute, string unit) attr)
    {
        item.AttributeName = attr.attribute;
        item.LowerBound = 0;
        item.UpperBound = 0;
        item.MeasurementUnit = attr.unit;

        this.StateHasChanged();
    }

    private bool IsCustomAttribute(MaterialAttribute item)
    {
        return _CommonAttributes.Any(a => a.attribute == item.AttributeName) == false;
    }

    private string FixFlagKey(string flagKey)
    {
        return new string(flagKey.ToLowerInvariant()
                    .Replace(" ", "_")
                    .Where(c => char.IsLetterOrDigit(c))
                    .ToArray());
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
        var featureRequest = new FindClusterRequest(this.Workspace.AIEmbeddingVector, this.Workspace.AIPatentFlags);
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

    public void SetActiveCluster(string clusterId)
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
            if (existing.Contains(id) == false)
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
                await this.ExtractPatent(patent);

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

    public async Task ExtractPatentForUI(Patent p)
    {
        this.IsBusy = true;

        try
        {
            await this.ExtractPatent(p);

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
    public async Task ExtractPatent(Patent p)
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

        this.LogMessage = $"Extracting List of Claims from #{p.ApplicationId}";
        this.StateHasChanged();

        var retryCount = 0;

    // 1) list the claims
    RetryClaim:
        try
        {
            var claimsResponse = await PratBackend.Default.Prompt(new PromptRequest()
            {
                PromptContext = promptContext,
                PromptKey = "EXTRACT_CLAIMS"
            });

            var claimsResult = claimsResponse.FixJson();
            var claimList = claimsResult["claims"] as JArray;
            p.Analysis.Claims = claimList.Select(item => item.ToObject<PatentAnalysis.Claim>()).ToList();

            p.Analysis.PromptResponses["EXTRACT_CLAIMS"] = claimsResponse;
            p.Analysis.PromptOutput["EXTRACT_CLAIMS"] = claimsResult;

        }
        catch (Exception)
        {
            if (retryCount == 3)
            {
                throw;
            }

            this.LogMessage = $"Retrying Claim Extraction #{p.ApplicationId}";
            await Task.Delay(5000);

            retryCount++;
            goto RetryClaim;
        }

    // 2) for each claim, extract material feature
    //p.Analysis.TestResults = new();
    //foreach (var claim in p.Analysis.Claims)
    //{
    //    this.LogMessage = $"Extracting Result from #{p.ApplicationId} ({claim.index}/{p.Analysis.Claims.Count})";
    //    retryCount = 0;

    //RetryExtractTestResult:
    //    try
    //    {
    //        var testResultResponse = await PratBackend.Default.Prompt(new PromptRequest()
    //        {
    //            PromptContext = JObject.FromObject(claim),
    //            PromptKey = "EXTRACT_TEST_RESULT_PER_CLAIM"
    //        });

    //        var testResultJo = testResultResponse.FixJson();

    //        p.Analysis.PromptResponses[$"EXTRACT_TEST_RESULT_PER_CLAIM_{claim.index}"] = testResultResponse;
    //        p.Analysis.PromptOutput["EXTRACT_TEST_RESULT_PER_CLAIM_{claim.index}"] = testResultJo;

    //        p.Analysis.TestResults.Add(testResultJo.ToObject<PatentAnalysis.TestResult>());
    //    }
    //    catch (Exception)
    //    {
    //        if (retryCount == 3)
    //        {
    //            throw;
    //        }

    //        this.LogMessage = $"Retrying Result Extraction #{p.ApplicationId} ({claim.index}/{p.Analysis.Claims.Count})";
    //        await Task.Delay(5000);

    //        retryCount++;
    //        goto RetryExtractTestResult;
    //    }
    //}

    RetryExtractTestResult:
        try
        {
            this.LogMessage = $"Extracting Attribute from #{p.ApplicationId}";

            var testResultResponse = await PratBackend.Default.Prompt(new PromptRequest()
            {
                PromptContext = promptContext,
                PromptKey = "EXTRACT_TEST_RESULT"
            });

            var testResultJo = testResultResponse.FixJson();
            var testList = testResultJo["test_results"] as JArray;
            p.Analysis.TestResults = testList.Select(item => item.ToObject<PatentAnalysis.TestResult>()).ToList();

            p.Analysis.PromptResponses[$"EXTRACT_TEST_RESULT"] = testResultResponse;
            p.Analysis.PromptOutput["EXTRACT_TEST_RESULT"] = testResultJo;
        }
        catch (Exception)
        {
            if (retryCount == 3)
            {
                throw;
            }

            this.LogMessage = $"Retrying Attribute Extraction #{p.ApplicationId}";
            await Task.Delay(5000);

            retryCount++;
            goto RetryExtractTestResult;
        }


        this.LogMessage = $"Extracting Polymer Features from #{p.ApplicationId}";
        this.StateHasChanged();

        retryCount = 0;
    RetryPolymerFeature:
        try
        {
            var polymerFeatureResponse = await PratBackend.Default.Prompt(new PromptRequest()
            {
                PromptContext = promptContext,
                PromptKey = "EXTRACT_POLYMER_FEATURE"
            });

            p.Analysis.PromptResponses["EXTRACT_POLYMER_FEATURE"] = polymerFeatureResponse;
            p.Analysis.PromptOutput["EXTRACT_POLYMER_FEATURE"] = polymerFeatureResponse.FixJson();

        }
        catch (Exception)
        {
            if (retryCount == 3)
            {
                throw;
            }

            this.LogMessage = $"Retrying Claim Extraction from #{p.ApplicationId}";
            await Task.Delay(5000);

            retryCount++;
            goto RetryPolymerFeature;
        }

        p.Analysis.IsAnalyzing = false;
        p.Analysis.IsAnalysisCompleted = true;
        this.LogMessage = $"Analysis of #{p.ApplicationId} completed";
        this.StateHasChanged();

    }

    public async Task AnalyzePatentForUI(Patent p)
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

    public async Task AnalyzePatent(Patent p)
    {
        var retryCount = 0;

        // Compare Attributes
        p.Analysis.SameAttributes = new();
        p.Analysis.MissingAttributes = new();

        foreach (var item in this.Workspace.MaterialAttributes)
        {
            var index = this.Workspace.MaterialAttributes.IndexOf(item);

        RetryComparison:
            try
            {
                this.LogMessage = $"Comparing '{item.AttributeName}' with #{p.ApplicationId} ({index + 1}/{this.Workspace.MaterialAttributes.Count})";
                this.StateHasChanged();

                var attributeAnalysisResponse = await PratBackend.Default.Prompt(new PromptRequest()
                {
                    PromptContext = JObject.FromObject(new
                    {
                        Our = item,
                        TestResults = p.Analysis.TestResults.Where(t => t.value_upper_bound != null && t.value_lower_bound != null)
                    }),
                    MaxTokens = 20,
                    PromptKey = "ANALYZE_ATTRIBUTE"
                });

                var attributeAnalysis = attributeAnalysisResponse.FixJson();

                p.Analysis.PromptResponses[$"ANALYZE_ATTRIBUTES_{index}"] = attributeAnalysisResponse;
                p.Analysis.PromptOutput[$"ANALYZE_ATTRIBUTES_{index}"] = attributeAnalysis;

                var match = attributeAnalysis.TryGetString("match");
                if (attributeAnalysis.TryGetString("match") != null)
                {
                    if (p.Analysis.TestResults.Any( t => t.attribute == match ))
                    {
                        p.Analysis.SameAttributes.Add(new AttributeAnalyis() { attribute = attributeAnalysis.TryGetString("match") });
                    }
                    else
                    {
                        p.Analysis.MissingAttributes.Add(new AttributeAnalyis() { attribute = item.AttributeName });
                    }
                }
                else
                {
                    p.Analysis.MissingAttributes.Add(new AttributeAnalyis() { attribute = item.AttributeName });
                }

            }
            catch (Exception)
            {
                if (retryCount == 3)
                {
                    throw;
                }

                this.LogMessage = $"Retrying Comparison  #{p.ApplicationId}";
                await Task.Delay(5000);

                retryCount++;
                goto RetryComparison;
            }

            retryCount = 0;
        }


    RetryApplication:
        try
        {
            this.LogMessage = $"Comparing Application with #{p.ApplicationId}";
            this.StateHasChanged();

            string applications = string.Empty;
            if (p.Analysis.PromptOutput["EXTRACT_POLYMER_FEATURE"]["application"] is JArray ja)
            {
                applications = string.Join(',', ja.Select(jt => (string)jt));
            }

            if (p.Analysis.PromptOutput["EXTRACT_POLYMER_FEATURE"]["application"].Type == JTokenType.String)
            {
                applications = (string)p.Analysis.PromptOutput["EXTRACT_POLYMER_FEATURE"]["application"];
            }

            if (string.IsNullOrEmpty(applications) == false)
            {
                var applicationAnalysisResponse = await PratBackend.Default.Prompt(new PromptRequest()
                {
                    PromptContext = JObject.FromObject(new
                    {
                        application = applications,
                        innovation_application = this.Workspace.InnovationApplication

                    }),
                    MaxTokens = 100,
                    PromptKey = "ANALYZE_SAME_APPLICATION"
                });

                var applicationAnalysis = applicationAnalysisResponse.FixJson();
                var sameList = applicationAnalysis["same"] as JArray;
                p.Analysis.SameApplications = sameList.Select(item => item.ToObject<PatentAnalysis.AttributeAnalyis>()).ToList();

                p.Analysis.PromptResponses["ANALYZE_SAME_APPLICATION"] = applicationAnalysisResponse;
                p.Analysis.PromptOutput["ANALYZE_SAME_APPLICATION"] = applicationAnalysis;

            }
        }
        catch (Exception)
        {
            if (retryCount == 3)
            {
                throw;
            }

            this.LogMessage = $"Retrying Comparison of Application  #{p.ApplicationId}";
            await Task.Delay(5000);

            retryCount++;
            goto RetryApplication;
        }
    }

    private PatentAnalysis.Claim _ActiveClaim;

    public void ClaimExplorer_SetActiveClaim( PatentAnalysis.Claim c)
    {
        _ActiveClaim = c;
        this.StateHasChanged();
    }

    public void ViewPatent(Patent p, string modal = "#viewingPatentModal")
    {
        this.ViewingPatent = p;
        _ActiveClaim = p.Analysis.Claims.FirstOrDefault();

        this.StateHasChanged();

        _ = this.JS.InvokeVoidAsync("showModal", modal);
    }

    public void LoadIframe(string iframeId, string src)
    {
        _ = this.JS.InvokeVoidAsync("navigateIframe", iframeId, src);
    }
}
