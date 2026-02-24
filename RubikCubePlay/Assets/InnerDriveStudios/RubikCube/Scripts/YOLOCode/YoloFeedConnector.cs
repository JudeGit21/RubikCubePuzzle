using UnityEngine;
using Meta.XR; // This matches the namespace in the script you provided

public class YoloFeedConnector : MonoBehaviour
{
    [Header("Connect your YOLO script here")]
    public YoloDetector yoloBrain;
    private RenderTexture rt;
    private PassthroughCameraAccess cameraAccess;

    void Start()
    {
        cameraAccess = FindFirstObjectByType<PassthroughCameraAccess>();
        if (cameraAccess == null)
        {
            Debug.LogError("PassthroughCameraAccess not found!");
            return;
        }

        rt = new RenderTexture(640, 640, 0, RenderTextureFormat.ARGB32);
    }

    void Update()
    {
        if (cameraAccess == null) return;

        Texture liveTexture = cameraAccess.GetTexture(); // <- grab the live frame
        if (liveTexture == null) return; // safety check

        Graphics.Blit(liveTexture, rt);

        if (yoloBrain != null)
            yoloBrain.DetectCube(rt);
    }

    void OnDestroy()
    {
        if (rt != null) rt.Release();
    }
}