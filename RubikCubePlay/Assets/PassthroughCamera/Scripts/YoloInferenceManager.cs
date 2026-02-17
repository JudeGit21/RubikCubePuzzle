using UnityEngine;
using Unity.Sentis; 

public class YoloInferenceManager : MonoBehaviour
{
    [Header("AI Assets")]
    public ModelAsset modelAsset;
    
    private Model runtimeModel;
    private Worker worker;
    private Tensor<float> inputTensor; 

    // Reference to the visualizer component
    private YoloVisualizer visualizer;

    void Start()
    {
        if (modelAsset == null) {
            Debug.LogError("No ModelAsset assigned! Drag yolov8n.onnx into the Inspector.");
            return;
        }

        // Get the visualizer component attached to the same object
        visualizer = GetComponent<YoloVisualizer>();

        runtimeModel = ModelLoader.Load(modelAsset);
        worker = new Worker(runtimeModel, BackendType.GPUCompute);
        inputTensor = new Tensor<float>(new TensorShape(1, 3, 640, 640));
        
        Debug.Log("YoloInferenceManager: AI Pipeline Initialized.");
    }

    public void ProcessFrame(Texture2D cameraFrame)
    {
        if (worker == null || cameraFrame == null) return;

        // 1. Convert pixels to Tensor
        TextureConverter.ToTensor(cameraFrame, inputTensor, new TextureTransform());
        
        // 2. Run the AI model
        worker.Schedule(inputTensor);

        // 3. Get the raw AI output
        var output = worker.PeekOutput() as Tensor<float>;
        
        // 4. Send the output to the Visualizer to draw the neon boxes
        if (output != null && visualizer != null) 
        {
            visualizer.UpdateVisuals(output); 
        }
    }

    void OnDestroy()
    {
        worker?.Dispose();
        inputTensor?.Dispose();
    }
}