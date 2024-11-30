using UnityEngine;
using System.Collections.Generic;

public class PressurePlate : PuzzleComponent
{
    #region Variables and References
    //----------------------------------------------------------------------------------------------------
    [Header("Pressure Plate Settings")]
    [SerializeField] float moveDistance = 0.5f;
    [SerializeField] float moveSpeed = 2f;

    [Header("Material Settings")]
    [SerializeField] List<MeshRenderer> controlledObjects = new List<MeshRenderer>();
    [SerializeField] Material onMaterial;
    [SerializeField] Material offMaterial;

    Vector3 originalPosition;
    Vector3 pressedPosition;
    bool isPressed;

    Material currentMaterial;
    float lastMaterialChange;
    const float MATERIAL_CHANGE_COOLDOWN = 0.1f;
    #endregion


    #region Unity Methods
    //----------------------------------------------------------------------------------------------------
    void Start()
    {
        originalPosition = transform.position;
        pressedPosition = originalPosition + Vector3.down * moveDistance;
        outputValue = false;
        ProcessOutput();
        currentMaterial = offMaterial;
        UpdateMaterials(true);
    }

    void Update()
    {
        Vector3 targetPosition = isPressed ? pressedPosition : originalPosition;
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        Debug.Log("Enter Once");

        isPressed = true;
        outputValue = true;
        ProcessOutput();
        UpdateMaterials();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        Debug.Log("Exit Once");

        isPressed = false;
        outputValue = false;
        ProcessOutput();
        UpdateMaterials();
    }
    #endregion

    void UpdateMaterials(bool forceUpdate = false)
    {
        if (!forceUpdate && Time.time - lastMaterialChange < MATERIAL_CHANGE_COOLDOWN) return;

        Material materialToUse = outputValue ? onMaterial : offMaterial;

        // Return if the material to use is the same as current
        if (!forceUpdate && materialToUse == currentMaterial) return;

        // Additional check for material properties being equal
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

    protected override void OnValidate()
    {
        base.OnValidate();
    }
}