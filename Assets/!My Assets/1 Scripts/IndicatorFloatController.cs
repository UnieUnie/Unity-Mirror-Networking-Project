using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndicatorFloatController : MonoBehaviour
{
    [Header("Sprite Settings")]
    [Tooltip("The Sprite To Spawn & Float")]
    [SerializeField] Sprite spriteObject;

    [Tooltip("Sprite Scale (Default 0.7, 0.7, 0.7)")]
    [SerializeField] Vector3 spriteScale = new(0.7f, 0.7f, 0.7f);

    [Tooltip("Sprite Spawn Height Offset")]
    [SerializeField] float verticalOffset = 1.5f;

    [Tooltip("Sprute Facing Object")]
    [SerializeField] Transform lookAtObject;

    [Tooltip("Sprite Facing Camera Angle Offset")]
    [SerializeField] Vector3 lookAtOffset = new(90, 0f, 180f);

    [Header("Float Settings")]
    [Tooltip("How far the object will move up and down")]
    [SerializeField] float floatAmplitude = 0.1f;

    [Tooltip("Floating Speed")]
    [SerializeField] float floatSpeed = 1.5f;

    GameObject gateIndicator;   // GameOvject for the sprite component
    Vector3 initialPosition; // Initial position to spawn and float the sprite

    void Start()
    {
        SpawnSprite();
    }

    void FixedUpdate()
    {
        FloatAndLookAtMethod();
    }

    /// <summary>
    /// Spawns the sprite with correct position, rotation and scale.
    /// </summary>
    void SpawnSprite()
    {
        if (spriteObject == null)
        {
            Debug.LogError("No Sprite Assigned");
            return;
        }

        // Create sprite's object
        gateIndicator = new GameObject(spriteObject.name);
        gateIndicator.transform.SetParent(transform);

        // Add SpriteRenderer and set sprite
        SpriteRenderer spriteRenderer = gateIndicator.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = spriteObject;

        // Set correct position and rotation (lossyScale so is not skewed)
        initialPosition = Vector3.up * verticalOffset;
        gateIndicator.transform.localPosition = initialPosition;
        gateIndicator.transform.localScale = Vector3.Scale(
            spriteScale,
            new Vector3(
                1f / transform.lossyScale.x,
                1f / transform.lossyScale.y,
                1f / transform.lossyScale.z
            )
        );
    }

    /// <summary>
    /// Handles the sprite floating and lookAt function.
    /// </summary>
    void FloatAndLookAtMethod()
    {
        if (gateIndicator == null || lookAtObject == null) return;

        // Calculate floating offset (Time.time)
        Vector3 floatOffset = Vector3.up * (floatAmplitude * Mathf.Sin(Time.time * floatSpeed));

        // Update position according to floating offset
        gateIndicator.transform.localPosition = initialPosition + floatOffset;

        // Calculate lookAt for sprite to face the lookAtObject (with a custom offset)
        Vector3 directionToCamera = lookAtObject.position - gateIndicator.transform.position;
        gateIndicator.transform.rotation = Quaternion.LookRotation(directionToCamera);
        gateIndicator.transform.rotation *= Quaternion.Euler(90f + lookAtOffset.x, lookAtOffset.y, lookAtOffset.z);
    }
}