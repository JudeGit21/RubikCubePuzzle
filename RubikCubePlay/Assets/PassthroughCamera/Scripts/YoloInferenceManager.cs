using UnityEngine;
using Unity.Sentis; // This MUST be present
using System.Collections.Generic;
using Meta.XR; 

public class YoloInferenceManager : MonoBehaviour
{
    public ModelAsset modelAsset;
    public GameObject boxPrefab;
    private Model runtimeModel;
    private Worker worker; 
    private List<GameObject> activeBoxes = new List<GameObject>();

    void Start()
    {
        if (modelAsset == null) return;

        runtimeModel = ModelLoader.Load(modelAsset);
        // Create the engine
        worker = new Worker(runtimeModel, BackendType.GPUCompute);

        var pca = FindFirstObjectByType<PassthroughCameraAccess>();
        if (pca != null) pca.OnFrameReceived += OnCameraFrameReceived;
    }

    public void OnCameraFrameReceived(Texture2D cameraTexture)
    {
        if (worker == null) return;

        // TextureConverter handles the TensorFloat creation internally
        using Tensor inputTensor = TextureConverter.ToTensor(cameraTexture, 640, 640, 3);
        worker.Schedule(inputTensor);

        // We cast the output to TensorFloat to read the numbers
        TensorFloat output = worker.PeekOutput() as TensorFloat;
        if (output != null) ProcessDetections(output);
    }

    void ProcessDetections(TensorFloat output)
    {
        foreach (var box in activeBoxes) Destroy(box);
        activeBoxes.Clear();

        // This moves the data from GPU to CPU so we can read it in C#
        output.MakeReadable(); 
        float[] results = output.ToReadOnlyArray();

        if (results.Length > 0 && results[0] > 0.3f)
        {
            GameObject box = Instantiate(boxPrefab, new Vector3(0, 0, 1.5f), Quaternion.identity);
            activeBoxes.Add(box);
        }
    }

    void OnDisable()
    {
        worker?.Dispose();
    }
}