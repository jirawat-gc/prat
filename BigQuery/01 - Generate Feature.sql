-- Check abstract
-- Polyolefins
-- Polyethylene (High density, Low Density) - 95% is PE
-- Polymerization Flag
-- Catalyst for Polymer
-- Check for Keywords send by user

INSERT INTO patents_processed.thailand_dip_1_promptlog
  SELECT ApplicationId, Title, prompt As Prompt, ml_generate_text_llm_result As Response, ml_generate_text_status As Status, CURRENT_TIMESTAMP() as Timestamp, 'TEST-20240407' As JobId FROM
    ML.GENERATE_TEXT( MODEL `curious-pointer-419406.raw_patents.gemini`,
      ( SELECT ApplicationId, Title,
              (
                'Title:' || Title ||
                '\n' ||
                'Content:' ||
                '\n' ||
                PatentClaims ||
                '\n\n'|| 
                '## Above is content from patent claims and title. Take a deep breath to consider both Title and Content then answer in this JSON format: {'||
                '"chemical" : if patent claim is related to chemical,' ||
                '"polymer" : if patent claim is related to polymer,' ||
                '"home_chemical" : if patent claim is related to chemical for home use,' ||
                '"industrial_chemical" : if patent claim is related to chemical for industrial use,' ||
                '"argriculture" : if patent claims is about material, product or process related to argriculture,' ||
                '"automotive" : if patent claims is about material, product or process related to automotive industry ,' ||
                '"bottle_caps" : if patent claims is about material, product or process related to production of bottle caps,' ||
                '"bottle" : if patent claims is about material, product or process related to production of bottle,' ||
                '"paint" : if patent claims is about material, product or process related to paint,' ||
                '"coating" : if patent claims is about material, product or process related to surface coating,' ||
                '"home_appliances" : if patent claims is about material, product or process related to home appliances,' ||
                '"epoxy_composite" : if patent claims is about material, product or process related to epoxy composite,' ||
                '"packaging" : if patent claims is about material, product or process related to packaging,' ||
                '"large_blow" : if patent claims is about material, product or process related to lage extrusion (large blow) products such as: barrel drum, spray tank, pontoon, bulk containers ,' ||
                '"non_woven" : if patent claims is about material, product or process related to non-woven products ,' ||
                '"pipe" : if patent claims is about material, product or process related to pipes, such as: gas pipes  liquid pipes, conduit pipes,' ||
                '"pipe_fittings" : if patent claims is about material, product or process related to pipe fittings,' ||
                '"wire" : if patent claims is about material, product or process related to wires and cables,' ||
                '"has_chemical_process" : is patent claims mentioned chemical process ,' ||
                '"has_manufacturing_process" : is patent claims mentioned manufacturing process ,' ||
                --'"product" : array of products mentioned in patent claim translated to english,' || This will usually make response broken
                '"summary" : "describe the patent claim in 3 sentences",' ||
                '}'
              )
                as prompt
        FROM `raw_patents.thailand_dip_subset_1percent`
      ),
      STRUCT(0 AS temperature, 3 As top_k, 1000 as max_output_tokens, TRUE AS flatten_json_output));

