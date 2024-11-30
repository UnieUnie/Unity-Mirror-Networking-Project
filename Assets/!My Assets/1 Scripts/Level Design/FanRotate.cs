using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FanRotate : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Degree per sec")]
    [SerializeField] float rotationSpeed = 30f;

    [Tooltip("is it rotating? true if start as rotating")]
    [SerializeField] bool isRotating = true;

    Vector3 rotatePivot; // The fan's pivot is wrong, this is used by mesh filter to fix it

    public PuzzleResult puzzleResult; // Reference to PuzzleResult

    void Awake()
    {
        puzzleResult.onCorrectResult.AddListener(this.StopRotating);

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        rotatePivot = transform.TransformPoint(meshFilter.mesh.bounds.center);
    }
    void OnDestroy()
    {
        puzzleResult?.onCorrectResult.RemoveListener(this.StopRotating);

    }

    void FixedUpdate()
    {
        if (isRotating)
        {
            transform.RotateAround(rotatePivot, Vector3.forward, rotationSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Start Rotation
    /// </summary>
    public void StartRotating()
    {
        isRotating = true;
    }

    /// <summary>
    /// Stop Rotation
    /// </summary>
    public void StopRotating()
    {
        isRotating = false;
    }

    /// <summary>
    /// Toggle Rotation (!=)
    /// </summary>
    public void ToggleRotation()
    {
        isRotating = !isRotating;
    }

    /// <summary>
    /// Change otation speed.
    /// </summary>
    /// <param name="axis"></param>
    /// <param name="speed"> Degree per seconds</param>
    public void SetRotation(float speed)
    {
        rotationSpeed = speed;
    }
}
