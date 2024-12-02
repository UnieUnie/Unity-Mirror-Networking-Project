using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Mirror;

/// <summary>
/// Logic gate contraption that processes input signals based on its type
/// </summary>
public class LogicGate : PuzzleComponent
{
    public enum GateType
    {
        AND,
        OR,
        NOT,
        XOR,
        NAND,
        NOR,
        XNOR
    }

    public static GateType? GetOppositeGateType(GateType currentType)
    {
        switch (currentType)
        {
            case GateType.AND:
                return GateType.NAND;
            case GateType.NAND:
                return GateType.AND;
            case GateType.OR:
                return GateType.NOR;
            case GateType.NOR:
                return GateType.OR;
            case GateType.XOR:
                return GateType.XNOR;
            case GateType.XNOR:
                return GateType.XOR;
            case GateType.NOT:
            default:
                return null;
        }
    }

    [Header("Logic Gate Settings")]
    [SerializeField, SyncVar(hook = nameof(OnGateTypeChanged))]
    public GateType gateType;

    [Tooltip("Components that provide input to this gate")]
    [SerializeField] private List<PuzzleComponent> inputComponents;

    [Header("Material Settings")]
    [SerializeField] private List<MeshRenderer> controlledObjects = new List<MeshRenderer>();
    [SerializeField] private Material onMaterial;
    [SerializeField] private Material offMaterial;
    Material currentMaterial;
    float lastMaterialChange;
    const float MATERIAL_CHANGE_COOLDOWN = 0.1f;

    // Gate Delay variables to avoid the D3D11 Swapchain blue screen crazy ass crash
    float lastUpdateTime;
    const float PROPAGATION_DELAY = 0.02f; // 20ms propagation delay (cool word :) cool variable)
    bool previousOutput;
    bool processingScheduled = false;

    public bool canSwitch;

    // Delegate and event for type changes
    public delegate void GateTypeChangedHandler(LogicGate gate, GateType oldType, GateType newType);
    public event GateTypeChangedHandler GateTypeChanged;

    void Start()
    {
        ValidateInputs();
        lastUpdateTime = Time.time;
        ProcessLogic();
        UpdateMaterials();
    }

    public override void ReceiveSignal()
    {
        if (!processingScheduled)
        {
            processingScheduled = true;
            Invoke(nameof(ProcessLogic), PROPAGATION_DELAY);
        }
    }

    void Update()
    {
        // Handle continuous state updates for flip-flops
        if (Time.time - lastUpdateTime >= PROPAGATION_DELAY)
        {
            ProcessLogic();
            lastUpdateTime = Time.time;
        }
    }

    public GateType GetGateType() => gateType;

    [Command(requiresAuthority = false)]
    public void CmdSetGateType(GateType newType)
    {
        if (gateType != newType)
        {
            gateType = newType;
        }
    }

    void OnGateTypeChanged(GateType oldType, GateType newType)
    {
        GateTypeChanged?.Invoke(this, oldType, newType);

        ValidateInputs();
        ProcessLogic();

        foreach (var input in inputComponents)
        {
            if (input != null)
            {
                input.ReceiveSignal();
            }
        }
    }

    void ProcessLogic()
    {
        if (!ValidateInputs()) return;

        processingScheduled = false;
        previousOutput = outputValue;
        var inputs = inputComponents.Select(x => x.GetOutputValue()).ToArray();
        bool newOutput = false;

        switch (gateType)
        {
            case GateType.AND:
                newOutput = inputs.All(x => x);
                break;
            case GateType.OR:
                newOutput = inputs.Any(x => x);
                break;
            case GateType.NOT:
                newOutput = !inputs[0];
                break;
            case GateType.NAND:
                newOutput = !inputs.All(x => x);
                break;
            case GateType.NOR:
                newOutput = !inputs.Any(x => x);
                break;
            case GateType.XOR:
                newOutput = inputs[0] != inputs[1];
                break;
            case GateType.XNOR:
                newOutput = inputs[0] == inputs[1];
                break;
        }

        if (previousOutput != newOutput)
        {
            outputValue = newOutput;
            Invoke(nameof(DelayedProcessOutput), PROPAGATION_DELAY);
            UpdateMaterials();
        }
    }

    void DelayedProcessOutput()
    {
        ProcessOutput();
    }

    /// <summary>
    /// I've lost track on how it works. it updates material
    /// </summary>
    /// <param name="forceUpdate"> ignore progation? DONT, IT WILL CRASH AND CORRUPT UR DRIVER (first hand experience on 1/12/24 :) ) </param>
    void UpdateMaterials(bool forceUpdate = false)
    {
        // Avoid overloading gpu by delaying update frequency
        if (!forceUpdate && Time.time - lastMaterialChange < MATERIAL_CHANGE_COOLDOWN) return;

        // Toggle between the two material
        Material materialToUse = outputValue ? onMaterial : offMaterial;

        if (!forceUpdate && materialToUse == currentMaterial) return;

        if (!forceUpdate && currentMaterial != null && materialToUse != null && currentMaterial.name == materialToUse.name) return;

        currentMaterial = materialToUse;
        foreach (MeshRenderer renderer in controlledObjects)
        {
            if (renderer != null)
            {
                renderer.sharedMaterial = currentMaterial;
            }
            // why renderer?.sharedMaterial = currentMaterial; doesnt work?
        }
        lastMaterialChange = Time.time;
    }

    /// <summary>
    /// this is a mess, it basically stops processing if required input is wrong (e.g. more than 1 input for NOT)
    /// </summary>
    /// <returns>true if inputs are correct, allow processing. Otherwise false, dont process</returns>
    bool ValidateInputs()
    {
        if (inputComponents == null || inputComponents.Count == 0)
        {
            Debug.LogError($"Gate {gameObject.name} has no input components assigned!");
            return false;
        }

        inputComponents.RemoveAll(x => x == null);

        switch (gateType)
        {
            case GateType.NOT:
                if (inputComponents.Count != 1)
                {
                    return false;
                }
                break;
            case GateType.XOR:
            case GateType.XNOR:
                if (inputComponents.Count != 2)
                {
                    return false;
                }
                break;
            default:
                if (inputComponents.Count < 2)
                {
                    return false;
                }
                break;
        }

        return true;
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        ValidateInputs();
        if (Application.isEditor && !Application.isPlaying)
        {
            UpdateMaterials();
        }
    }
}