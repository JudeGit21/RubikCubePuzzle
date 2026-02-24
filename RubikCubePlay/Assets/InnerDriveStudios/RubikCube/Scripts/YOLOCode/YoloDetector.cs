using UnityEngine;
using Unity.Sentis;

public class YoloDetector : MonoBehaviour
{
    [Header("AI Assets")]
    public ModelAsset yoloModel;

    [Header("Visualizer (optional)")]
    public YoloVisualizer visualizer; // <--- ADD THIS

    private Worker worker;
    private Tensor<float> inputTensor;

    void Start()
    {
        var model = ModelLoader.Load(yoloModel);
        worker = new Worker(model, BackendType.GPUCompute);

        // Pre-allocate 640x640 tensor
        inputTensor = new Tensor<float>(new TensorShape(1, 3, 640, 640));
    }

    public void DetectCube(Texture cameraFrame)
    {
        Debug.Log($"[YoloDetector] Input tensor created: {inputTensor != null}, shape: {inputTensor.shape}");
        if (cameraFrame == null || worker == null) return;

        // Convert texture to tensor
        var transform = new TextureTransform().SetDimensions(640, 640, 3);
        TextureConverter.ToTensor(cameraFrame, inputTensor, transform);

        // Schedule inference (async)
        worker.Schedule(inputTensor);

        // Get output immediately (async safe in 2026)
        var output = worker.PeekOutput() as Tensor<float>;



        if (output != null && visualizer != null)
        {
            visualizer.UpdateBoxes(output);
        }
    }

    private void OnDisable()
    {
        worker?.Dispose();
        inputTensor?.Dispose();
    }
}