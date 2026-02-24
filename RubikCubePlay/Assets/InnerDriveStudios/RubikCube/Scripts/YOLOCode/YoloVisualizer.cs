using System.Collections.Generic;
using Unity.Sentis;
using UnityEngine;

// Holds detection info for solver
public class DetectionData
{
    public float x;      // Normalized [0,1] X
    public float y;      // Normalized [0,1] Y
    public string label; // Detected color
}

public class YoloVisualizer : MonoBehaviour
{
    public GameObject boxPrefab;
    [Range(0, 1)]
    public float confidenceThreshold = 0.5f;

    private List<GameObject> activeBoxes = new List<GameObject>();
    private string[] labels = { "white", "yellow", "red", "orange", "blue", "green" };

    public List<DetectionData> lastFrameDetections = new List<DetectionData>();

    public void UpdateBoxes(Tensor<float> output)
    {
        // Clear old boxes & detections
        foreach (var box in activeBoxes) Destroy(box);
        activeBoxes.Clear();
        lastFrameDetections.Clear();

        if (output == null) return;

        // Get output array
        float[] data = output.DownloadToArray();

        // Shape parameters — adjust based on your YOLOv8 Sentis output
        int totalDetections = output.shape[1]; // numBoxes
        int totalAttributes = output.shape[2]; // 4 coords + numClasses

        for (int i = 0; i < totalDetections; i++)
        {
            // 1. Find class with max confidence
            float maxScore = 0;
            int classId = -1;

            for (int c = 4; c < totalAttributes; c++)
            {
                float score = data[i * totalAttributes + c];
                if (score > maxScore)
                {
                    maxScore = score;
                    classId = c - 4;
                }
            }

            if (maxScore < confidenceThreshold) continue;

            // 2. Get bounding box
            // Check if YOLO outputs normalized [0,1] coords or absolute pixels
            float x = data[i * totalAttributes + 0]; // center X
            float y = data[i * totalAttributes + 1]; // center Y
            float w = data[i * totalAttributes + 2];
            float h = data[i * totalAttributes + 3];

            // If YOLO gives pixel coords, divide by 640 (input tensor size)
            x /= 640f;
            y /= 640f;
            w /= 640f;
            h /= 640f;

            // 3. Store detection for solver
            lastFrameDetections.Add(new DetectionData { x = x, y = y, label = labels[classId] });
            Debug.Log($"[YoloVisualizer] Detected {labels[classId]} at ({x:F2},{y:F2}) conf {maxScore:F2}");

            // 4. Optional: create visual box in MR
            CreateVisualBox(x, y, w, h, labels[classId]);
        }
    }

    public List<DetectionData> GetActiveDetections()
    {
        return lastFrameDetections;
    }

    void CreateVisualBox(float x, float y, float w, float h, string labelName)
    {
        // In MR, you may want to parent this box to the cube prefab later
        Vector3 screenPos = new Vector3(x * Screen.width, (1 - y) * Screen.height, 1f);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);

        GameObject newBox = Instantiate(boxPrefab, worldPos, Camera.main.transform.rotation);
        newBox.transform.localScale = new Vector3(w * 2, h * 2, 0.01f);
        newBox.name = $"Sticker_{labelName}";

        activeBoxes.Add(newBox);
    }
}