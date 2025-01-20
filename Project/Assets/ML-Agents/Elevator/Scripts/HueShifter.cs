using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HueShifter : MonoBehaviour
{
    private Renderer objectRenderer;
    private float hue;

    // Start is called before the first frame update
    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        hue = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        hue += Time.deltaTime * 0.1f; // Adjust the speed of hue shift here
        if (hue > 1f)
        {
            hue -= 1f;
        }

        Color newColor = Color.HSVToRGB(hue, 1f, 1f);
        objectRenderer.material.color = newColor;
    }
}
