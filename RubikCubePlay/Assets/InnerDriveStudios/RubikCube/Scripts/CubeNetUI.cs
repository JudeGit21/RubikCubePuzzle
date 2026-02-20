using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CubeNetUI : MonoBehaviour
{
    [Header("Face Containers (each should contain 9 Image children in order 0..8)")]
    public RectTransform faceU;
    public RectTransform faceR;
    public RectTransform faceF;
    public RectTransform faceD;
    public RectTransform faceL;
    public RectTransform faceB;

    private readonly Dictionary<string, Image[]> faceImages = new Dictionary<string, Image[]>();

    void Awake()
    {
        faceImages["U"] = Collect9(faceU, "U");
        faceImages["R"] = Collect9(faceR, "R");
        faceImages["F"] = Collect9(faceF, "F");
        faceImages["D"] = Collect9(faceD, "D");
        faceImages["L"] = Collect9(faceL, "L");
        faceImages["B"] = Collect9(faceB, "B");
    }

    Image[] Collect9(RectTransform rt, string code)
    {
        if (rt == null) { Debug.LogError($"CubeNetUI: Missing face container {code}"); return new Image[9]; }
        var imgs = rt.GetComponentsInChildren<Image>(true);

        // We expect 9 images (excluding possible background image on parent).
        // If parent has an Image too, it will be included first — so filter by direct children if needed.
        var list = new System.Collections.Generic.List<Image>();
        for (int i = 0; i < rt.childCount; i++)
        {
            var img = rt.GetChild(i).GetComponent<Image>();
            if (img != null) list.Add(img);
        }

        if (list.Count == 9) return list.ToArray();

        // fallback: try all images but take last 9
        if (imgs.Length >= 9)
        {
            var arr = new Image[9];
            Array.Copy(imgs, imgs.Length - 9, arr, 0, 9);
            return arr;
        }

        Debug.LogError($"CubeNetUI: Face {code} does not have 9 Image children.");
        return new Image[9];
    }

    public void ClearAll()
    {
        foreach (var kv in faceImages)
        {
            foreach (var img in kv.Value)
            {
                if (img == null) continue;
                img.color = Color.black;
            }
        }
    }

    public void SetFaceColors(string faceCode, string[] colors9)
    {
        if (!faceImages.TryGetValue(faceCode, out var imgs) || imgs == null || imgs.Length != 9) return;
        if (colors9 == null || colors9.Length != 9) return;

        for (int i = 0; i < 9; i++)
        {
            if (imgs[i] == null) continue;
            imgs[i].color = ToUnityColor(colors9[i]);
        }
    }

    // Map your classifier strings -> UI color
    Color ToUnityColor(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return Color.black;
        name = name.ToLowerInvariant();

        return name switch
        {
            "white" => Color.white,
            "yellow" => Color.yellow,
            "red" => Color.red,
            "green" => Color.green,
            "blue" => Color.blue,
            "orange" => new Color(1f, 0.5f, 0f),
            _ => new Color(0.2f, 0.2f, 0.2f) // unknown
        };
    }
}