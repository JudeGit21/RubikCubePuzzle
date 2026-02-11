using UnityEngine;
using Unity.Sentis;
using System.Collections.Generic;

public class YoloVisualizer : MonoBehaviour
{
    public GameObject boxPrefab; // Drag your 'Sticker_Highlight' prefab here
    [Range(0, 1)]
    public float confidenceThreshold = 0.5f;
    
    private List<GameObject> activeBoxes = new List<GameObject>();

    // These labels match the standard COCO dataset used by YOLOv8
    private string[] labels = { "person", "bicycle", "car", "motorcycle", "airplane", "bus", "train", "truck", "boat", "traffic light", "fire hydrant", "stop sign", "parking meter", "bench", "bird", "cat", "dog", "horse", "sheep", "cow", "elephant", "bear", "zebra", "giraffe", "backpack", "umbrella", "handbag", "tie", "suitcase", "frisbee", "skis", "snowboard", "sports ball", "kite", "baseball bat", "baseball glove", "skateboard", "surfboard", "tennis racket", "bottle", "wine glass", "cup", "fork", "knife", "spoon", "bowl", "banana", "apple", "sandwich", "orange", "broccoli", "carrot", "hot dog", "pizza", "donut", "cake", "chair", "couch", "potted plant", "bed", "dining table", "toilet", "tv", "laptop", "mouse", "remote", "keyboard", "cell phone", "microwave", "oven", "toaster", "sink", "refrigerator", "book", "clock", "vase", "scissors", "teddy bear", "hair drier", "toothbrush" };

    public void UpdateBoxes(Tensor<float> output)
    {
        // 1. Clear old boxes
        foreach (var box in activeBoxes) Destroy(box);
        activeBoxes.Clear();

        // 2. Download data from GPU
        float[] data = output.DownloadToArray();
        
        // YOLOv8n constants
        int totalDetections = 8400; 
        int totalAttributes = 84; // 4 coordinates + 80 classes

        // 3. Iterate through detections (limiting to 100 for performance)
        for (int i = 0; i < totalDetections; i++) 
        {
            float maxScore = 0;
            int classId = -1;

            // Find the highest confidence class for this detection
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
                // Normalize 640x640 AI coordinates to 0.0 - 1.0 range
                float x = data[0 * totalDetections + i] / 640f; 
                float y = data[1 * totalDetections + i] / 640f;
                float w = data[2 * totalDetections + i] / 640f;
                float h = data[3 * totalDetections + i] / 640f;

                CreateVisualBox(x, y, w, h, labels[classId]);
                
                // Don't overwhelm the VR view
                if (activeBoxes.Count > 15) break;
            }
        }
    }

    void CreateVisualBox(float x, float y, float w, float h, string labelName)
    {
        // Place the box 1.0 meters in front of the camera
        float depth = 1.0f; 

        // Convert normalized coordinates to screen space
        // YOLO x/y is center, Screen expects center for WorldPoint conversion
        Vector3 screenPos = new Vector3(x * Screen.width, (1 - y) * Screen.height, depth);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);

        GameObject newBox = Instantiate(boxPrefab, worldPos, Camera.main.transform.rotation);
        
        // Scale the box based on detected width/height
        // Adjust these multipliers if the neon boxes are too big or small
        newBox.transform.localScale = new Vector3(w * 2, h * 2, 0.01f);
        
        // Optional: Change the name so you can see what it is in the Hierarchy
        newBox.name = $"Detected_{labelName}";

        activeBoxes.Add(newBox);
    }
}