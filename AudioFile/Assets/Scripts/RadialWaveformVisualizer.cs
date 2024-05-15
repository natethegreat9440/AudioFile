using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialWaveformVisualizer : MonoBehaviour
{
    private AudioSource currentTrack;
    public LineRenderer lineRenderer;
    public int resolution = 1024; // Number of samples to analyze
    public float radius = 5f; // Radius of the circle
    public Color startColor = Color.red; // Initial color of the line
    public Color endColor = Color.blue; // Final color of the line
    public float renderingFrequency = 0.1f; // Frequency of the rendering execution (in seconds)
    public float colorChangeFrequency = 3f; // Frequency of the color change lerping (in seconds)
    private Coroutine renderingCoroutine;
    private Coroutine colorChangeCoroutine;

    private float[] samples;

    public void Start()
    {
        samples = new float[resolution];
        lineRenderer.positionCount = resolution;

        lineRenderer.startColor = startColor;
        lineRenderer.endColor = endColor;

        MediaPlayerManager mediaPlayerManager = FindObjectOfType<MediaPlayerManager>();

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

            // Subscribe to events
            if (mediaPlayerManager != null)
            {
                mediaPlayerManager.OnPlayStateChanged += HandlePlayStateChanged;
                mediaPlayerManager.OnTrackChanged += HandleTrackChanged;
            }

            // Start coroutines for executing the for loop and color lerping at specified frequencies.
            //Coroutines need to be assigned to a reference otherwise other methods will ignore them
            renderingCoroutine = StartCoroutine(ExecuteRendering());
            colorChangeCoroutine = StartCoroutine(ExecuteColorChange());
        }
        else
        {
            Debug.LogWarning("There is no current track to play");
        }
    }
    private void HandlePlayStateChanged(bool isPlaying)
    {
        if (isPlaying)
        {
            if (renderingCoroutine == null)
            {
                renderingCoroutine = StartCoroutine(ExecuteRendering());
            }
            if (colorChangeCoroutine == null)
            {
                colorChangeCoroutine = StartCoroutine(ExecuteColorChange());
            }
            Debug.Log("Visualizer started");
        }
        else
        {
            if (renderingCoroutine != null)
            {
                StopCoroutine(renderingCoroutine);
                renderingCoroutine = null;
            }
            if (colorChangeCoroutine != null)
            {
                StopCoroutine(colorChangeCoroutine);
                colorChangeCoroutine = null;
            }
            Debug.Log("Visualizer stopped");
        }
    }

    private void HandleTrackChanged(string trackName)
    {
        if (renderingCoroutine != null)
        {
            StopCoroutine(renderingCoroutine);
            renderingCoroutine = null;
        }
        if (colorChangeCoroutine != null)
        {
            StopCoroutine(colorChangeCoroutine);
            colorChangeCoroutine = null;
        }
        Debug.Log("Visualizer stopped");

        renderingCoroutine = StartCoroutine(ExecuteRendering());
        colorChangeCoroutine = StartCoroutine(ExecuteColorChange());
        Debug.Log("Visualizer started");
    }

    IEnumerator ExecuteRendering()
    {
        while (true)
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
        while (true)
        {
            // Execute the color lerping block
            float lerpFactor = Mathf.PingPong(Time.time, 1f); // Use PingPong to smoothly interpolate between startColor and endColor
            lineRenderer.material.color = Color.Lerp(startColor, endColor, lerpFactor);

            yield return new WaitForSeconds(colorChangeFrequency); // Wait for colorChangeFrequency seconds before executing the lerping again
        }
    }
    void Update()
    {
        // Get audio samples from the AudioSource
        currentTrack.GetOutputData(samples, 0);
    }
    private void OnDestroy()
    {
        //Unsubscribes HandlePlayStateChanged and HandleTrackChanged methods from the OnPlayStateChanged and OnTrackChanged events respectively
        MediaPlayerManager mediaPlayerManager = FindObjectOfType<MediaPlayerManager>();
        mediaPlayerManager.OnPlayStateChanged -= HandlePlayStateChanged;
        mediaPlayerManager.OnTrackChanged -= HandleTrackChanged;
    }
}
