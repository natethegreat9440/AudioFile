using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialWaveformVisualizer : MonoBehaviour
{
    public GameObject mediaPlayerManager;
    private AudioSource currentTrack;
    public LineRenderer lineRenderer;
    public int resolution = 1024; // Number of samples to analyze
    public float radius = 5f; // Radius of the circle
    public Color startColor = Color.red; // Initial color of the line
    public Color endColor = Color.blue; // Final color of the line
    public float renderingFrequency = 0.1f; // Frequency of the rendering execution (in seconds)
    public float colorChangeFrequency = 3f; // Frequency of the color change lerping (in seconds)

    private bool isPlaying = true; // Flag to control coroutine execution

    private float[] samples;

    public void Start()
    {
        samples = new float[resolution];
        lineRenderer.positionCount = resolution;

        lineRenderer.startColor = startColor;
        lineRenderer.endColor = endColor;

        currentTrack = mediaPlayerManager.GetComponent<AudioSource>();

        if (currentTrack != null)
        {
            // Initialize the LineRenderer along the circumference of the circle
            for (int i = 0; i < resolution; i++)
            {
                float angle = Mathf.PI * 2f * i / resolution;
                float x = Mathf.Cos(angle) * radius;
                float y = Mathf.Sin(angle) * radius;
                lineRenderer.SetPosition(i, new Vector3(x, y, 0f));
            }

            // Start coroutines for executing the for loop and color lerping at specified frequencies
            StartCoroutine(ExecuteRendering());
            StartCoroutine(ExecuteColorChange());
        }
        else
        {
            Debug.LogWarning("There is no current track to play");
        }
    }

    // Method to pause coroutines
    public void PauseRadialWaveformVisualizer()
    {
        isPlaying = false;
        Debug.Log("Visualizer was Paused");
    }

    // Method to resume coroutines
    public void ResumeRadialWaveformVisualizer()
    {
        isPlaying = true;
        Start();
        Debug.Log("Visualizer was resumed");
    }

    IEnumerator ExecuteRendering()
    {
        while (isPlaying)
        {          
            // Execute the for loop
            for (int i = 0; i < resolution; i++)
            {
                float angle = Mathf.PI * 2f * i / resolution;
                float x = Mathf.Cos(angle) * (radius + samples[i] + 1);
                float y = Mathf.Sin(angle) * (radius + samples[i] + 1);
                lineRenderer.SetPosition(i, new Vector3(x, y, 0f));
            }

            yield return new WaitForSeconds(renderingFrequency); // Wait for renderingFrequency seconds before executing the loop again
        }
    }

    IEnumerator ExecuteColorChange()
    {
        while (isPlaying)
        {
            // Execute the color lerping block
            float lerpFactor = Mathf.PingPong(Time.time, 1f); // Use PingPong to smoothly interpolate between startColor and endColor
            lineRenderer.material.color = Color.Lerp(startColor, endColor, lerpFactor);
            lineRenderer.material.color = Color.Lerp(startColor, endColor, lerpFactor);

            yield return new WaitForSeconds(colorChangeFrequency); // Wait for colorChangeFrequency seconds before executing the lerping again
        }
    }

    void Update()
    {
        // Get audio samples from the AudioSource
        currentTrack.GetOutputData(samples, 0);
    }
}
