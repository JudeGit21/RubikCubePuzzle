using UnityEngine;
using System.Collections.Generic;

public class CubeColorCalibrator : MonoBehaviour
{
    public Dictionary<char, Color> calibratedColors = new Dictionary<char, Color>();

    public void Calibrate(char faceColor, Color sample)
    {
        if (calibratedColors.ContainsKey(faceColor))
            calibratedColors[faceColor] = sample;
        else
            calibratedColors.Add(faceColor, sample);

        Debug.Log("Calibrated " + faceColor + " = " + sample);
    }

    public char MatchColor(Color sample)
    {
        char bestMatch = 'U';
        float bestDistance = 999f;

        foreach (var pair in calibratedColors)
        {
            float d = ColorDistance(sample, pair.Value);

            if (d < bestDistance)
            {
                bestDistance = d;
                bestMatch = pair.Key;
            }
        }

        return bestMatch;
    }

    float ColorDistance(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r)
             + Mathf.Abs(a.g - b.g)
             + Mathf.Abs(a.b - b.b);
    }
}