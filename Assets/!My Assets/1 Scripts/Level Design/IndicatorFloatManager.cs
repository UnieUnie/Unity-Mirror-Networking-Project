using UnityEngine;
using System.Collections.Generic;

public class IndicatorFloatManager : MonoBehaviour
{
    /// <summary>
    /// They are individual sprite instead of a list is because....
    /// u can drag and drop the sprites onto the script without attaching it on to anything.
    /// u cant do that with List. and im too lazy to make a scriptable object.
    /// </summary>
    [Header("Gate Sprites")] // Used to make a list of sprites to spawn...
    [SerializeField] Sprite andGateSprite;
    [SerializeField] Sprite nandGateSprite;
    [SerializeField] Sprite orGateSprite;
    [SerializeField] Sprite norGateSprite;
    [SerializeField] Sprite xorGateSprite;
    [SerializeField] Sprite xnorGateSprite;
    [SerializeField] Sprite notGateSprite;

    [Header("Display Settings")]
    [Tooltip("Sprite Scale (Default 0.7, 0.7, 0.7)")]
    [SerializeField] Vector3 spriteScale = new(0.7f, 0.7f, 0.7f);

    [Tooltip("Sprite Spawn Height Offset")]
    [SerializeField] float verticalOffset = 1.5f;

    [System.Serializable]
    public class LevelGroup
    {
        public GameObject levelObject;
        public List<LogicGate> gatesInLevel = new List<LogicGate>();
    }

    [Header("Level Groups")]
    [SerializeField] LevelGroup[] levelGroups = new LevelGroup[5];
    Dictionary<LogicGate, GameObject> gateIndicators = new Dictionary<LogicGate, GameObject>();

    [Header("Update Control")] // Stops D3D11 swapchain error (maybe)
    [Tooltip("Minimum time (in seconds) between material updates")]
    [SerializeField]
    float materialChangeCooldown = 0.1f;
    float lastMaterialUpdate; // Tracks time since last material update
    Dictionary<LogicGate, LogicGate.GateType> lastKnownTypes = new Dictionary<LogicGate, LogicGate.GateType>();



    void Awake()
    {
        SpawnAllIndicators();
        UpdateAllLevelStates();

        lastMaterialUpdate = Time.time;
    }

    void Update()
    {
        UpdateAllLevelStates();
    }

    void SpawnAllIndicators()
    {
        foreach (var group in levelGroups)
        {
            foreach (var gate in group.gatesInLevel)
            {
                if (gate != null) SpawnIndicator(gate);
                lastKnownTypes[gate] = gate.GetGateType();
            }
        }
    }

    void SpawnIndicator(LogicGate gate)
    {
        if (gate == null) return;

        GameObject indicator = new GameObject($"Indicator_{gate.GetGateType()}Gate");
        var spriteRenderer = indicator.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = GetSpriteForGate(gate);

        indicator.transform.position = gate.transform.position + (Vector3.up * verticalOffset);
        indicator.transform.localScale = spriteScale;

        gateIndicators[gate] = indicator;
        indicator.SetActive(false); // Start as disabled
    }

    void UpdateAllLevelStates()
    {
        foreach (var group in levelGroups)
        {
            if (group.levelObject == null) continue;
            UpdateLevelState(group);

            if (Time.time - lastMaterialUpdate >= materialChangeCooldown)
            {
                CheckGateTypeChanges();
                lastMaterialUpdate = Time.time;
            }
        }
    }

    void UpdateLevelState(LevelGroup group)
    {
        bool isLevelEnabled = group.levelObject.activeSelf;
        foreach (var gate in group.gatesInLevel)
        {
            if (gate != null && gateIndicators.TryGetValue(gate, out GameObject indicator))
            {
                indicator.SetActive(isLevelEnabled);
            }
        }
    }

    Sprite GetSpriteForGate(LogicGate gate) => gate.GetGateType() switch
    {
        LogicGate.GateType.AND => andGateSprite,
        LogicGate.GateType.OR => orGateSprite,
        LogicGate.GateType.NOT => notGateSprite,
        LogicGate.GateType.NAND => nandGateSprite,
        LogicGate.GateType.NOR => norGateSprite,
        LogicGate.GateType.XOR => xorGateSprite,
        LogicGate.GateType.XNOR => xnorGateSprite,
        _ => null
    };

    void CheckGateTypeChanges()
    {
        foreach (var gate in gateIndicators.Keys)
        {
            if (gate == null) continue;

            LogicGate.GateType currentType = gate.GetGateType();
            if (lastKnownTypes.TryGetValue(gate, out LogicGate.GateType lastType) && currentType != lastType)
            {
                UpdateGateSprite(gate, currentType);
                lastKnownTypes[gate] = currentType;
            }
        }
    }

    void UpdateGateSprite(LogicGate gate, LogicGate.GateType newType)
    {
        if (gateIndicators.TryGetValue(gate, out GameObject indicator))
        {
            var spriteRenderer = indicator.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetSpriteForGate(gate);
            indicator.name = $"Indicator_{newType}Gate";
        }
    }

    private void OnValidate()
    {
        if (levelGroups.Length != 5)
        {
            System.Array.Resize(ref levelGroups, 5);
        }
    }

    private void OnDestroy()
    {
        foreach (var indicator in gateIndicators.Values)
        {
            if (indicator) Destroy(indicator);
        }
        gateIndicators.Clear();
        lastKnownTypes.Clear();
    }
}