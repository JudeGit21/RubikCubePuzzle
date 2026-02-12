using UnityEngine;
using Unity.Sentis;
using System.Collections.Generic;

// Simple class to hold detection data for the solver
public class DetectionData
{
    public float x;
    public float y;
    public string label;
}

public class YoloVisualizer : MonoBehaviour
{
    public GameObject boxPrefab;
    [Range(0, 1)]
    public float confidenceThreshold = 0.5f;

    private List<GameObject> activeBoxes = new List<GameObject>();

    // 1. UPDATED LABELS: These must match the order your YOLO model was trained on
    // Usually: white, yellow, red, orange, blue, green
    private string[] labels = { "white", "yellow", "red", "orange", "blue", "green" };

    // 2. DATA STORAGE: This is what the Inference Manager will read
    public List<DetectionData> lastFrameDetections = new List<DetectionData>();

    public void UpdateBoxes(Tensor<float> output)
    {
        // Clear old visual boxes and data
        foreach (var box in activeBoxes) Destroy(box);
        activeBoxes.Clear();
        lastFrameDetections.Clear();

        float[] data = output.DownloadToArray();

        int totalDetections = 8400;
        int totalAttributes = 4 + labels.Length; // 4 coordinates + number of colors

        for (int i = 0; i < totalDetections; i++)
        {
            float maxScore = 0;
            int classId = -1;

            for (int c = 4; c < totalAttributes; c++)
            {
                float score = data[c * totalDetections + i];
                if (score > maxScore)
                {
                    maxScore = score;
                    classId = c - 4;
                }
            }

            if (maxScore > confidenceThreshold)
            {
                float x = data[0 * totalDetections + i] / 640f;
                float y = data[1 * totalDetections + i] / 640f;
                float w = data[2 * totalDetections + i] / 640f;
                float h = data[3 * totalDetections + i] / 640f;

                // Store the data for the solver
                lastFrameDetections.Add(new DetectionData { x = x, y = y, label = labels[classId] });

                // Create the visual box in the VR view
                CreateVisualBox(x, y, w, h, labels[classId]);

                if (activeBoxes.Count > 20) break; // Increased to 20 to ensure all 9 stickers show
            }
        }
    }

    // This helper method allows the InferenceManager to get the data
    public List<DetectionData> GetActiveDetections()
    {
        return lastFrameDetections;
    }

    void CreateVisualBox(float x, float y, float w, float h, string labelName)
    {
        float depth = 1.0f;
        Vector3 screenPos = new Vector3(x * Screen.width, (1 - y) * Screen.height, depth);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);

        GameObject newBox = Instantiate(boxPrefab, worldPos, Camera.main.transform.rotation);
        newBox.transform.localScale = new Vector3(w * 2, h * 2, 0.01f);
        newBox.name = $"Sticker_{labelName}";

        activeBoxes.Add(newBox);
    }
}