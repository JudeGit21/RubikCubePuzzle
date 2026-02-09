using UnityEngine;
using Meta.XR; // This matches the namespace in the script you provided

public class YoloFeedConnector : MonoBehaviour 
{
    [Header("Connect your YOLO script here")]
    public YoloDetector yoloBrain; 
    
    private PassthroughCameraAccess cameraAccess;

    void Start() {
        // Find the camera component in your scene
        cameraAccess = FindFirstObjectByType<PassthroughCameraAccess>();
        
        if (cameraAccess == null) {
            Debug.LogError("YoloFeedConnector: PassthroughCameraAccess not found in Hierarchy!");
        }
    }

    void Update() {
        // Using the public 'IsPlaying' property and 'GetTexture()' from your script
        if (cameraAccess != null && cameraAccess.IsPlaying) {
            Texture liveTexture = cameraAccess.GetTexture();
            
            if (liveTexture != null && yoloBrain != null) {
                // Send the texture to your AI brain
                yoloBrain.DetectCube(liveTexture as RenderTexture);
            }
        }
    }
}