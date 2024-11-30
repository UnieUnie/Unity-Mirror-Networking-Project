using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Syncs FOV of the overlay camera with the console display's camera.
/// </summary>
public class OverlayCameraController : MonoBehaviour
{
    [Header("Camera References")]
    [Tooltip("Source Camera To Copy Data From")]
    [SerializeField] Camera sourceCamera;

    [Tooltip("Target Camera To Paste Data To")]
    [SerializeField] Camera targetCamera;

    void Update()
    {
        SyncCameraProperties();
    }

    void SyncCameraProperties()
    {
        targetCamera.fieldOfView = sourceCamera.fieldOfView;
    }
}
