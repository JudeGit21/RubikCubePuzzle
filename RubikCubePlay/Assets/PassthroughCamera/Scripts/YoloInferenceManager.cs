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
    private Tensor<float> inputTensor; 

    private PassthroughCameraAccess cameraAccess;
    private YoloVisualizer visualizer;

    void Start()
    {
        Debug.Log("AI_Brain: Starting initialization...");

        if (modelAsset == null)
        {
            Debug.LogError("AI_Brain: No ModelAsset assigned in the Inspector!");
            return;
        }

        try 
        {
            // 1. Setup Sentis Pipeline
            runtimeModel = ModelLoader.Load(modelAsset);
            worker = new Worker(runtimeModel, BackendType.GPUCompute);
            
            // Initialize the input tensor 'bucket' (1 batch, 3 channels, 640x640)
            inputTensor = new Tensor<float>(new TensorShape(1, 3, 640, 640));
            
            // 2. Locate Components
            visualizer = GetComponent<YoloVisualizer>();
            cameraAccess = FindFirstObjectByType<PassthroughCameraAccess>();

            // 3. Find and Color the Child Cube
            Transform childCube = transform.Find("Cube"); 
            if (childCube != null)
            {
                // Set the color to GREEN to show the AI logic is loaded
                childCube.GetComponent<Renderer>().material.color = Color.green;
                Debug.Log("AI_Brain: Found child cube and set color to Green.");
            }
            else
            {
                Debug.LogWarning("AI_Brain: Could not find a child object named 'Cube'!");
            }

            Debug.Log("AI_Brain: Worker created and pipeline ready.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"AI_Brain: Error during Start: {e.Message}");
        }
    }

    void Update()
    {
        // Check if the Passthrough script has a new frame ready for us
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

        // Fill the inputTensor with pixels from the camera
        TextureConverter.ToTensor(cameraFrame, inputTensor, new TextureTransform());
        
        // Execute the AI model
        worker.Schedule(inputTensor);

        // Send output to the visualizer
        var output = worker.PeekOutput() as Tensor<float>;
        if (output != null && visualizer != null) 
        {
            visualizer.UpdateVisuals(output); 
        }
    }

    void OnDestroy()
    {
        // Clean up memory
        worker?.Dispose();
        inputTensor?.Dispose();
    }
}