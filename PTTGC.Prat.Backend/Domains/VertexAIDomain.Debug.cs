namespace PTTGC.Prat.Backend.Domains;

public static partial class VertexAIDomain
{
    private static void AddPromptsPOC(Dictionary<string, string> promptDict)
    {
        promptDict["DEBUG"] = "{{prompt_text}}";

        promptDict["EXTRACT_INNOVATION_FEATURES"] = """
            Title: {{InnovationTitle}}
            Content:
            {{InnovationDescription}}
            Application:
            {{InnovationApplication}}
            {{#if InnovationIncludeTestResults}}
                The invention has following attributes:
                {{#each MaterialAttributes}}
                - {{AttributeName}}: {{LowerBound}}{{#if UpperBound}}- {{UpperBound}}{{/if}} {{MeasurementUnit}}
                {{/each}}
            {{/if}}
            {{#if InnovationIsPolymer}}
            - Polymer Kind: {{InnovationPolymerKind}}
            - Comonomer: {{InnovationComonomer}}
            {{/if}}

            ## Above is information about patent claim. Take a deep breath to consider both Title and Content then answer in this JSON format (without Markdown):
            {
              "summary" : describe the patent claim in 3 sentences,
              "chemical" : if patent claim is related to chemical,
              "polymer" : if patent claim is related to polymer,
              "home_chemical" : if patent claim is related to chemical for home use,
              "industrial_chemical" : if patent claim is related to chemical for industrial use,
              "argriculture" : if patent claims is about material, product or process related to argriculture,
              "automotive" : if patent claims is about material, product or process related to automotive industry ,
              "bottle_caps" : if patent claims is about material, product or process related to production of bottle caps,
              "bottle" : if patent claims is about material, product or process related to production of bottle,
              "paint" : if patent claims is about material, product or process related to paint,
              "coating" : if patent claims is about material, product or process related to surface coating,
              "home_appliances" : if patent claims is about material, product or process related to home appliances,
              "epoxy_composite" : if patent claims is about material, product or process related to epoxy composite,
              "packaging" : if patent claims is about material, product or process related to packaging,
              "large_blow" : if patent claims is about material, product or process related to lage extrusion (large blow) products such as: barrel drum, spray tank, pontoon, bulk containers ,
              "non_woven" : if patent claims is about material, product or process related to non-woven products ,
              "pipe" : if patent claims is about material, product or process related to pipes, such as: gas pipes  liquid pipes, conduit pipes,
              "pipe_fittings" : if patent claims is about material, product or process related to pipe fittings,
              "wire" : if patent claims is about material, product or process related to wires and cables,
              "has_chemical_process" : is patent claims mentioned chemical process ,
              "has_manufacturing_process" : is patent claims mentioned manufacturing process ,
            }

        """;

        promptDict["EXTRACT_MAIN_CLAIM"] = """
            Title: {{Title}}
            Content:
            {{PatentClaims}}

            ## Above is content from patent claims and title. Take a deep breath to consider both Title and Content before answering.
            "Standalone Claim" are claim points in Patent Claims where the point does not referring to any other point.
            Answer in this JSON format:
            {
                "standalone_claims" : [
                    List any and all Standalone Claim present in patent claims in this format:
                    {
                        "claim" : standalone claim content shorten to 2 sentences,
                        "point_index" : claim number of this standalone claim as mentioned in content,
                        "revision" : if the patent claim contains multiple revisions, specify the revision or date where this standalone claim is presented
                    }
                ]
            }
        """;

        promptDict["EXTRACT_TEST_RESULT"] = """
            Title: {{Title}}
            Content:
            {{PatentClaims}}

        
            ## Above is content from patent claims and title. Take a deep breath to consider both Title and Content before answering.
            Answer in this JSON format:
            {
        	    "test_results" : [
        		    List any and all material test results, such as: melting point, density, molecular weight, in this format:
        		    {
        			    "test" : name of the test, such as: CDBI,
        			    "condition": testing condition, for example: 'Melt flow rate (MFR; at 190C, 2.16 kg)' - the answer is: 'at 190c, 2.16kg' or 'Melt flow rate (MFR; I2.16)' - the answer is: 'I2.16'
        			    "value_lower_bound": lower bound result of the test, for example: if result is '0.930 to 0.950 g/10 min' - answer '0.930'. if the value is not a range, such as: 70%, answer with that value,
        			    "value_upper_bound": upper bound result of the test, for example: if result is '0.930 to 0.950 g/10 min' - answer '0.950'. if the value is not a range, such as: 70%, answer with that value,
        			    "unit" : unit of the rest result, for example: if result is '0.930 to 0.950 g/10 min' - answer 'g/10 min',
        			    "revision" : if the patent claim contains multiple revisions, specify the revision or date where this test result is presented,
        		    }
        	    ]
            }
        """;

        promptDict["EXTRACT_POLYMER_FEATURE"] = """
            Title: {{Title}}
            Content:
            {{PatentClaims}}

            ## Above is content from patent claims and title. Take a deep breath to consider both Title and Content before answering.
            Do not include markdown in your answer.
            Answer in this JSON format:
            {
                "is_olefins" : if patent claim is about olefins or mentioned olefins,
                "is_aromatics" : if patent claim is about aromatics or mentioned aromatics,
                "is_polyolefins" : if patent claim is about polyolefins or mentioned polyolefins,
                "is_polyethylene" : if patent claim is about polyethylene or mentioned polyethylene,
                "base_of_polymer" : base of polymer mentioned in patent claim,
                "comonomer" : Comonomer mentioned in patent claim,
                "application": application of product or process as mentioned in patent claim, such as: film, bottle caps, coating
            }
        """;

    }
}
