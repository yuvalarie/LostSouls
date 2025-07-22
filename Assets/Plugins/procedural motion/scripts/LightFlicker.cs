using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightFlicker : MonoBehaviour
{
    [Header("Flicker Settings")]
    [Tooltip("Base intensity of the light.")]
    public float baseIntensity = 1f;

    [Tooltip("Amplitude of intensity flicker.")]
    public float intensityAmplitude = 0.5f;

    [Tooltip("Frequency of intensity flicker.")]
    public float intensityFrequency = 1f;

    [Tooltip("Base range of the light.")]
    public float baseRange = 10f;

    [Tooltip("Amplitude of range flicker.")]
    public float rangeAmplitude = 2f;

    [Tooltip("Frequency of range flicker.")]
    public float rangeFrequency = 0.5f;

    [Header("Noise Settings")]
    [Tooltip("Use noise instead of sine waves for flickering.")]
    public bool useNoise = false;

    private Light flickerLight;
    private float noiseOffset;

    void Start()
    {
        // Get the Light component on this GameObject
        flickerLight = GetComponent<Light>();

        // Randomize noise offset for independent flicker behavior
        noiseOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        if (flickerLight == null)
            return;

        float intensityOffset;
        float rangeOffset;

        if (useNoise)
        {
            // Generate smooth noise-based flicker
            intensityOffset = Mathf.PerlinNoise(Time.time * intensityFrequency, noiseOffset) * 2f - 1f; // [-1, 1]
            rangeOffset = Mathf.PerlinNoise(Time.time * rangeFrequency, noiseOffset + 1f) * 2f - 1f; // [-1, 1]
        }
        else
        {
            // Generate sine wave flicker
            intensityOffset = Mathf.Sin(Time.time * Mathf.PI * 2f * intensityFrequency);
            rangeOffset = Mathf.Cos(Time.time * Mathf.PI * 2f * rangeFrequency);
        }

        // Apply amplitude and base values
        flickerLight.intensity = baseIntensity + intensityOffset * intensityAmplitude;
        flickerLight.range = baseRange + rangeOffset * rangeAmplitude;
    }
}