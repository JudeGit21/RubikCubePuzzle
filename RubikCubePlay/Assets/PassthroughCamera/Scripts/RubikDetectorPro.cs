using UnityEngine;
using Unity.Sentis;
using Meta.XR.MRUtilityKit;
using Meta.XR; // Required for PassthroughCameraAccess

public class RubikDetectorPro : MonoBehaviour
{
    [Header("AI Configuration")]
    public ModelAsset yoloModel;
    public float confidenceThreshold = 0.5f;

    [Header("World Objects")]
    public GameObject digitalCubePrefab;
    private GameObject activeCube;

    private Worker worker;
    private Tensor<float> inputTensor;
    private PassthroughCameraAccess cameraAccess;

    void Start()
    {
        // 1. Initialize AI
        var model = ModelLoader.Load(yoloModel);
        worker = new Worker(model, BackendType.GPUCompute);
        // Standard YOLOv8 shape (Batch, Channels, Height, Width)
        inputTensor = new Tensor<float>(new TensorShape(1, 3, 640, 640));

        // 2. Find the camera "Eyes"
        cameraAccess = FindFirstObjectByType<PassthroughCameraAccess>();
        
        if (cameraAccess == null)
            Debug.LogError("RubikDetector: PassthroughCameraAccess missing from Scene!");
    }

    void Update()
    {
        if (cameraAccess != null && cameraAccess.IsPlaying)
        {
            Texture frame = cameraAccess.GetTexture();
            if (frame != null) RunInference(frame);
        }
    }

    void RunInference(Texture sourceTexture)
    {
        // 1. Texture to Tensor (Sentis 2.1 syntax)
        // This resizes your camera frame to 640x640 for the AI
        TextureConverter.ToTensor(sourceTexture, inputTensor, new TextureTransform().SetDimensions(640, 640));

        // 2. Execute AI
        worker.Schedule(inputTensor);

        // 3. Get Output
        var output = worker.PeekOutput() as Tensor<float>;
        if (output == null) return;

        // 4. Parse YOLOv8 Data (Shape: 1, 84, 8400)
        float[] data = output.DownloadToArray();
        int numCandidates = 8400;
        
        float bestScore = confidenceThreshold;
        float bestX = -1, bestY = -1;

        for (int i = 0; i < numCandidates; i++)
        {
            // Class 0 score is at index [4 * 8400 + i] in a standard YOLOv8 export
            float score = data[4 * numCandidates + i]; 
            if (score > bestScore)
            {
                bestScore = score;
                // Get normalized coordinates (0 to 1)
                bestX = data[0 * numCandidates + i] / 640f; 
                bestY = data[1 * numCandidates + i] / 640f;
            }
        }

        // 5. Move the Cube
        if (bestX != -1) PlaceCube(bestX, bestY);
    }

    void PlaceCube(float x, float y)
    {
        // UPDATED LINE 82: 
        // Using Meta's specialized raycaster for Passthrough alignment
        // Viewport is 0-1. We use '1 - y' because screen vs texture coordinates are flipped.
        Ray ray = cameraAccess.ViewportPointToRay(new Vector2(x, 1 - y));

        if (MRUK.Instance?.GetCurrentRoom() != null)
        {
            // MRUK Environment Raycast checks against your scanned room walls/furniture
            if (MRUK.Instance.GetCurrentRoom().Raycast(ray, 10f, out RaycastHit hit))
            {
                if (activeCube == null) 
                    activeCube = Instantiate(digitalCubePrefab);

                // Lock digital cube to the real world position detected by MRUK
                activeCube.transform.position = hit.point;
                
                // Align to surface normal (so it sits flat on a table)
                activeCube.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            }
        }
    }

    private void OnDisable()
    {
        worker?.Dispose();
        inputTensor?.Dispose();
    }
}