using UnityEngine;
using Unity.Sentis; 
using System.Collections.Generic;
using Meta.XR; 

public class YoloInferenceManager : MonoBehaviour
{
    [Header("AI Assets")]
    public ModelAsset modelAsset;
    
    private Model runtimeModel;
    private Worker worker;
    // Explicitly use Tensor<float> as requested by the compiler
    private Tensor<float> inputTensor; 

    private PassthroughCameraAccess cameraAccess;
    private YoloVisualizer visualizer;

    void Start()
    {
        if (modelAsset == null) return;
        runtimeModel = ModelLoader.Load(modelAsset);
        worker = new Worker(runtimeModel, BackendType.GPUCompute);
        
        // Initialize the 'bucket' once to avoid the Obsolete warning
        inputTensor = new Tensor<float>(new TensorShape(1, 3, 640, 640));
        
        visualizer = GetComponent<YoloVisualizer>();
        cameraAccess = FindFirstObjectByType<PassthroughCameraAccess>();
    }

    void Update()
    {
        if (cameraAccess != null && cameraAccess.IsPlaying && cameraAccess.IsUpdatedThisFrame)
        {
            Texture cameraTexture = cameraAccess.GetTexture();
            if (cameraTexture != null)
            {
                ProcessFrame(cameraTexture);
            }
        }
    }

    public void ProcessFrame(Texture cameraFrame)
    {
        if (worker == null || cameraFrame == null) return;

        // Use the new non-obsolete method: fill the existing inputTensor
        TextureConverter.ToTensor(cameraFrame, inputTensor, new TextureTransform());
        
        worker.Schedule(inputTensor);

        // Peek the output and ensure it matches the Visualizer's expected type
        var output = worker.PeekOutput() as Tensor<float>;
        if (output != null && visualizer != null) 
        {
            visualizer.UpdateBoxes(output); 
        }
    }

    void OnDestroy()
    {
        worker?.Dispose();
        inputTensor?.Dispose();
    }
}