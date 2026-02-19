using UnityEngine;
using UnityEngine.UI;

public class WebcamPreview : MonoBehaviour
{
    public LaptopWebcamAccess cameraAccess;
    private RawImage raw;

    void Start()
    {
        raw = GetComponent<RawImage>();
        if (cameraAccess == null) cameraAccess = FindFirstObjectByType<LaptopWebcamAccess>();

        if (cameraAccess == null)
        {
            Debug.LogError("WebcamPreview: LaptopCameraAccess not found.");
            return;
        }
    }

    void Update()
    {
        var tex = cameraAccess.GetTexture();
        if (tex != null) raw.texture = tex;

        
    }
}
