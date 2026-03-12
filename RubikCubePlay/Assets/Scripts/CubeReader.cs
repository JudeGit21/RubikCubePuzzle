using CubeXdotNET;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class CubeReader : MonoBehaviour
{
    public string cube;

    private char[] colorIndexToColorChar = new char[6] { 'w', 'r', 'y', 'o', 'g', 'b' };
    private int[] faceIndexToColorIndex = new int[6] { 5, 3, 4, 1, 0, 2 };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        cube = "gggggggggooooooooobbbbbbbbbrrrrrrrrryyyyyyyyywwwwwwwww";

        foreach (var cublet in FindObjectsByType<CubletFaceUI>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            cublet.OnSetColor += SetCublet;
        }
    }

    [ContextMenu("Solve")]
    public void Solve()
    {
        FridrichSolver Solver = new FridrichSolver(cube);

        Solver.Solve();

        if (Solver.IsSolved)
        {
            Debug.Log($"Solution ({Solver.Length} Moves) : {Solver.Solution}");
        }
        else
        {
            Debug.Log("Did not solve");
        }
    }

    public void SetCublet(int cubletIndex, int faceIndex, int colorIndex)
    {
        cube = new StringBuilder(cube) { [faceIndexToColorIndex[faceIndex] * 9 + cubletIndex] = colorIndexToColorChar[colorIndex] }.ToString();
    }

    private void OnDestroy()
    {
        foreach (var cublet in FindObjectsByType<CubletFaceUI>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            cublet.OnSetColor -= SetCublet;
        }
    }
}
