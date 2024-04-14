// webworker.js

// Setup your project to serve `py-worker.js`. You should also serve
// `pyodide.js`, and all its associated `.asm.js`, `.json`,
// and `.wasm` files as well:
importScripts("https://cdn.jsdelivr.net/pyodide/v0.25.1/full/pyodide.js");

async function loadPyodideAndPackages() {
    self.pyodide = await loadPyodide();

    console.log("Pyoide started, installing packages...");

    let mountDir = "/mnt";
    self.pyodide.FS.mkdir(mountDir);
    self.pyodide.FS.mount(self.pyodide.FS.filesystems.IDBFS, { root: "." }, mountDir);

    await self.pyodide.loadPackage("micropip");

    self.micropip = self.pyodide.pyimport("micropip");
    await self.micropip.install(['scikit-learn', 'numpy', 'scipy', 'requests'])

    console.log("Packages installed, loading processing code and models...");

    self.pyodide.runPython(`
import requests
import joblib
import numpy as np
import json

def dbscan_predict(model, X):

    nr_samples = X.shape[0]

    y_new = np.ones(shape=nr_samples, dtype=int) * -1

    for i in range(nr_samples):
        diff = model.components_ - X[i, :]  # NumPy broadcasting

        dist = np.linalg.norm(diff, axis=1)  # Euclidean distance

        shortest_dist_idx = np.argmin(dist)

        if dist[shortest_dist_idx] < model.eps:
            y_new[i] = model.labels_[model.core_sample_indices_[shortest_dist_idx]]

    return y_new

def download_file(url, destination):
    response = requests.get(url)
    if response.status_code == 200:
        with open(destination, 'wb') as f:
            f.write(response.content)
        print("File downloaded successfully.")
    else:
        print("Failed to download file. Status code:", response.status_code)

base_url = "https://storage.googleapis.com/prat-config-public/"

dbscan_file = 'model__patent_cluster_dbscan.pkl'
embedding_pca_file = 'model__patent_cluster_embeddingpca.pkl'
embedding_pca_scaler_file = 'model__patent_cluster_embeddingscaler.pkl'
visualization_pca_file = 'model__patent_cluster_visualizationpca.pkl'

toload = [dbscan_file, embedding_pca_scaler_file, embedding_pca_file, visualization_pca_file]

for file in toload:
	url = (f"{base_url}{file}").replace('model__', 'model/')
	download_file(url, file)

dbscan_model = joblib.load(dbscan_file)
embedding_pca = joblib.load(embedding_pca_file)
embedding_pca_scaler = joblib.load(embedding_pca_scaler_file)
visualization_pca = joblib.load(visualization_pca_file)

def process( input_string ):
  input = json.loads(input_string)
  embedding = input["EmbeddingVector"]
  features  = input["FeatureFlags"]

  embedding = np.reshape(embedding, (1, len(embedding)))

  embedding_transformed = embedding_pca.transform(embedding)
  embedding_scaled = embedding_pca_scaler.transform(embedding_transformed)

  features = np.concatenate([embedding_scaled[0], features])
  dbscan_input = np.reshape(features, (1, len(features)))
  
  predicted_cluster = dbscan_predict(dbscan_model, dbscan_input)
  visualization_coord = visualization_pca.transform(dbscan_input)[0]
  
  return {
  	"cluster" : predicted_cluster,
    "visualization_coord" : visualization_coord
  }

import js
js.clusterPredictor = process;

  `);

    self.clusterPredictor = clusterPredictor
    console.log("Cluster Predictor Loaded:" + self.clusterPredictor)

}
let pyodideReadyPromise = loadPyodideAndPackages();

self.onmessage = async (event) => {
    // make sure loading is done
    await pyodideReadyPromise;

    const { id, input } = event.data;

    // Now is the easy part, the one that is similar to working in the main thread:
    try {
        let results = self.clusterPredictor(input).toJs();

        let cluster = results.get("cluster")[0];
        let visualization_coord = results.get("visualization_coord");

        let result = {
            cluster,
            visualization_coord
        };

        console.log("Predictor Ran:");
        console.log(result);

        self.postMessage({ result, id });
    } catch (error) {
        self.postMessage({ error: error.message, id });
    }
};