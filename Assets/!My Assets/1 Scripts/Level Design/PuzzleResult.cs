using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

/// <summary>
/// Result component that checks if the received signal matches the expected value
/// </summary>
public class PuzzleResult : PuzzleComponent
{
    [Header("Result Settings")]
    [SerializeField] bool expectedValue;
    public UnityEvent onCorrectResult;
    public UnityEvent onIncorrectResult;

    [Tooltip("Component providing input for the result")]
    [SerializeField] PuzzleComponent inputComponent;

    [Header("Material Settings")]
    [SerializeField] List<MeshRenderer> controlledObjects = new List<MeshRenderer>();
    [SerializeField] Material onMaterial;
    [SerializeField] Material offMaterial;

    Material currentMaterial;
    float lastMaterialChange;
    const float MATERIAL_CHANGE_COOLDOWN = 0.1f;

    bool wasCorrect;

    [Header("Level Progression")]
    [SerializeField] int levelNumber;
    public bool completeState = false;
    float completeStateTimer = 0f;
    float completionRequiredTime = 3f;
    bool hasTriggeredCompletion = false;

    private void Start()
    {
        ValidateInput();
        CheckResult();
        UpdateMaterials();
    }

    void Update()
    {
        if (completeState && !hasTriggeredCompletion)
        {
            completeStateTimer += Time.deltaTime;

            if (completeStateTimer >= completionRequiredTime)
            {
                SetLevelComplete();
                hasTriggeredCompletion = true;
            }
        }
        else if (!completeState)
        {
            completeStateTimer = 0f;
            hasTriggeredCompletion = false;
        }
    }

    void SetLevelComplete()
    {
        if (levelNumber < 1 || levelNumber > 8)
        {
            Debug.LogWarning($"Invalid level number {levelNumber} on PuzzleResult");
            return;
        }
    }


    bool ValidateInput() => inputComponent != null;

    public override void ReceiveSignal()
    {
        if (!ValidateInput()) return;

        outputValue = inputComponent.GetOutputValue();

        CheckResult();
        UpdateMaterials();
    }

    /// <summary>
    /// Using unity events.
    /// Require script to add listners for them invokeesss
    /// </summary>
    void CheckResult()
    {
        bool isCorrect = (outputValue == expectedValue);
        completeState = isCorrect;

        if (isCorrect)
        {
            onCorrectResult.Invoke();
        }
        else if (!isCorrect)
        {
            onIncorrectResult.Invoke();
        }
    }

    /// <summary>
    /// I've lost track on how it works. it updates material
    /// </summary>
    /// <param name="forceUpdate"> ignore progation? DONT, IT WILL CRASH AND CORRUPT UR DRIVER (first hand experience on 1/12/24 :) ) </param>
    void UpdateMaterials(bool forceUpdate = false)
    {
        // Avoid overloading gpu by delaying update frequency
        if (!forceUpdate && Time.time - lastMaterialChange < MATERIAL_CHANGE_COOLDOWN) return;

        bool inputValue = inputComponent.GetOutputValue();
        Material materialToUse = inputValue ? onMaterial : offMaterial;

        if (!forceUpdate && materialToUse == currentMaterial) return;

        if (!forceUpdate && currentMaterial != null && materialToUse != null && currentMaterial.name == materialToUse.name) return;

        currentMaterial = materialToUse;
        foreach (MeshRenderer renderer in controlledObjects)
        {
            if (renderer != null)
            {
                renderer.sharedMaterial = currentMaterial;
            }
        }
        lastMaterialChange = Time.time;
    }
}