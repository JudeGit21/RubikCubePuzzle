using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Scans a Rubik's Cube face-by-face from a webcam ROI (scanFrame) and produces a 54-char
/// Kociemba facelet string in the order: U, R, F, D, L, B.
/// Press SPACE to capture the current face. Press R to reset.
/// </summary>
public class RubikFaceScanner : MonoBehaviour
{
    [Header("References")]
    public LaptopWebcamAccess cameraAccess;
    public RectTransform scanFrame;
    public RawImage webcamRawImage;

    [Header("Input")]
    public KeyCode captureKey = KeyCode.Space;
    public KeyCode resetKey = KeyCode.R;

    [Header("Color References (RGB 0-255)")]
    public Color32 refWhite = new Color32(208, 238, 243, 255);
    public Color32 refYellow = new Color32(209, 207, 60, 255);
    public Color32 refOrange = new Color32(214, 142, 15, 255);
    public Color32 refRed = new Color32(174, 60, 80, 255);
    public Color32 refGreen = new Color32(27, 124, 97, 255);
    public Color32 refBlue = new Color32(3, 71, 140, 255);

    [Header("Color Tuning")]
    [Tooltip("If the closest reference color is farther than this squared RGB distance, return 'unknown'.")]
    public float maxDistanceSq = 16000f;

    // Kociemba scan order: U, R, F, D, L, B
    private readonly string[] faceNames = { "Up", "Right", "Front", "Down", "Left", "Back" };
    private readonly string[] faceCodes = { "U", "R", "F", "D", "L", "B" };

    private int currentFaceIndex = 0;

    // store colors for each face, 6 faces x 9 stickers
    private readonly string[][] capturedFaces = new string[6][];

    void Start()
    {
        if (cameraAccess == null) cameraAccess = FindFirstObjectByType<LaptopWebcamAccess>();
        if (webcamRawImage == null) webcamRawImage = FindFirstObjectByType<RawImage>();

        if (cameraAccess == null) Debug.LogError("RubikFaceScanner: LaptopWebcamAccess not found.");
        if (scanFrame == null) Debug.LogError("RubikFaceScanner: ScanFrame not assigned.");
        if (webcamRawImage == null) Debug.LogError("RubikFaceScanner: RawImage not found.");

        Debug.Log($"RubikFaceScanner ready. Show {faceNames[currentFaceIndex]} face inside the frame and press SPACE.");
        Debug.Log("Press R to reset scanning.");
    }

    void Update()
    {
        if (Input.GetKeyDown(resetKey))
        {
            ResetScan();
            return;
        }

        if (!Input.GetKeyDown(captureKey))
            return;

        var cam = cameraAccess != null ? cameraAccess.GetTexture() as WebCamTexture : null;
        if (cam == null || cam.width < 16)
        {
            Debug.LogWarning("RubikFaceScanner: Webcam not ready yet.");
            return;
        }

        if (currentFaceIndex >= 6)
        {
            Debug.Log("Already captured 6 faces. Press R to reset.");
            return;
        }

        // Scan current face (3x3)
        var face = Scan3x3(cam);

        if (face == null || face.Length != 9)
        {
            Debug.LogError("Scan failed: face array not 9.");
            return;
        }

        // Reject if too many unknowns
        int unknownCount = 0;
        for (int i = 0; i < 9; i++) if (face[i] == "unknown") unknownCount++;
        if (unknownCount > 2)
        {
            Debug.LogWarning($"Too many unknown stickers ({unknownCount}/9). Try better lighting and keep the face inside the frame.");
            Debug.Log("Face = " + string.Join(",", face));
            return;
        }

        capturedFaces[currentFaceIndex] = face;
        Debug.Log($"Captured {faceNames[currentFaceIndex]} ({faceCodes[currentFaceIndex]}) = {string.Join(",", face)}");

        currentFaceIndex++;

        if (currentFaceIndex < 6)
        {
            Debug.Log($"Now show {faceNames[currentFaceIndex]} face and press SPACE.");
            return;
        }

        Debug.Log("Captured 6 faces. Building cube string...");
        string cubeString = BuildKociembaString();
        if (cubeString == "ERROR_DUPLICATE_CENTER")
        {
            Debug.Log($"Rescan required. Show {faceNames[currentFaceIndex]} face and press SPACE.");
            return;
        }

        Debug.Log("Final Cube String for Kociemba: " + cubeString);

        // Call solver (static helper class, NOT a MonoBehaviour component)
        string info;
        string solution = KociembaSolver.Solve(cubeString, out info);

        Debug.Log("Solver info: " + info);
        Debug.Log("Moves to solve: " + solution);
    }

    void ResetScan()
    {
        for (int i = 0; i < 6; i++) capturedFaces[i] = null;
        currentFaceIndex = 0;
        Debug.Log("Scan reset. Show Up face and press SPACE.");
    }

    // Build 54-char string in order U/R/F/D/L/B using the scanned centers as the mapping.
    string BuildKociembaString()
    {
        // Map center color name -> face letter (U/R/F/D/L/B)
        Dictionary<string, string> centerToFace = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        for (int f = 0; f < 6; f++)
        {
            var face = capturedFaces[f];
            if (face == null || face.Length != 9) throw new Exception($"Face {f} missing.");

            string centerColor = face[4];
            Debug.Log($"Face {faceNames[f]} center = {centerColor} (full: {string.Join(",", face)})");

            if (centerColor == "unknown")
                throw new Exception($"Center color unknown on face {faceNames[f]}");

            if (centerToFace.ContainsKey(centerColor))
            {
                Debug.LogWarning(
                    $"Duplicate center color '{centerColor}' detected on {faceNames[f]}. " +
                    "Scan the cube again with better lighting and keep the face aligned."
                );

                // Jump back to the face that failed and clear it
                currentFaceIndex = f;
                capturedFaces[f] = null;
                return "ERROR_DUPLICATE_CENTER";
            }

            centerToFace[centerColor] = faceCodes[f];
        }

        // Convert all stickers using the mapping
        char[] out54 = new char[54];
        int idx = 0;

        for (int f = 0; f < 6; f++)
        {
            var face = capturedFaces[f];
            for (int i = 0; i < 9; i++)
            {
                string col = face[i];
                if (!centerToFace.TryGetValue(col, out string code))
                {
                    // Fallback to the center of this face
                    string fallback = face[4];

                    if (!centerToFace.TryGetValue(fallback, out code))
                        code = faceCodes[f];

                    Debug.LogWarning($"Color '{col}' not mapped. Using fallback '{code}'. (face {faceNames[f]}, sticker {i})");
                }

                out54[idx++] = code[0];
            }
        }

        return new string(out54);
    }

    // ===================== SCANNING =====================

    string[] Scan3x3(WebCamTexture cam)
    {
        Rect roi01 = GetRoiInRawImage01(scanFrame, webcamRawImage);

        int x0 = Mathf.RoundToInt(roi01.x * cam.width);
        int y0 = Mathf.RoundToInt(roi01.y * cam.height);
        int w = Mathf.RoundToInt(roi01.width * cam.width);
        int h = Mathf.RoundToInt(roi01.height * cam.height);

        x0 = Mathf.Clamp(x0, 0, cam.width - 1);
        y0 = Mathf.Clamp(y0, 0, cam.height - 1);
        w = Mathf.Clamp(w, 1, cam.width - x0);
        h = Mathf.Clamp(h, 1, cam.height - y0);

        Color32[] pixels = cam.GetPixels32();

        string[] out9 = new string[9];

        int cellW = w / 3;
        int cellH = h / 3;

        // Sample the inner region of each cell to avoid edges/borders
        for (int gy = 0; gy < 3; gy++)
        {
            for (int gx = 0; gx < 3; gx++)
            {
                int cx0 = x0 + gx * cellW + cellW / 3;
                int cy0 = y0 + gy * cellH + cellH / 3;
                int cx1 = x0 + gx * cellW + (2 * cellW) / 3;
                int cy1 = y0 + gy * cellH + (2 * cellH) / 3;

                Color avg = AvgColor(pixels, cam.width, cam.height, cx0, cy0, cx1, cy1);
                out9[gy * 3 + gx] = ClassifyRubikColor(avg);
            }
        }

        return out9;
    }

    // Convert scanFrame rect (world space) to normalized rect (0..1) inside the RawImage
    Rect GetRoiInRawImage01(RectTransform frame, RawImage raw)
    {
        RectTransform rawRt = raw.rectTransform;

        Vector3[] corners = new Vector3[4];
        frame.GetWorldCorners(corners);

        Vector2 p0 = WorldToLocal(rawRt, corners[0]); // bottom-left
        Vector2 p2 = WorldToLocal(rawRt, corners[2]); // top-right

        Rect rawRect = rawRt.rect;

        float xMin = Mathf.InverseLerp(rawRect.xMin, rawRect.xMax, p0.x);
        float yMin = Mathf.InverseLerp(rawRect.yMin, rawRect.yMax, p0.y);
        float xMax = Mathf.InverseLerp(rawRect.xMin, rawRect.xMax, p2.x);
        float yMax = Mathf.InverseLerp(rawRect.yMin, rawRect.yMax, p2.y);

        xMin = Mathf.Clamp01(xMin); yMin = Mathf.Clamp01(yMin);
        xMax = Mathf.Clamp01(xMax); yMax = Mathf.Clamp01(yMax);

        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }

    Vector2 WorldToLocal(RectTransform rt, Vector3 world)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rt,
            RectTransformUtility.WorldToScreenPoint(null, world),
            null,
            out Vector2 local
        );
        return local;
    }

    // Average a rectangular pixel region and return a Unity Color (0..1)
    Color AvgColor(Color32[] pixels, int width, int height, int x0, int y0, int x1, int y1)
    {
        x0 = Mathf.Clamp(x0, 0, width - 1);
        x1 = Mathf.Clamp(x1, 0, width);
        y0 = Mathf.Clamp(y0, 0, height - 1);
        y1 = Mathf.Clamp(y1, 0, height);

        long r = 0, g = 0, b = 0;
        long count = 0;

        for (int y = y0; y < y1; y++)
        {
            for (int x = x0; x < x1; x++)
            {
                var c = pixels[y * width + x];
                r += c.r; g += c.g; b += c.b;
                count++;
            }
        }

        if (count == 0) return Color.black;
        return new Color((float)r / (255f * count), (float)g / (255f * count), (float)b / (255f * count));
    }

    // ===================== COLOR CLASSIFICATION =====================

    string ClassifyRubikColor(Color rgb01)
    {
        // Convert to 0-255 space
        Color32 c = rgb01;

        // Compute squared distance to each reference color
        float dWhite = DistSq(c, refWhite);
        float dYellow = DistSq(c, refYellow);
        float dOrange = DistSq(c, refOrange);
        float dRed = DistSq(c, refRed);
        float dGreen = DistSq(c, refGreen);
        float dBlue = DistSq(c, refBlue);

        float min = dWhite;
        string best = "white";

        if (dYellow < min) { min = dYellow; best = "yellow"; }
        if (dOrange < min) { min = dOrange; best = "orange"; }
        if (dRed < min) { min = dRed; best = "red"; }
        if (dGreen < min) { min = dGreen; best = "green"; }
        if (dBlue < min) { min = dBlue; best = "blue"; }

        if (min > maxDistanceSq)
            return "unknown";

        return best;
    }

    static float DistSq(Color32 a, Color32 b)
    {
        float dr = a.r - b.r;
        float dg = a.g - b.g;
        float db = a.b - b.b;
        return dr * dr + dg * dg + db * db;
    }
}
