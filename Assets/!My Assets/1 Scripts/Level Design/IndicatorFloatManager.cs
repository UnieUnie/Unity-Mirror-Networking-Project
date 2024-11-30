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

    [SerializeField] Transform lookAtObject;

    [Tooltip("Sprite FaceAt Camera Angle Offset")]
    [SerializeField] Vector3 lookAtOffset = new(0f, 0f, 0f);

    [Header("Float Settings")]
    [Tooltip("How far the object will move up and down")]
    [SerializeField] float floatAmplitude = 0.1f;

    [Tooltip("Floating Speed")]
    [SerializeField] float floatSpeed = 1.5f;

    [Header("Logic Gates")]
    [SerializeField] List<LogicGate> logicGates = new List<LogicGate>();

    // Kvp for gate and indicator
    Dictionary<LogicGate, GameObject> gateIndicators = new Dictionary<LogicGate, GameObject>();

    Dictionary<GameObject, float> timeOffsets = new Dictionary<GameObject, float>();

    private void FixedUpdate()
    {
        if (lookAtObject == null) return;

        foreach (var kvp in gateIndicators)
        {
            LogicGate gate = kvp.Key;
            GameObject indicator = kvp.Value;

            if (gate == null || indicator == null) continue;

            // Update position
            Vector3 basePosition = gate.transform.position + (Vector3.up * verticalOffset);
            float yOffset = floatAmplitude * Mathf.Sin((Time.time + timeOffsets[indicator]) * floatSpeed);
            indicator.transform.position = basePosition + (Vector3.up * yOffset);

            // Update rotation
            Vector3 directionToTarget = lookAtObject.position - indicator.transform.position;
            if (directionToTarget != Vector3.zero)
            {
                indicator.transform.rotation = Quaternion.LookRotation(directionToTarget)
                    * Quaternion.Euler(lookAtOffset);
            }

            if (indicator.transform.localScale != spriteScale)
            {
                indicator.transform.localScale = spriteScale;
            }
        }
    }

    private void OnEnable()
    {
        foreach (var gate in logicGates)
        {
            if (gate != null)
            {
                gate.GateTypeChanged += OnGateTypeChanged;
            }
        }
    }

    private void OnDisable()
    {
        foreach (var gate in logicGates)
        {
            if (gate != null)
            {
                gate.GateTypeChanged -= OnGateTypeChanged;
            }
        }
    }

    private void Awake()
    {
        if (lookAtObject == null)
        {
            // Find the Console Camera by name and tag
            Camera[] cameras = Camera.allCameras;
            foreach (Camera cam in cameras)
            {
                if (cam.gameObject.name == "Console Camera" && cam.CompareTag("MainCamera"))
                {
                    lookAtObject = cam.transform;
                    break;
                }
            }
        }

        SpawnAllIndicators();
    }

    void OnGateTypeChanged(LogicGate gate, LogicGate.GateType oldType, LogicGate.GateType newType)
    {
        UpdateGateIndicator(gate);
    }

    void SpawnAllIndicators()
    {
        foreach (var gate in logicGates)
        {
            if (gate != null)
            {
                SpawnIndicator(gate);
            }
        }
    }

     void SpawnIndicator(LogicGate gate)
    {
        if (gate == null) return;

        Sprite selectedSprite = GetSpriteForGate(gate);

        GameObject indicator = new GameObject($"Indicator_{gate.GetGateType()}Gate");
        indicator.transform.SetParent(null);

        var spriteRenderer = indicator.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = selectedSprite;

        indicator.transform.position = gate.transform.position + (Vector3.up * verticalOffset);
        indicator.transform.localScale = spriteScale;

        gateIndicators[gate] = indicator;
        timeOffsets[indicator] = Random.Range(0f, Mathf.PI * 2);
    }

    void UpdateGateIndicator(LogicGate gate)
    {
        if (gateIndicators.TryGetValue(gate, out GameObject indicator))
        {
            var spriteRenderer = indicator.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = GetSpriteForGate(gate);
                indicator.name = $"Indicator_{gate.GetGateType()}Gate";
            }
        }
    }

    Sprite GetSpriteForGate(LogicGate gate)
    {
        return gate.GetGateType() switch
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
    }

    /// <summary>
    /// These methods are designed to be used with TMP button's onClick()
    /// 
    /// </summary>
    #region Public Method To Control Indicators
    public void EnableAllIndicators()
    {
        foreach (var indicator in gateIndicators.Values)
        {
            indicator?.SetActive(true);
        }
    }
    public void DisableAllIndicators()
    {
        foreach (var indicator in gateIndicators.Values)
        {
            if (indicator != null)
            {
                indicator.SetActive(false);
            }
        }
    }


    public void DeleteIndicator(LogicGate gate)
    {
        if (gateIndicators.TryGetValue(gate, out var indicator))
        {
            if (indicator != null)
            {
                Destroy(indicator);
            }
        }

        gateIndicators.Remove(gate);
        timeOffsets.Remove(indicator);
    }

    public void DeleteAllIndicators()
    {
        foreach (var indicator in gateIndicators.Values)
        {
            if (indicator != null)
            {
                Destroy(indicator);
            }
        }
        gateIndicators.Clear();
        timeOffsets.Clear();
    }

    public void RespawnAllIndicators()
    {
        DeleteAllIndicators();
        SpawnAllIndicators();
    }

    public void DisableIndicator(LogicGate gate)
    {
        if (gateIndicators.TryGetValue(gate, out var indicator))
        {
            indicator?.SetActive(false);
        }
    }

    public void EnableIndicator(LogicGate gate)
    {
        if (gateIndicators.TryGetValue(gate, out var indicator))
        {
            indicator?.SetActive(false);
        }
    }

    public void RespawnIndicator(LogicGate gate)
    {
        DeleteIndicator(gate);
        SpawnIndicator(gate);
    }
    #endregion

    void OnDestroy()
    {
        DeleteAllIndicators();
    }

    void OnValidate()
    {
        // crazy looking code
        // removes all gate that returns null
        // i.e remove every null references of gate
        logicGates.RemoveAll(gate => gate == null);
    }
}