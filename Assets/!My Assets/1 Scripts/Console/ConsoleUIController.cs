using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.EventSystems;

public class ConsoleUIController : NetworkBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] Camera consoleViewCamera;
    [SerializeField] float uiRayCastRange = 2f;
    [SerializeField] LayerMask uiLayerMask;
    [SerializeField] LayerMask ignoredLayerMask;


    [Header("UI Settings")]
    [SerializeField] Canvas uiCanvas;

    public delegate void MouseActionHandler(int buttonIndex);
    public MouseActionHandler OnMouseAction;

    void Start()
    {
        OnMouseAction = HandleMouseAction;
        uiLayerMask = 1 << LayerMask.NameToLayer("UI");
        ignoredLayerMask = ~LayerMask.GetMask("Default");
    }

    void Update()
    {
        
    }

    void HandleMouseAction(int buttonIndex)
    {
        Ray ray = consoleViewCamera.ScreenPointToRay(Input.mousePosition);


        // raycasts
        if (Physics.Raycast(ray, out RaycastHit hit, uiRayCastRange, uiLayerMask & ignoredLayerMask))
        {
            Debug.Log($"Mouse Button {buttonIndex}: Hit {hit.collider.gameObject.name} at position {hit.point}");
            Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green); // Hit indicator
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction * uiRayCastRange, Color.red); // Miss indicator
        }


    }
}
