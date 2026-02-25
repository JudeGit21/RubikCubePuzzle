using UnityEngine;
using Unity.Sentis;
using Meta.XR.MRUtilityKit;
using Meta.XR; 

public class RubikDetectorPro : MonoBehaviour
{
    [Header("AI Configuration")]
    public ModelAsset yoloModel;
    [Range(0, 1)] public float confidenceThreshold = 0.5f;

    [Header("Instructor Settings")]
    public GameObject digitalCubePrefab;
    [Tooltip("How high (in meters) the digital cube floats above the real one")]
    public float hoverHeight = 0.25f; 
    [Tooltip("How smoothly the cube follows your hand (1 = slow, 20 = instant)")]
    public float smoothSpeed = 10f;

    private GameObject activeCube;
    private Worker worker;
    private Tensor<float> inputTensor;
    private PassthroughCameraAccess cameraAccess;

    void Start()
    {
        var model = ModelLoader.Load(yoloModel);
        worker = new Worker(model, BackendType.GPUCompute);
        inputTensor = new Tensor<float>(new TensorShape(1, 3, 640, 640));

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
        TextureConverter.ToTensor(sourceTexture, inputTensor, new TextureTransform().SetDimensions(640, 640));
        worker.Schedule(inputTensor);

        var output = worker.PeekOutput() as Tensor<float>;
        if (output == null) return;

        float[] data = output.DownloadToArray();
        int numCandidates = 8400;
        
        float bestScore = confidenceThreshold;
        float bestX = -1, bestY = -1;

        for (int i = 0; i < numCandidates; i++)
        {
            float score = data[4 * numCandidates + i]; 
            if (score > bestScore)
            {
                bestScore = score;
                bestX = data[0 * numCandidates + i] / 640f; 
                bestY = data[1 * numCandidates + i] / 640f;
            }
        }

        if (bestX != -1) PlaceCube(bestX, bestY);
    }

    // --- THE UPDATED FUNCTION ---
    void PlaceCube(float x, float y)
    {
        // 1. Create a Ray from the physical RGB cameras using Meta's specialized PCA API
        // This accounts for the physical offset of the Quest 3 lenses.
        Ray ray = cameraAccess.ViewportPointToRay(new Vector2(x, 1 - y));

        if (MRUK.Instance?.GetCurrentRoom() != null)
        {
            // 2. Check the MRUK room data to see where that Ray hits your hand/table
            if (MRUK.Instance.GetCurrentRoom().Raycast(ray, 10f, out RaycastHit hit))
            {
                if (activeCube == null) 
                    activeCube = Instantiate(digitalCubePrefab);

                // 3. THE FLOATING LOGIC:
                // We take the hit point (the real cube) and add the hoverHeight (going UP).
                Vector3 targetPosition = hit.point + (Vector3.up * hoverHeight);

                // 4. THE SMOOTHING LOGIC (Lerp):
                // Instead of snapping, we glide. This stops the digital cube from vibrating 
                // if the AI is slightly jumpy.
                activeCube.transform.position = Vector3.Lerp(activeCube.transform.position, targetPosition, Time.deltaTime * smoothSpeed);
                
                // 5. THE BILLBOARDING LOGIC:
                // This makes the digital cube rotate to face YOU so you can read the moves easily.
                Vector3 lookDirection = Camera.main.transform.position - activeCube.transform.position;
                activeCube.transform.rotation = Quaternion.LookRotation(-lookDirection);
            }
        }
    }

    private void OnDisable()
    {
        worker?.Dispose();
        inputTensor?.Dispose();
    }
}