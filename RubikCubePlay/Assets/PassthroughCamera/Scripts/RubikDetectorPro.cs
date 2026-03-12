using UnityEngine;
using Unity.Sentis;
using Meta.XR.MRUtilityKit;
using Meta.XR;

public class RubikDetectorPro : MonoBehaviour
{
    [Header("AI Configuration")]
    public ModelAsset yoloModel;
    [Range(0, 1)] public float confidenceThreshold = 0.40f;

    [Header("Instructor Settings")]
    public GameObject digitalCubePrefab;
    public float hoverHeight = 0.25f;
    public float smoothSpeed = 12f;

    [Header("Debug")]
    public Vector4 lastDetection;

    private GameObject activeCube;
    private Worker worker;
    private Tensor<float> inputTensor;
    private PassthroughCameraAccess cameraAccess;
    private Camera vrCamera;

    void Start()
    {
        Debug.Log("RubikDetectorPro starting...");

        // Load YOLO model
        try
        {
            var model = ModelLoader.Load(yoloModel);
            worker = new Worker(model, BackendType.GPUCompute);
            inputTensor = new Tensor<float>(new TensorShape(1, 3, 640, 640));

            Debug.Log("YOLO model loaded successfully.");
        }
        catch
        {
            Debug.LogError("Failed to load YOLO model!");
        }

        // Find passthrough camera
        cameraAccess = FindFirstObjectByType<PassthroughCameraAccess>();

        if (cameraAccess == null)
            Debug.LogError("PassthroughCameraAccess NOT found!");
        else
            Debug.Log("Passthrough camera connected.");

        // Find headset camera
        vrCamera = Camera.main;

        if (vrCamera == null)
            vrCamera = FindFirstObjectByType<Camera>();

        if (vrCamera != null)
            Debug.Log("VR Camera found.");
        else
            Debug.LogError("VR Camera NOT found!");

        // Spawn instructor cube
        if (digitalCubePrefab != null)
        {
            activeCube = Instantiate(digitalCubePrefab);

            activeCube.name = "INSTRUCTOR_CUBE";
            activeCube.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

            activeCube.SetActive(false);

            Debug.Log("Instructor cube created.");

            // Disable legacy rotator scripts
            MonoBehaviour[] scripts = activeCube.GetComponentsInChildren<MonoBehaviour>();

            foreach (var s in scripts)
            {
                if (s.GetType().Name.Contains("Rotator"))
                    s.enabled = false;
            }
        }
        else
        {
            Debug.LogError("Digital Cube Prefab is not assigned!");
        }
    }

    void Update()
    {
        if (cameraAccess == null)
            return;

        if (!cameraAccess.IsPlaying)
            return;

        Texture frame = cameraAccess.GetTexture();

        if (frame == null)
            return;

        Debug.Log("Camera frame received.");

        RunInference(frame);
    }

    void RunInference(Texture sourceTexture)
    {
        TextureConverter.ToTensor(sourceTexture, inputTensor,
            new TextureTransform().SetDimensions(640, 640));

        worker.Schedule(inputTensor);

        var output = worker.PeekOutput() as Tensor<float>;

        if (output == null)
            return;

        float[] data = output.DownloadToArray();

        int numCandidates = 8400;

        float bestScore = confidenceThreshold;
        float bestX = -1;
        float bestY = -1;
        float bestW = 0;
        float bestH = 0;

        for (int i = 0; i < numCandidates; i++)
        {
            float score = data[4 * numCandidates + i];

            if (score > bestScore)
            {
                Debug.Log("Detection score: " + score);

                bestScore = score;

                bestX = data[0 * numCandidates + i] / 640f;
                bestY = data[1 * numCandidates + i] / 640f;
                bestW = data[2 * numCandidates + i] / 640f;
                bestH = data[3 * numCandidates + i] / 640f;
            }
        }

        if (bestX != -1)
        {
            lastDetection = new Vector4(bestX, bestY, bestW, bestH);

            PlaceCube(bestX, bestY);
        }
    }

    void PlaceCube(float x, float y)
    {
        if (cameraAccess == null || vrCamera == null)
            return;

        Ray ray = cameraAccess.ViewportPointToRay(new Vector2(x, 1 - y));

        if (MRUK.Instance?.GetCurrentRoom() == null)
            return;

        if (MRUK.Instance.GetCurrentRoom().Raycast(ray, 10f, out RaycastHit hit))
        {
            activeCube.SetActive(true);

            Vector3 targetPos = hit.point + (Vector3.up * hoverHeight);

            activeCube.transform.position =
                Vector3.Lerp(activeCube.transform.position,
                             targetPos,
                             Time.deltaTime * smoothSpeed);

            // Face the player
            Vector3 lookDir = vrCamera.transform.position - activeCube.transform.position;
            lookDir.y = 0;

            if (lookDir != Vector3.zero)
            {
                activeCube.transform.rotation =
                    Quaternion.Slerp(
                        activeCube.transform.rotation,
                        Quaternion.LookRotation(lookDir),
                        Time.deltaTime * smoothSpeed);
            }
        }
    }

    void OnDisable()
    {
        worker?.Dispose();
        inputTensor?.Dispose();
    }
}