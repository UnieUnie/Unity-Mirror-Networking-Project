using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateUtil : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("The axis around which the object will rotate.")]
    public Vector3 rotationAxis = Vector3.up;

    [Tooltip("The speed of the rotation in degrees per second.")]
    public float rotationSpeed = 30f;

    void Update()
    {
        // Apply constant rotation
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime, Space.Self);
    }
}