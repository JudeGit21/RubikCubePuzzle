using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CubeState
{
    // Order must be U, R, F, D, L, B
    public static readonly string[] FaceCodes = { "U", "R", "F", "D", "L", "B" };
    public static readonly string[] FaceNames = { "Up", "Right", "Front", "Down", "Left", "Back" };

    // Store scanned color-names ("white","red",...) per face
    private readonly string[][] faces = new string[6][];

    public int CapturedCount => faces.Count(f => f != null);

    public void Reset()
    {
        for (int i = 0; i < 6; i++) faces[i] = null;
    }

    public bool HasFace(int faceIndex) => faces[faceIndex] != null;

    public void SetFace(int faceIndex, string[] colors9)
    {
        if (colors9 == null || colors9.Length != 9)
            throw new ArgumentException("colors9 must be length 9");

        faces[faceIndex] = colors9.ToArray();
    }

    public string[] GetFace(int faceIndex) => faces[faceIndex];

    public bool IsComplete() => faces.All(f => f != null && f.Length == 9);

    /// <summary>
    /// Builds Kociemba facelet string (54 chars) in order U R F D L B.
    /// Uses centers to map color-name -> face letter.
    /// </summary>
    public bool TryBuildKociembaStringFixedOrientation(out string cubeString, out string error)
    {
        cubeString = null;
        error = null;

        if (!IsComplete())
        {
            error = "Not all 6 faces captured.";
            return false;
        }

        // Expected centers in URFDLB order
        string[] expectedCenters = { "white", "red", "green", "yellow", "orange", "blue" };

        for (int f = 0; f < 6; f++)
        {
            var center = faces[f][4]?.ToLowerInvariant();
            if (center != expectedCenters[f])
            {
                error = $"Wrong orientation: {FaceCodes[f]} center expected '{expectedCenters[f]}' but got '{center}'. " +
                        $"Hold cube fixed (white Up, green Front) and rescan that face.";
                return false;
            }
        }

        // Hard map color -> face letter
        char Map(string c)
        {
            c = (c ?? "").ToLowerInvariant();
            return c switch
            {
                "white" => 'U',
                "red" => 'R',
                "green" => 'F',
                "yellow" => 'D',
                "orange" => 'L',
                "blue" => 'B',
                _ => '?' // unknown
            };
        }

        char[] out54 = new char[54];
        int idx = 0;

        for (int f = 0; f < 6; f++)
        {
            for (int i = 0; i < 9; i++)
            {
                char ch = Map(faces[f][i]);
                if (ch == '?')
                {
                    error = $"Unknown color on {FaceCodes[f]} index {i}. Improve lighting and rescan.";
                    return false;
                }
                out54[idx++] = ch;
            }
        }

        cubeString = new string(out54);

        // Validate counts
        foreach (char ch in new[] { 'U', 'R', 'F', 'D', 'L', 'B' })
        {
            int count = 0;
            for (int i = 0; i < cubeString.Length; i++) if (cubeString[i] == ch) count++;
            if (count != 9)
            {
                error = $"Invalid counts: expected 9 of {ch} but got {count}. Rescan (lighting/orientation).";
                return false;
            }
        }

        return true;
    }
}