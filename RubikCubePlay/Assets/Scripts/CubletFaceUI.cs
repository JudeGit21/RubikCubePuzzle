using System;
using UnityEngine;
using UnityEngine.UI;

public class CubletFaceUI : MonoBehaviour
{
    public event Action<int, int, int> OnSetColor;

    public Color[] colors = new Color[6] { Color.white, Color.red, Color.yellow, new Color(255, 87, 0), Color.green, Color.blue };

    public bool locked = false;
    public int startColorIndex = 0;

    public Image[] images;
    private Button button;

    private int currentColorIndex;
    void Start()
    {
        button = GetComponent<Button>();

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

        foreach ( var image in images )
            image.color = colors[currentColorIndex];

        OnSetColor?.Invoke(transform.GetSiblingIndex(), startColorIndex, currentColorIndex);
    }
}
