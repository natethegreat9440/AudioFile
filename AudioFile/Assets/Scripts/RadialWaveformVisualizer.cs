using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialWaveformVisualizer : MonoBehaviour
{
    private AudioSource currentTrack;
    public LineRenderer lineRenderer;
    public int resolution = 1024; // Number of samples to analyze
    public float radius = 5f; // Radius of the circle
    //public Color startColor = Color.red; // Initial color of the line
    //public Color endColor = Color.blue; // Final color of the line
    public float renderingFrequency = 0.1f; // Frequency of the rendering execution (in seconds)
    public float colorChangeFrequency = 3f; // Frequency of the color change lerping (in seconds)
    private Coroutine renderingCoroutine;
    private Coroutine colorChangeCoroutine;

    private float[] samples;

    public void Initialize(AudioSource track, MediaPlayerManager mediaPlayerManager, float scale)
    {
        currentTrack = track;
        samples = new float[resolution];
        lineRenderer.positionCount = resolution;

        // Adjust radius based on the scale
        radius *= scale;

        if (currentTrack != null)
        {
            for (int i = 0; i < resolution; i++)
            {
                float angle = Mathf.PI * 2f * i / resolution;
                float x = Mathf.Cos(angle) * radius;
                float y = Mathf.Sin(angle) * radius;
                lineRenderer.SetPosition(i, new Vector3(x, y, 0f));
            }

            mediaPlayerManager.OnPlayStateChanged += HandlePlayStateChanged;
            mediaPlayerManager.OnTrackChanged += HandleTrackChanged;

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
            // Calculate the hue value based on time to cycle through colors
            float hue = Mathf.Repeat(Time.time / colorChangeFrequency, 1f);
            Color newColor = Color.HSVToRGB(hue, 1f, 1f); // Full saturation and value for bright colors

            // Apply the new color to the LineRenderer's material
            lineRenderer.material.color = newColor;

            yield return null; // Wait for the next frame before executing the color change again
        }
    }

    void Update()
    {
        // Get audio samples from the AudioSource
        if (currentTrack != null)
        {
            currentTrack.GetOutputData(samples, 0);
        }
    }
    private void OnDestroy()
    {
        //Unsubscribes HandlePlayStateChanged and HandleTrackChanged methods from the OnPlayStateChanged and OnTrackChanged events respectively
        MediaPlayerManager mediaPlayerManager = FindObjectOfType<MediaPlayerManager>();
        if (mediaPlayerManager != null)
        {
            mediaPlayerManager.OnPlayStateChanged -= HandlePlayStateChanged;
            mediaPlayerManager.OnTrackChanged -= HandleTrackChanged;
        }
    }
}
