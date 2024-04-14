const pyodideWorker = new Worker("/pyoide/webworker.js");

const callbacks = { };

pyodideWorker.onmessage = (event) => {
    const { id, result } = event.data;
    const onSuccess = callbacks[id];
    delete callbacks[id];

    onSuccess(JSON.stringify(event.data));
};

const runPredictor = (() => {
    let id = 0; // identify a Promise
    return (input) => {
        // the id could be generated more carefully
        id = (id + 1) % Number.MAX_SAFE_INTEGER;
        return new Promise((onSuccess) => {
        callbacks[id] = onSuccess;
        pyodideWorker.postMessage({
            input,
            id,
      });
    });
};
})();

export { runPredictor };