DECLARE last_count DEFAULT 0;
DECLARE job_id DEFAULT "job202404090913";

SET (last_count) = (
    SELECT AS STRUCT Count(1) FROM `curious-pointer-419406.patents_processed.thailand_dip_1_promptlog`
    WHERE
      JobId = job_id AND
      Response Is Not Null AND
      CAST( ApplicationId as STRING) Not In ( SELECT ApplicationId FROM patents_processed.thailand_dip_3_embeddings WHERE JobId = job_id) AND
      JSON_EXTRACT_SCALAR(REPLACE(Response, "```json", ""), "$.summary") IS NOT NULL
);

LOOP

  SELECT "Remaining: " || last_count;
  
  IF last_count <= 0 THEN
    LEAVE;
  END IF;

  INSERT INTO patents_processed.thailand_dip_3_embeddings
    SELECT ApplicationId, Title, content As Summary, ml_generate_embedding_result As SummaryEmbedding, JobId
    FROM
    ML.GENERATE_EMBEDDING(
      MODEL  `curious-pointer-419406.raw_patents.textembedding-gecko` ,
      (
        SELECT
          JobId,
          CAST( ApplicationId as STRING) As ApplicationId,
          Title,
          JSON_EXTRACT_SCALAR(REPLACE(Response, "```json", ""), "$.summary") As content,
        FROM `curious-pointer-419406.patents_processed.thailand_dip_1_promptlog` 
        WHERE
          JobId = job_id AND
          Response Is Not Null AND
          CAST( ApplicationId as STRING) Not In ( SELECT ApplicationId FROM patents_processed.thailand_dip_3_embeddings WHERE JobId = job_id) AND
          JSON_EXTRACT_SCALAR(REPLACE(Response, "```json", ""), "$.summary") IS NOT NULL

        LIMIT 100
      ),
      STRUCT(TRUE AS flatten_json_output, 'RETRIEVAL_DOCUMENT' as task_type)
    );

  SET last_count = last_count - 100;

  END LOOP