using UnityEngine;
using Meta.XR;

public class CubeScanner : MonoBehaviour
{
    public RubikDetectorPro detector;
    public PassthroughCameraAccess cameraAccess;

    Vector2[] grid =
    {
        new Vector2(-0.33f,0.33f),
        new Vector2(0,0.33f),
        new Vector2(0.33f,0.33f),

        new Vector2(-0.33f,0),
        new Vector2(0,0),
        new Vector2(0.33f,0),

        new Vector2(-0.33f,-0.33f),
        new Vector2(0,-0.33f),
        new Vector2(0.33f,-0.33f)
    };

    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            ScanFace();
        }
    }

    void ScanFace()
    {
        if (detector == null || cameraAccess == null)
        {
            Debug.Log("Scanner not configured");
            return;
        }

        Texture frame = cameraAccess.GetTexture();

        if (frame == null)
        {
            Debug.Log("No camera frame");
            return;
        }

        Texture2D tex = frame as Texture2D;

        if (tex == null)
        {
            Debug.Log("Camera texture not readable");
            return;
        }

        Vector4 det = detector.lastDetection;

        if (det.x <= 0)
        {
            Debug.Log("No cube detected");
            return;
        }

        Debug.Log("Scanning cube face...");

        for (int i = 0; i < 9; i++)
        {
            Vector2 center = new Vector2(det.x, det.y);
            Vector2 size = new Vector2(det.z, det.w);

            Vector2 sample = center + Vector2.Scale(grid[i], size);

            int px = Mathf.Clamp((int)(sample.x * tex.width), 0, tex.width - 1);
            int py = Mathf.Clamp((int)((1 - sample.y) * tex.height), 0, tex.height - 1);

            Color c = tex.GetPixel(px, py);

            char cubeColor = ClassifyColor(c);

            Debug.Log("Sticker " + i + " = " + cubeColor);
        }
    }

    char ClassifyColor(Color col)
    {
        Color.RGBToHSV(col, out float h, out float s, out float v);

        if (v > 0.8f && s < 0.25f) return 'W';

        if (h < 0.05f || h > 0.95f) return 'R';

        if (h > 0.05f && h < 0.12f) return 'O';

        if (h > 0.12f && h < 0.25f) return 'Y';

        if (h > 0.25f && h < 0.55f) return 'G';

        return 'B';
    }
}