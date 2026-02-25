using UnityEngine;
using Unity.Sentis;
using Meta.XR.MRUtilityKit;
using Meta.XR; 

public class RubikDetectorPro : MonoBehaviour
{
    [Header("AI Configuration")]
    public ModelAsset yoloModel;
    // We set the default to 0.75 for better stability
    [Range(0, 1)] public float confidenceThreshold = 0.75f;

    [Header("Instructor Settings")]
    public GameObject digitalCubePrefab;
    [Tooltip("How high (in meters) the digital cube floats above the real one")]
    public float hoverHeight = 0.25f; 
    [Tooltip("How smoothly the cube follows your hand (1 = slow, 20 = instant)")]
    public float smoothSpeed = 12f;

    private GameObject activeCube;
    private Worker worker;
    private Tensor<float> inputTensor;
    private PassthroughCameraAccess cameraAccess;

    void Start()
    {
        // Initialize Sentis Worker
        var model = ModelLoader.Load(yoloModel);
        worker = new Worker(model, BackendType.GPUCompute);
        inputTensor = new Tensor<float>(new TensorShape(1, 3, 640, 640));

        // Find the Meta Camera Access component
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
        // Resize and convert texture to Tensor
        TextureConverter.ToTensor(sourceTexture, inputTensor, new TextureTransform().SetDimensions(640, 640));
        worker.Schedule(inputTensor);

        var output = worker.PeekOutput() as Tensor<float>;
        if (output == null) return;

        float[] data = output.DownloadToArray();
        int numCandidates = 8400;
        
        float bestScore = confidenceThreshold;
        float bestX = -1, bestY = -1;

        // Parse YOLOv8 output
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

        // Only update if detection is valid
        if (bestX != -1) 
        {
            PlaceCube(bestX, bestY);
        }
        else if (activeCube != null)
        {
            // Optional: You could hide the cube if the AI loses sight of it
            // activeCube.SetActive(false);
        }
    }

    void PlaceCube(float x, float y)
    {
        // 1. Get Ray from physical camera lenses
        Ray ray = cameraAccess.ViewportPointToRay(new Vector2(x, 1 - y));

        if (MRUK.Instance?.GetCurrentRoom() != null)
        {
            // 2. Raycast against the real world (hand/table)
            if (MRUK.Instance.GetCurrentRoom().Raycast(ray, 10f, out RaycastHit hit))
            {
                if (activeCube == null) 
                {
                    // Ensure the cube is spawned active
                    activeCube = Instantiate(digitalCubePrefab);
                }
                
                activeCube.SetActive(true);

                // 3. SMOOTH POSITION: Floating logic
                Vector3 targetPosition = hit.point + (Vector3.up * hoverHeight);
                activeCube.transform.position = Vector3.Lerp(activeCube.transform.position, targetPosition, Time.deltaTime * smoothSpeed);
                
                // 4. STABLE ROTATION: Faces the player (Billboarding)
                // This prevents the cube from spinning wildly while you try to read moves
                Vector3 directionToPlayer = Camera.main.transform.position - activeCube.transform.position;
                directionToPlayer.y = 0; // Lock the Y axis so the cube stays upright
                if (directionToPlayer != Vector3.zero)
                {
                    activeCube.transform.rotation = Quaternion.Slerp(activeCube.transform.rotation, Quaternion.LookRotation(-directionToPlayer), Time.deltaTime * smoothSpeed);
                }
            }
        }
    }

    private void OnDisable()
    {
        worker?.Dispose();
        inputTensor?.Dispose();
    }
}