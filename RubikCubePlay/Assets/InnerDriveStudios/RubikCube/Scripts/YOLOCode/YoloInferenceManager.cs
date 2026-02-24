using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Unity.Sentis;
using Meta.XR;
using Kociemba; // Make sure your FaceCube and CubeColor are in this namespace

public class YoloInferenceManager : MonoBehaviour
{
    [Header("AI Assets")]
    public ModelAsset modelAsset;

    [Header("Cube Solving State")]
    public string[] fullCubeData = new string[54]; // 54 stickers
    private int currentFaceIndex = 0; // 0=Up,1=Right,2=Front,3=Down,4=Left,5=Back
    private string[] faceNames = { "Up", "Right", "Front", "Down", "Left", "Back" };

    [Header("YOLO Components")]
    public YoloVisualizer visualizer;

    [Header("Optional TTS")]
    public PowerShellTTS tts;

    void Start()
    {
        if (tts != null) tts.Speak("System ready. Please show the top white face.");
    }

    // Call this when user triggers a face capture
    public void CaptureCurrentFace()
    {
        if (visualizer == null)
        {
            Debug.LogError("Visualizer not assigned!");
            return;
        }

        // 1. Get YOLO detections
        var detections = visualizer.GetActiveDetections();

        if (detections.Count < 9)
        {
            tts?.Speak($"I only see {detections.Count} stickers. Please align the cube properly.");
            return;
        }

        // 2. Sort stickers: Top-to-Bottom, Left-to-Right
        var sortedStickers = detections
            .OrderBy(d => d.y)  // row
            .ThenBy(d => d.x)   // column
            .ToList();

        // 3. Map YOLO labels to Kociemba characters
        int faceStartIndex = currentFaceIndex * 9;
        for (int i = 0; i < 9; i++)
        {
            fullCubeData[faceStartIndex + i] = MapColorToChar(sortedStickers[i].label).ToString();
            Debug.Log($"[CubeScan] Sticker {i}: {sortedStickers[i].label} => {fullCubeData[faceStartIndex + i]}");
        }

        tts?.Speak($"{faceNames[currentFaceIndex]} face captured.");

        // 4. Increment face index or solve
        currentFaceIndex++;
        if (currentFaceIndex < 6)
        {
            tts?.Speak($"Please turn to the {faceNames[currentFaceIndex]} face.");
        }
        else
        {
            tts?.Speak("Scanning complete. Calculating solution.");
            SolveCube();
        }
    }

    // Map YOLO color labels to Kociemba face codes
    private char MapColorToChar(string colorLabel)
    {
        switch (colorLabel.ToLower())
        {
            case "white": return 'U';
            case "blue": return 'R';
            case "red": return 'F';
            case "yellow": return 'D';
            case "green": return 'L';
            case "orange": return 'B';
            default: return 'U'; // fallback
        }
    }

    // Solve cube using Kociemba
    private void SolveCube()
    {
        try
        {
            string cubeString = string.Join("", fullCubeData); // Must be 54 chars
            FaceCube faceCube = new FaceCube(cubeString);       // ✅ no error
            Debug.Log("[CubeSolver] Cube string: " + cubeString);

            // Convert to CubieCube if you want Kociemba solution
            var cubieCube = faceCube.toCubieCube();
            // Call your solver here, e.g., Solver.Solve(cubieCube);

            tts?.Speak("Solution calculated. Displaying steps.");
        }
        catch (Exception e)
        {
            Debug.LogError("[CubeSolver] Error creating FaceCube: " + e.Message);
            tts?.Speak("Error reading cube. Please rescan.");
        }
    }
}