using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextColourCycle : MonoBehaviour
{
    [Header("Colour Cycle Settings")]
    [Tooltip("Speed of cycling")]
    [SerializeField] float cycleSpeed = 1.0f;

    TMP_Text tmpText;

    void Start()
    {
        tmpText = GetComponent<TMP_Text>();
    }

    void Update()
    {
        if (tmpText == null) return;

        float hue = Mathf.Repeat(Time.time * cycleSpeed, 1.0f);
        Color newColor = Color.HSVToRGB(hue, 1.0f, 1.0f);
        tmpText.color = newColor;
    }
}
