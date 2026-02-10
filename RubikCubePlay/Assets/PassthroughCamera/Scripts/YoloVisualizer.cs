using UnityEngine;
using Unity.Sentis;
using System.Collections.Generic;

public class YoloVisualizer : MonoBehaviour
{
    public GameObject boxPrefab; // Drag your 'Sticker_Highlight' prefab here
    public float confidenceThreshold = 0.5f;
    
    private List<GameObject> activeBoxes = new List<GameObject>();

    public void UpdateBoxes(Tensor<float> output)
    {
        // 1. Clear old boxes from the previous frame
        foreach (var box in activeBoxes) Destroy(box);
        activeBoxes.Clear();

        // 2. YOLOv8 output is usually [1, 84, 8400]
        // [center_x, center_y, width, height, class0, class1...]
        // For this breakdown, we'll focus on the first 50 detections for speed
        float[] data = output.DownloadToArray();

        for (int i = 0; i < 50; i++) 
        {
            float confidence = data[4 * 8400 + i]; // Simplified indexing
            if (confidence > confidenceThreshold)
            {
                // 3. Map AI coordinates to VR Screen
                float x = data[0 * 8400 + i] / 640f; 
                float y = data[1 * 8400 + i] / 640f;

                CreateVisualBox(x, y);
            }
        }
    }

    void CreateVisualBox(float x, float y)
    {
        // Create the highlight in 3D space
        GameObject newBox = Instantiate(boxPrefab);
        
        // This math puts the box in front of your face in the headset
        Vector3 screenPos = new Vector3(x * Screen.width, (1-y) * Screen.height, 0.5f);
        newBox.transform.position = Camera.main.ScreenToWorldPoint(screenPos);
        
        activeBoxes.Add(newBox);
    }
}