/* using UnityEngine;
using Unity.Sentis;
using System.Collections.Generic;
using System.Linq;


public class YoloInferenceManager : MonoBehaviour
{
    [Header("AI Assets")]
    public ModelAsset modelAsset;

    [Header("Cube Solving State")]
    public string[] fullCubeData = new string[54];
    public int currentFaceIndex = 0;
    private string[] faceNames = { "Up", "Right", "Front", "Down", "Left", "Back" };
    private string[] faceCodes = { "U", "R", "F", "D", "L", "B" };

    private Model runtimeModel;
    // Sentis 2.x adjustment: Use 'Worker' instead of 'IWorker'
    private Worker worker;
    private Tensor<float> inputTensor;

   // private LaptopCameraAccess cameraAccess;

    private YoloVisualizer visualizer;
    private PowerShellTTS tts;

    void Start()
    {
        Debug.Log("AI_Brain: Starting Quest-optimized initialization...");

        if (modelAsset == null)
        {
            Debug.LogError("AI_Brain: No ModelAsset assigned!");
            return;
        }

        try
        {
            // 1. Setup Sentis Pipeline
            runtimeModel = ModelLoader.Load(modelAsset);

            // Sentis 2.x adjustment: Create worker directly without WorkerFactory
            worker = new Worker(runtimeModel, BackendType.GPUCompute);

            inputTensor = new Tensor<float>(new TensorShape(1, 3, 640, 640));

            // 2. Locate Components
            visualizer = GetComponent<YoloVisualizer>();
            cameraAccess = FindFirstObjectByType<LaptopCameraAccess>();

            tts = FindFirstObjectByType<PowerShellTTS>();

            // 3. Feedback logic
            if (tts != null) tts.Speak("System ready. Please show me the top white face.");

            Debug.Log("AI_Brain: Quest-compatible Worker created.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"AI_Brain: Initialization failed: {e.Message}");
        }
    }

    public void CaptureCurrentFace()
    {
        if (visualizer == null || tts == null) return;

        var detections = visualizer.GetActiveDetections();

        if (detections.Count < 9)
        {
            tts.Speak($"I only see {detections.Count} stickers. Please align the cube.");
            return;
        }

        // Sort stickers into the 3x3 grid (Top-to-Bottom, then Left-to-Right)
        var sortedDetections = detections
            .OrderBy(d => d.y)
            .ThenBy(d => d.x)
            .ToList();

        int offset = currentFaceIndex * 9;
        for (int i = 0; i < 9; i++)
        {
            fullCubeData[offset + i] = MapColorToFaceCode(sortedDetections[i].label);
        }

        tts.Speak($"{faceNames[currentFaceIndex]} face captured.");
        currentFaceIndex++;

        if (currentFaceIndex < 6)
        {
            tts.Speak($"Please turn to the {faceNames[currentFaceIndex]} face.");
        }
        else
        {
            tts.Speak("Scanning complete. Calculating solution.");
            RunKociemba();
        }
    }

    private string MapColorToFaceCode(string colorLabel)
    {
        switch (colorLabel.ToLower())
        {
            case "white": return "U";
            case "red": return "F";
            case "blue": return "R";
            case "orange": return "B";
            case "green": return "L";
            case "yellow": return "D";
            default: return "U";
        }
    }

    private void RunKociemba()
    {
        string cubeString = string.Join("", fullCubeData);
        Debug.Log("Final Cube String for Kociemba: " + cubeString);
        // You can now pass 'cubeString' to your Kociemba solving script.
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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CaptureCurrentFace();
        }

    }

    public void ProcessFrame(Texture cameraFrame)
    {
        if (worker == null || cameraFrame == null) return;

        TextureConverter.ToTensor(cameraFrame, inputTensor, new TextureTransform());

        // Sentis 2.x adjustment: Use 'Schedule' instead of 'Execute'
        worker.Schedule(inputTensor);

        // Sentis 2.x adjustment: PeekOutput is fine, but cast to Tensor<float>
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
} */