using UnityEngine;

public class AnimateTransform : MonoBehaviour
{
    public Vector3 startPosition = Vector3.zero; // Starting position of the motion
    public Vector3 endPosition = new Vector3(1, 1, 1); // End position of the motion
    public Quaternion startRotation = Quaternion.identity; // Starting rotation
    public Quaternion endRotation = Quaternion.Euler(0, 90, 0); // End rotation
    public float duration = 1f; // Duration of the motion
    public AnimationCurve motionCurve = AnimationCurve.Linear(0, 0, 1, 1); // Animation curve to define motion
    public bool useRelativePosition = false; // Checkbox to switch between world and relative position

    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;

    void Start()
    {
        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;
    }

    void Update()
    {
        // Calculate looped elapsed time
        float loopedTime = (Time.time % duration) / duration;

        // Evaluate the curve based on the looped time
        float t = motionCurve.Evaluate(loopedTime);

        // Determine the target local position
        Vector3 targetLocalPosition = Vector3.Lerp(startPosition, endPosition, t);

        // Determine the target local rotation
        Quaternion targetLocalRotation = Quaternion.Lerp(startRotation, endRotation, t);

        // Apply either relative or world position and rotation based on the parent
        if (useRelativePosition)
        {
            transform.localPosition = initialLocalPosition + targetLocalPosition;
            transform.localRotation = initialLocalRotation * targetLocalRotation;
        }
        else
        {
            transform.localPosition = targetLocalPosition;
            transform.localRotation = targetLocalRotation;
        }
    }
}