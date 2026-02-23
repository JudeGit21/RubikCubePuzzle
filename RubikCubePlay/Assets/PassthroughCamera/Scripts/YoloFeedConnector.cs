using UnityEngine;
using Meta.XR; // This matches the namespace in the script you provided

public class YoloFeedConnector : MonoBehaviour
{
    [Header("Connect your YOLO script here")]
    public YoloDetector yoloBrain;

    private PassthroughCameraAccess cameraAccess;

    void Start()
    {
        // Find the camera component in your scene
        cameraAccess = FindFirstObjectByType<PassthroughCameraAccess>();

        if (cameraAccess == null)
        {
            Debug.LogError("YoloFeedConnector: PassthroughCameraAccess not found in Hierarchy!");
        }
    }

    void Update()
    {
        Texture liveTexture = cameraAccess.GetTexture();
        Debug.Log($"[YoloFeedConnector] Camera frame: {liveTexture?.width}x{liveTexture?.height}");

        if (liveTexture != null && yoloBrain != null)
        {
            // Create a temporary 640x640 RenderTexture
            RenderTexture rt = RenderTexture.GetTemporary(640, 640, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(liveTexture, rt);

            // Send it to YOLO
            yoloBrain.DetectCube(rt);

            RenderTexture.ReleaseTemporary(rt);
        }
    }
}