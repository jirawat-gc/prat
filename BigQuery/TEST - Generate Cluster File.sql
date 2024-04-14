SELECT
  raw.*,
  emb.Summary,
  TO_JSON_STRING(emb.SummaryEmbedding) As SummaryEmbeddingJson
FROM
  raw_patents.thailand_dip raw INNER JOIN
  `patents_processed.thailand_dip_3_embeddings` emb
  ON CAST(raw.ApplicationId As String) = emb.ApplicationId
WHERE
  raw.ApplicationId IN
    (SELECT
      ApplicationId
    FROM
      `curious-pointer-419406.patents_processed.thailand_dip_4_clusters`,
      UNNEST(PatentApplicationIds) As ApplicationId
    WHERE 
      ClusterLabel = -1)