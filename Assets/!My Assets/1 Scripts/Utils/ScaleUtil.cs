using UnityEngine;
public class ScaleUtil : MonoBehaviour
{
    [Header("Scale Settings")]
    [Tooltip("The Value Used To Calculate How Much Bigger/Smaller The Target Scale Should Be (e.g. +3 or -3)")]
    [SerializeField] float scaleModifier = 1f;
    [Tooltip("How Fast It Lerps")]
    [SerializeField] float lerpSpeed = 2f;

    [Header("Lerp Direction")]
    [Tooltip("If enabled, the scaling will loop between min and max.")]
    [SerializeField] bool loop = true;
    [Tooltip("If enabled, scaling starts at Start()")]
    [SerializeField] bool autoStart = true;

    Vector3 minScale;   // Set at Start(), the object's original scale
    Vector3 maxScale;   // Set at Start(), the object's original scale + scaleModifier
    Vector3 targetScale;// Set at Runtime, is the target lerping value, which inverts when finished
    bool scalingUp = true;

    /// <summary>
    /// Initialises min scale and target scale values, and calculates max scale based on scaleModifier
    /// </summary>
    void Start()
    {
        minScale = transform.localScale;
        maxScale = new Vector3(
            minScale.x * (1 + scaleModifier),
            minScale.y * (1 + scaleModifier),
            minScale.z * (1 + scaleModifier)
        );

        if (autoStart)
            targetScale = maxScale;
    }

    /// <summary>
    /// Lerps object scale between min and max scales.
    /// Looping is done by inverting the values.
    /// </summary>
    void Update()
    {
        if (!autoStart) return;

        // lerp toward target scale
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * lerpSpeed);

        // Inert Target Scale Value
        if (Vector3.Distance(transform.localScale, targetScale) < 0.01f)
        {
            if (loop)
            {
                scalingUp = !scalingUp;
                targetScale = scalingUp ? maxScale : minScale;
            }
        }
    }

    /// <summary>
    /// Starts scaling, sets initial target scale
    /// </summary>
    public void StartScaling()
    {
        autoStart = true;
        targetScale = scalingUp ? maxScale : minScale;
    }

    /// <summary>
    /// Stops scaling
    /// </summary>
    public void StopScaling()
    {
        autoStart = false;
    }

    /// <summary>
    /// Resets scale to minimum and sets targetScale to maximum
    /// </summary>
    public void ResetScale()
    {
        transform.localScale = minScale;
        scalingUp = true;
        targetScale = maxScale;
    }
}