using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        // Sets both position and rotation to the source camera
        //targetCamera.transform.SetPositionAndRotation(sourceCamera.transform.position, sourceCamera.transform.rotation);

        //targetCamera.transform.localScale = sourceCamera.transform.localScale;
        targetCamera.fieldOfView = sourceCamera.fieldOfView;
    }

}
