using UnityEngine;
using System.Collections;
using System.Threading;

public class ScanFlowController : MonoBehaviour
{
    [Header("Refs")]
    public RubikFaceScanner scanner;
    public CubeNetUI cubeNetUI;

    [Header("Input")]
    public KeyCode captureKey = KeyCode.Space;
    public KeyCode resetKey = KeyCode.R;

    [Header("Debug")]
    public bool autoSolveWhenComplete = true;

    private readonly CubeState cube = new CubeState();
    private int currentFaceIndex = 0;
    private bool isSolving = false;

    void Start()
    {
        if (cubeNetUI != null) cubeNetUI.ClearAll();
        cube.Reset();
        currentFaceIndex = 0;

        Debug.Log($"ScanFlow: Ready. Show {CubeState.FaceNames[currentFaceIndex]} and press SPACE.");
    }

    void Update()
    {
        if (Input.GetKeyDown(resetKey))
        {
            ResetAll();
            return;
        }

        if (!Input.GetKeyDown(captureKey)) return;

        if (scanner == null)
        {
            Debug.LogError("ScanFlow: scanner not assigned.");
            return;
        }

        // Ask scanner for current face
        if (!scanner.TryScanCurrentFace(out string[] colors9, out string reason))
        {
            Debug.LogWarning($"ScanFlow: Scan failed: {reason}");
            return;
        }

        // Store + update UI
        cube.SetFace(currentFaceIndex, colors9);

        if (cubeNetUI != null)
            cubeNetUI.SetFaceColors(CubeState.FaceCodes[currentFaceIndex], colors9);

        Debug.Log($"ScanFlow: Captured {CubeState.FaceNames[currentFaceIndex]} => {string.Join(",", colors9)}");

        currentFaceIndex++;

        if (currentFaceIndex < 6)
        {
            Debug.Log($"ScanFlow: Next => show {CubeState.FaceNames[currentFaceIndex]} and press SPACE.");
            return;
        }

        Debug.Log("ScanFlow: All 6 faces captured.");

        if (!cube.TryBuildKociembaStringFixedOrientation(out string cubeString, out string err))
        {
            Debug.LogError("ScanFlow: Cannot build cube string: " + err);
            // Force rescan (you can choose a smarter step-back strategy later)
            currentFaceIndex = 0;
            return;
        }

        Debug.Log("ScanFlow: Kociemba string => " + cubeString);

        if (autoSolveWhenComplete)
            StartCoroutine(SolveCoroutine(cubeString));
    }

    public void ResetAll()
    {
        cube.Reset();
        currentFaceIndex = 0;
        if (cubeNetUI != null) cubeNetUI.ClearAll();
        Debug.Log("ScanFlow: Reset. Show Up and press SPACE.");
    }

    IEnumerator SolveCoroutine(string cubeString)
    {
        if (isSolving) yield break;
        isSolving = true;

        yield return null;

        string info = "";
        string solution = null;

        var t = new Thread(() => { solution = KociembaSolver.Solve(cubeString, out info); });
        t.Start();

        while (t.IsAlive) yield return null;

        Debug.Log("Kociemba info: " + info);
        if (string.IsNullOrWhiteSpace(solution))
        {
            Debug.Log("Cube is already SOLVED ");
        }
        else
        {
            Debug.Log("Kociemba solution: " + solution);
        }

        isSolving = false;
    }
}