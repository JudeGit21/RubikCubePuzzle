using UnityEngine;
using Unity.Sentis;
using Meta.XR.MRUtilityKit;
using Meta.XR;

public class RubikDetectorPro : MonoBehaviour
{
    [Header("AI Configuration")]
    public ModelAsset yoloModel;
    [Range(0, 1)] public float confidenceThreshold = 0.70f;

    [Header("Instructor Settings")]
    public GameObject digitalCubePrefab;
    public float hoverHeight = 0.25f;
    public float smoothSpeed = 12f;

    private GameObject activeCube;
    private Worker worker;
    private Tensor<float> inputTensor;
    private PassthroughCameraAccess cameraAccess;
    private Camera vrCamera;

    void Start()
    {
        // 1. Setup AI Worker
        try {
            var model = ModelLoader.Load(yoloModel);
            worker = new Worker(model, BackendType.GPUCompute);
            inputTensor = new Tensor<float>(new TensorShape(1, 3, 640, 640));
        } catch { Debug.LogWarning("AI Worker failed to start. Check your model asset."); }

        // 2. Find the Camera Rig components
        cameraAccess = FindFirstObjectByType<PassthroughCameraAccess>();
        
        // Find the camera even if it's not tagged "MainCamera"
        vrCamera = Camera.main;
        if (vrCamera == null) vrCamera = FindFirstObjectByType<Camera>();

        // 3. FORCE SPAWN A TEST CUBE (To verify the prefab works)
        if (digitalCubePrefab != null)
        {
            activeCube = Instantiate(digitalCubePrefab);
            activeCube.name = "INSTRUCTOR_CUBE";
            activeCube.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            
            // Disable old scripts on the prefab so they don't crash the Input System
            MonoBehaviour[] scripts = activeCube.GetComponentsInChildren<MonoBehaviour>();
            foreach (var s in scripts) {
                if (s.GetType().Name.Contains("Rotator")) s.enabled = false;
            }
        }
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

        for (int i = 0; i < numCandidates; i++) {
            float score = data[4 * numCandidates + i];
            if (score > bestScore) {
                bestScore = score;
                bestX = data[0 * numCandidates + i] / 640f;
                bestY = data[1 * numCandidates + i] / 640f;
            }
        }

        if (bestX != -1) PlaceCube(bestX, bestY);
    }

    void PlaceCube(float x, float y)
    {
        if (cameraAccess == null || vrCamera == null) return;

        Ray ray = cameraAccess.ViewportPointToRay(new Vector2(x, 1 - y));

        if (MRUK.Instance?.GetCurrentRoom() != null) {
            if (MRUK.Instance.GetCurrentRoom().Raycast(ray, 10f, out RaycastHit hit)) {
                activeCube.SetActive(true);
                
                // Position above hand
                Vector3 targetPos = hit.point + (Vector3.up * hoverHeight);
                activeCube.transform.position = Vector3.Lerp(activeCube.transform.position, targetPos, Time.deltaTime * smoothSpeed);
                
                // Rotate to face player
                Vector3 lookDir = vrCamera.transform.position - activeCube.transform.position;
                lookDir.y = 0;
                if (lookDir != Vector3.zero)
                    activeCube.transform.rotation = Quaternion.Slerp(activeCube.transform.rotation, Quaternion.LookRotation(-lookDir), Time.deltaTime * smoothSpeed);
            }
        }
    }

    private void OnDisable() {
        worker?.Dispose();
        inputTensor?.Dispose();
    }
}