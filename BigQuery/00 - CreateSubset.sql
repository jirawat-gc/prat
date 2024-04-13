CREATE TABLE raw_patents.thailand_dip_subset_10 (
  ApplicationId INTEGER,
  Title String,
  PatentClaims String
);

INSERT INTO raw_patents.thailand_dip_subset_10
  SELECT ApplicationId, Title, PatentClaims FROM raw_patents.thailand_dip TABLESAMPLE SYSTEM (1 PERCENT)