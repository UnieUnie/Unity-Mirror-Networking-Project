using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spins a list of objects around its center local axis.
/// Customizable Speed.
/// </summary>
public class FanSpin : MonoBehaviour
{
    [Header("Spin Settings")]
    [Tooltip("Spin floatSpeed: Degrees per second")]
    [SerializeField] float spinSpeed = 100f;

    [Tooltip("List of objects to spin.")]
    [SerializeField] List<Transform> bladesToSpin;

    Dictionary<Transform, Vector3> bladeCenter = new Dictionary<Transform, Vector3>();

    void Awake()
    {
        // Precompute and cache world center and offset for each blade
        foreach (Transform blade in bladesToSpin)
        {
            if (blade == null) continue;

            Renderer renderer = blade.GetComponent<Renderer>();
            if (renderer == null) continue;

            // Calculate and store the local offset
            Vector3 worldCenter = renderer.bounds.center;
            Vector3 localOffset = blade.position - worldCenter;
            bladeCenter[blade] = localOffset;

            // Reposition the blade to remove displacement
            blade.position = worldCenter + localOffset;
        }
    }

    private void LateUpdate()
    {
        // Rotate each blade around its local center
        foreach (Transform blade in bladesToSpin)
        {
            if (blade == null || !bladeCenter.ContainsKey(blade)) continue;

            // Perform rotation relative to local center
            blade.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.Self);
        }
    }
}
