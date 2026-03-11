using UnityEngine;

public class CubeColorScanner : MonoBehaviour
{
    public Camera vrCamera;

    public Vector2[] sampleOffsets =
    {
        new Vector2(-0.03f,0.03f),
        new Vector2(0,0.03f),
        new Vector2(0.03f,0.03f),

        new Vector2(-0.03f,0),
        new Vector2(0,0),
        new Vector2(0.03f,0),

        new Vector2(-0.03f,-0.03f),
        new Vector2(0,-0.03f),
        new Vector2(0.03f,-0.03f),
    };

    public Texture2D cameraFrame;

    public char[] ScanFace()
    {
        char[] face = new char[9];

        for (int i = 0; i < 9; i++)
        {
            Vector2 screenPos = new Vector2(
                Screen.width / 2 + sampleOffsets[i].x * Screen.width,
                Screen.height / 2 + sampleOffsets[i].y * Screen.height
            );

            Color pixel = cameraFrame.GetPixel((int)screenPos.x, (int)screenPos.y);

            face[i] = ClassifyColor(pixel);
        }

        return face;
    }

    char ClassifyColor(Color c)
    {
        Color.RGBToHSV(c, out float h, out float s, out float v);

        if (v > 0.85f && s < 0.25f) return 'W';

        if (h < 0.05f || h > 0.95f) return 'R';

        if (h > 0.05f && h < 0.12f) return 'O';

        if (h > 0.12f && h < 0.25f) return 'Y';

        if (h > 0.25f && h < 0.55f) return 'G';

        return 'B';
    }
}