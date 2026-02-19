using UnityEngine;

public class LaptopWebcamAccess : MonoBehaviour
{
    [Header("Camera selection")]
    public int deviceIndex = 0;

    [Header("Requested")]
    public int requestedWidth = 1280;
    public int requestedHeight = 720;
    public int requestedFPS = 30;

    private WebCamTexture webCamTexture;

    public bool IsPlaying => webCamTexture != null && webCamTexture.isPlaying;

    void Start()
    {
        StartCamera();
    }

    public void StartCamera()
    {
        var devices = WebCamTexture.devices;
        if (devices == null || devices.Length == 0)
        {
            Debug.LogError("LaptopWebcamAccess: No webcam devices found.");
            return;
        }

        deviceIndex = Mathf.Clamp(deviceIndex, 0, devices.Length - 1);

        webCamTexture = new WebCamTexture(devices[deviceIndex].name, requestedWidth, requestedHeight, requestedFPS);
        webCamTexture.Play();

        Debug.Log($"LaptopWebcamAccess: Started camera '{devices[deviceIndex].name}' " +
                  $"({requestedWidth}x{requestedHeight}@{requestedFPS}).");
    }

    public Texture GetTexture()
    {
        return webCamTexture;
    }

    void OnDisable()
    {
        if (webCamTexture != null && webCamTexture.isPlaying)
            webCamTexture.Stop();
    }
}
