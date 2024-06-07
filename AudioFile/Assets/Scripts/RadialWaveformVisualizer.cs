using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialWaveformVisualizer : MonoBehaviour
{
    private AudioSource currentTrack;
    public LineRenderer lineRenderer;
    public int resolution = 1024; // Number of samples to analyze
    public float radius = 5f; // Radius of the circle
    public float renderingFrequency = 0.1f; // Frequency of the rendering execution (in seconds)
    public float colorChangeFrequency = 3f; // Frequency of the color change lerping (in seconds)
    private Coroutine renderingCoroutine;
    private Coroutine colorChangeCoroutine;
    private float radiusOffset;

    private float[] samples;

    public void Initialize(AudioSource track, MediaPlayerManager mediaPlayerManager, float scale, float initialRadiusOffset)
    {
        currentTrack = track;
        samples = new float[resolution];
        lineRenderer.positionCount = resolution;

        radius = scale * 5f;
        radiusOffset = initialRadiusOffset;

        if (currentTrack != null)
        {
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

    public void UpdateRadius(float newRadius)
    {
        radius = newRadius;
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
            float offsetRadius = radius + Mathf.Sin(Time.time + radiusOffset) * 3.0f; // Use the offset for individual wave effect

            for (int i = 0; i < resolution; i++)
            {
                float angle = Mathf.PI * 2f * i / resolution;
                float x = Mathf.Cos(angle) * (offsetRadius + samples[i] + 1);
                float y = Mathf.Sin(angle) * (offsetRadius + samples[i] + 1);
                lineRenderer.SetPosition(i, new Vector3(x, y, 0f));
            }

            yield return new WaitForSeconds(renderingFrequency);
        }
    }

    IEnumerator ExecuteColorChange()
    {
        while (true)
        {
            float hue = Mathf.Repeat(Time.time / colorChangeFrequency, 1f);
            Color newColor = Color.HSVToRGB(hue, 1f, 1f);

            lineRenderer.material.color = newColor;

            yield return null;
        }
    }

    void Update()
    {
        if (currentTrack != null)
        {
            currentTrack.GetOutputData(samples, 0);
        }
    }

    private void OnDestroy()
    {
        MediaPlayerManager mediaPlayerManager = FindObjectOfType<MediaPlayerManager>();
        if (mediaPlayerManager != null)
        {
            mediaPlayerManager.OnPlayStateChanged -= HandlePlayStateChanged;
            mediaPlayerManager.OnTrackChanged -= HandleTrackChanged;
        }
    }
}
