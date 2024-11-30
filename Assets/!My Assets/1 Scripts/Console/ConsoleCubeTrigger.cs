using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to call ConsoleUIController.cs during collision.
/// That script isnt on the cube, so this is here to act as a easy delegate
/// </summary>
public class ConsoleCubeTrigger : MonoBehaviour
{
    [SerializeField] ConsoleUIController uiController;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Puzzle")) return;
        uiController.HandleTriggerEnter(other);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Puzzle")) return;
        uiController.HandleTriggerExit(other);
    }
}