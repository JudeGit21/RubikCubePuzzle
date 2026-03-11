using System;
using UnityEngine;
using UnityEngine.UI;

public class CubletFaceUI : MonoBehaviour
{
    public Color[] colors = new Color[6] { Color.white, Color.red, Color.yellow, new Color(255, 87, 0), Color.green, Color.blue };

    public bool locked = false;
    public int startColorIndex = 0;

    private Image image;
    private Button button;

    private int currentColorIndex;
    void Start()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();

        currentColorIndex = startColorIndex - 1;
        GoToNextColor();

        if (locked)
            return;

        button.onClick.AddListener(() => 
            {
                GoToNextColor();
            }
        );
    }

    private void GoToNextColor()
    {
        currentColorIndex++;
        if ( currentColorIndex >= colors.Length )
            currentColorIndex = 0;

        image.color = colors[currentColorIndex];
    }
}
