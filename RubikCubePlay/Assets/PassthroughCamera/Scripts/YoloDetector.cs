using UnityEngine;
using Unity.Sentis;

public class YoloDetector : MonoBehaviour {
    [Header("AI Assets")]
    public ModelAsset yoloModel;
    
    private Worker worker;
    private Tensor<float> inputTensor; // Reusable memory container

    void Start() {
        var model = ModelLoader.Load(yoloModel);
        worker = new Worker(model, BackendType.GPUCompute);
        
        // Pre-allocate a 640x640 RGB tensor so we don't create it every frame
        inputTensor = new Tensor<float>(new TensorShape(1, 3, 640, 640));
    }

    public void DetectCube(Texture cameraFrame) {
        if (cameraFrame == null || worker == null) return;

        // 1. New 2026 way to convert texture to tensor
        // This is much faster for the Quest 3 GPU
        var transform = new TextureTransform().SetDimensions(640, 640, 3);
        TextureConverter.ToTensor(cameraFrame, inputTensor, transform);
        
        // 2. Run the AI
        worker.Schedule(inputTensor);

        // 3. Peek the output (TensorFloat is now Tensor<float>)
        var output = worker.PeekOutput() as Tensor<float>;
        
        if (output != null) {
            // Your AI is officially thinking!
            // Logic for Rubik's stickers goes here.
        }
    }

    private void OnDisable() {
        worker?.Dispose();
        inputTensor?.Dispose(); // Clean up GPU memory
    }
}