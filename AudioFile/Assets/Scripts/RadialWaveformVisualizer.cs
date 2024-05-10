using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialWaveformVisualizer : MonoBehaviour
{
    public AudioSource audioSource;
    public LineRenderer lineRenderer;
    public int resolution = 1024; // Number of samples to analyze
    public float radius = 5f; // Radius of the circle
    public Color startColor = Color.red; // Initial color of the line
    public Color endColor = Color.blue; // Final color of the line
    public float renderingFrequency = 0.5f; // Frequency of the rendering execution (in seconds)
    public float gradientFrequency = 3f; // Frequency of the color lerping (in seconds)

    private float[] samples;

    void Start()
    {
        samples = new float[resolution];
        lineRenderer.positionCount = resolution;

        lineRenderer.startColor = startColor;
        lineRenderer.endColor = endColor;

        // Start coroutines for executing the for loop and color lerping at specified frequencies
        StartCoroutine(ExecuteRendering());
        StartCoroutine(ExecuteColorLerp());
    }
    IEnumerator ExecuteRendering()
    {
        while (true)
        {
                // Execute the for loop
                // Example: Change the number of positions in the LineRenderer
                //lineRenderer.positionCount = Random.Range(2, 10); // Randomize the number of positions between 2 and 10

            for (int i = 0; i < resolution; i++)
            {
                float angle = Mathf.PI * 2f * i / resolution;
                float x = Mathf.Cos(angle) * (radius + samples[i]);
                float y = Mathf.Sin(angle) * (radius + samples[i]);
                lineRenderer.SetPosition(i, new Vector3(x, y, 0f));
            }

            yield return new WaitForSeconds(renderingFrequency); // Wait for renderingFrequency seconds before executing the loop again
        }
    }

    IEnumerator ExecuteColorLerp()
    {
        while (true)
        {
            // Execute the color lerping block
            float lerpFactor = Mathf.PingPong(Time.time, 1f); // Use PingPong to smoothly interpolate between startColor and endColor
            lineRenderer.startColor = Color.Lerp(startColor, endColor, lerpFactor);
            lineRenderer.endColor = Color.Lerp(startColor, endColor, lerpFactor);

            yield return new WaitForSeconds(gradientFrequency); // Wait for gradientFrequency seconds before executing the lerping again
        }
    }
        /*
        // Initialize the LineRenderer along the circumference of the circle
        for (int i = 0; i < resolution; i++)
        {
            float angle = Mathf.PI * 2f * i / resolution;
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;
            lineRenderer.SetPosition(i, new Vector3(x, y, 0f));
        }
        */
    void Update()
    {
        // Get audio samples from the AudioSource
        audioSource.GetOutputData(samples, 0);

        /*
        // Update the LineRenderer with the audio waveform mapped to the circle
        for (int i = 0; i < resolution; i++)
        {
            float angle = Mathf.PI * 2f * i / resolution;
            float x = Mathf.Cos(angle) * (radius + samples[i]);
            float y = Mathf.Sin(angle) * (radius + samples[i]);
            lineRenderer.SetPosition(i, new Vector3(x, y, 0f));
        }
        */
    }
}
