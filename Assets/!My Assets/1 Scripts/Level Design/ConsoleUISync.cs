using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConsoleUISync : MonoBehaviour
{
    #region Variables and References
    //----------------------------------------------------------------------------------------------------
    // UI and Graphic References
    [Header("UI and Renderer References")]
    [Tooltip("TMP Text to monitor for change (Component/Gate Type)")]
    [SerializeField] TextMeshProUGUI textToMonitor;

    [Tooltip("The material when text change detected (drop in the object needing change)")]
    [SerializeField] MeshRenderer materialToUpdate;

    [Tooltip("The sprite when text change detected (drop in the object needing change)")]
    [SerializeField] Image spriteToUpdate;

    // Fallback References
    [Header("Fallback References")]
    [Tooltip("Fallback material for empty gates or errors")]
    [SerializeField] Material fallbackMaterial;

    [Tooltip("Fallback sprite for empty gates or errors")]
    [SerializeField] Sprite fallBackSprite;

    // Gate Graphics
    [Header("Gate Graphics")]
    [Tooltip("Array of gate materials. To be applied as the material of the current active gate")]
    [SerializeField] Material[] gateMaterials;

    [Tooltip("Array of gate sprites. To be applied as the sprite of the current active gate")]
    [SerializeField] Sprite[] gateSprites;

    // Gate Types, used to determine what material/sprite to use
    readonly string[] gateTypes = { "XNOR", "NAND", "XOR", "NOR", "AND", "NOT", "OR" };

    string lastText; // Used to compare and detect changes against textToMonitor
    //----------------------------------------------------------------------------------------------------
    [Header("Update Control")] // Hopefully this stops D3D11 swapchain error lol
    [Tooltip("Minimum time (in seconds) between material updates")]
    [SerializeField]
    float materialChangeCooldown = 0.1f;
    float lastMaterialUpdate; // Tracks time since last material update
    //----------------------------------------------------------------------------------------------------
    #endregion

    void Start()
    {
        // immdeiatly allow 
        lastMaterialUpdate = -materialChangeCooldown;
    }

    void Update()
    {
        if (textToMonitor == null) return;

        string currentText = textToMonitor.text;

        if (currentText != lastText)
        {
            // by slowing down text update, it stops mat/sprite update to desync...
            if (Time.time - lastMaterialUpdate < materialChangeCooldown) return;

            lastText = currentText;
            UpdateVisuals(false);
            lastMaterialUpdate = Time.time;
        }
    }

    void UpdateVisuals(bool forceUpdate = false)
    {
        if (!forceUpdate && Time.time - lastMaterialUpdate < materialChangeCooldown) return;

        // Loop through every gateTypes and find the matching type
        foreach (string gateType in gateTypes)
        {
            if (lastText.ToUpper().Contains(gateType.ToUpper()))
            {
                // Apply updated material
                if (materialToUpdate != null)
                {
                    Material matchingMaterial = GetMaterialByName("Gate " + gateType);
                    materialToUpdate.material = matchingMaterial != null ? matchingMaterial : fallbackMaterial;
                }

                // APply updated sprite
                if (spriteToUpdate != null)
                {
                    Sprite matchingSprite = GetSpriteByName(gateType + " Sprite");
                    spriteToUpdate.sprite = matchingSprite != null ? matchingSprite : fallBackSprite;
                }
                return;
            }
        }

        // If no match found, use fallback
        if (materialToUpdate != null) materialToUpdate.material = fallbackMaterial;
        if (spriteToUpdate != null) spriteToUpdate.sprite = fallBackSprite;

        lastMaterialUpdate = Time.time;
    }

    /// <summary>
    /// Find material from the gateMaterial array that matches targetName
    /// </summary>
    /// <param name="targetName">Set by UpdateVisuals using ("Gate " + gateType). gateType is from the list gateTypes</param>
    /// <returns></returns>
    Material GetMaterialByName(string targetName)
    {
        foreach (Material material in gateMaterials)
        {
            if (material != null && material.name.ToUpper().Contains(targetName.ToUpper()))
            {
                return material;
            }
        }
        return null;
    }

    /// <summary>
    /// Find sprite from the gateSprite array that matches targetName
    /// </summary>
    /// <param name="targetName">Set by UpdateVisuals using (gateType + " Sprite"). GateType is from the list gateTypes</param>
    /// <returns></returns>
    Sprite GetSpriteByName(string targetName)
    {
        foreach (Sprite sprite in gateSprites)
        {
            if (sprite != null && sprite.name.ToUpper().Contains(targetName.ToUpper()))
            {
                return sprite;
            }
        }
        return null;
    }
}