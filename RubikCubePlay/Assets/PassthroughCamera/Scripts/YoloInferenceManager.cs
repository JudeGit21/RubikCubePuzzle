using UnityEngine;
using Unity.Sentis; 
using System.Collections.Generic;
using System.Linq; // Added for sotying logic
using Meta.XR;


public class YoloInferenceManager : MonoBehaviour
{
    [Header("AI Assets")]
    public ModelAsset modelAsset;

    [Header("Cube Solving State")]
    public string[] fullCubeData = new string[54]; // U1..U9, R1..R9, etc.
    public int currentFaceIndex = 0; // 0=U, 1=R, 2=F, 3=D, 4=L, 5=B
    private string[] faceNames = { "Up", "Right", "Front", "Down", "Left", "Back" };
    private string[] faceCodes = { "U", "R", "F", "D", "L", "B" };

    private Model runtimeModel;
    private Worker worker;
    private Tensor<float> inputTensor;

    private PassthroughCameraAccess cameraAccess;
    private YoloVisualizer visualizer;
    private PowerShellTTS tts; // Reference to our new voice script

    void Start()
    {
        // ... (Your existing initialization code) ...
        tts = FindFirstObjectByType<PowerShellTTS>();

        // Initial Greeting
        if (tts != null) tts.Speak("System ready. Please show me the top white face.");
    }

    // Call this via a UI Button or VR Controller Trigger
    public void CaptureCurrentFace()
    {
        if (visualizer == null) return;

        // 1. Get detections from the visualizer
        var detections = visualizer.GetActiveDetections();

        if (detections.Count < 9)
        {
            tts.Speak($"I only see {detections.Count} stickers. Please align the cube.");
            return;
        }

        // 2. Sort stickers into the 3x3 grid (Top-to-Bottom, then Left-to-Right)
        var sortedDetections = detections
            .OrderBy(d => d.y) // Sort rows
            .ThenBy(d => d.x)  // Sort columns
            .ToList();

        // 3. Map detected colors to Kociemba characters
        int offset = currentFaceIndex * 9;
        for (int i = 0; i < 9; i++)
        {
            // Note: You'll need to map YOLO labels (e.g., "red") to face codes
            fullCubeData[offset + i] = MapColorToFaceCode(sortedDetections[i].label);

            Debug.Log($"[YoloInferenceManager] Sticker {i}: {sortedDetections[i].label} mapped to {fullCubeData[offset + i]}");
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
        // This logic assumes standard color-to-face mapping.
        // In a pro version, you'd check the center sticker color.
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
        // Pass 'cubeString' to your Kociemba script here
        Debug.Log("Final Cube String: " + cubeString);
    }

    // ... (Rest of your existing ProcessFrame and OnDestroy code) ...
}