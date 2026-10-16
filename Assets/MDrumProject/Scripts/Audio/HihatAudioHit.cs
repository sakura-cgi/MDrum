using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HihatAudioHit : MonoBehaviour
{
    [System.Serializable]
    public class HitSoundSet
    {
        public AudioClip small;
        public AudioClip medium;
        public AudioClip large;
    }

    [Header("Hit Zone")]
    [SerializeField] private Collider innerZone;

    [Header("Pedal")]
    [SerializeField] private bool pedalClosed = false;

    [Header("Pedal Input")]
    [SerializeField] private bool enableKeyboardInput = true;

    [Header("Open Sounds")]
    [SerializeField] private HitSoundSet openSounds;

    [Header("Closed Sounds")]
    [SerializeField] private HitSoundSet closedSounds;

    [Header("Sound Settings")]
    [SerializeField] private AudioSource audioSource;

    [Header("Velocity Thresholds")]
    [SerializeField] private float hitThreshold = 1.0f;
    [SerializeField] private float mediumThreshold = 3.0f;
    [SerializeField] private float largeThreshold = 6.0f;

    [Header("Real-time Volume")]
    [SerializeField, Range(0f, 1f)]
    private float minVolume = 0.5f;

    [SerializeField, Range(0f, 1f)]
    private float maxVolume = 1.0f;

    [SerializeField] private float volumeMinVelocity = 1.0f;
    [SerializeField] private float volumeMaxVelocity = 8.0f;

    [Header("Visual Feedback")]
    [SerializeField] private Transform visualModel;
    [SerializeField] private float scaleMultiplier = 1.08f;
    [SerializeField] private float scaleDuration = 0.12f;


    [Header("Cymbal Movement")]
    [SerializeField] private Transform upperCymbal; // Up
    [SerializeField] private Transform lowerCymbal; // Under

    [SerializeField] private float closedGap = 0.02f;
    [SerializeField] private float closeSpeed = 8f;

    private Vector3 upperOpenPosition;
    private Vector3 lowerOpenPosition;
    private Vector3 upperClosedPosition;
    private Vector3 lowerClosedPosition;

    private Vector3 originalScale;
    private Coroutine scaleCoroutine;

    private readonly Dictionary<Collider, bool> activeSticks
        = new Dictionary<Collider, bool>();

    private void Awake()
    {
        if (visualModel != null)
            originalScale = visualModel.localScale;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (upperCymbal != null && lowerCymbal != null)
        {
            upperOpenPosition = upperCymbal.localPosition;
            lowerOpenPosition = lowerCymbal.localPosition;

            upperClosedPosition = upperOpenPosition;
            lowerClosedPosition = lowerOpenPosition;

            upperClosedPosition.y -= 0.0003f;

            float gap = Mathf.Abs(
                upperOpenPosition.y - lowerOpenPosition.y
            );

            float closeDistance = Mathf.Max(0f, gap - closedGap);

            upperClosedPosition.y -= closeDistance;
        }
    }

    private void Update()
    {
        if (enableKeyboardInput)
        {
            bool closed =
                Keyboard.current != null &&
                Keyboard.current.hKey.isPressed;

            SetPedalClosed(closed);
        }

        if (upperCymbal == null || lowerCymbal == null)
            return;

        Vector3 targetUpper = pedalClosed
            ? upperClosedPosition
            : upperOpenPosition;

        Vector3 targetLower = pedalClosed
            ? lowerClosedPosition
            : lowerOpenPosition;

        upperCymbal.localPosition = Vector3.Lerp(
            upperCymbal.localPosition,
            targetUpper,
            Time.deltaTime * closeSpeed
        );

        lowerCymbal.localPosition = Vector3.Lerp(
            lowerCymbal.localPosition,
            targetLower,
            Time.deltaTime * closeSpeed
        );
    }


    public void SetPedalClosed(bool closed)
    {
        pedalClosed = closed;
    }

    public bool IsPedalClosed()
    {
        return pedalClosed;
    }

    public void ZoneEntered(
        Collider zone,
        Collider stick,
        float velocity)
    {
        if (zone != innerZone || stick == null)
            return;

        // 同じスティックが接触し続けている間は再発音しない
        if (activeSticks.ContainsKey(stick))
            return;

        activeSticks.Add(stick, true);

        if (velocity < hitThreshold)
            return;

        PlayHit(velocity);
    }

    public void ZoneExited(Collider zone, Collider stick)
    {
        if (zone != innerZone || stick == null)
            return;

        activeSticks.Remove(stick);
    }

    private void PlayHit(float velocity)
    {
        HitSoundSet sounds = pedalClosed
            ? closedSounds
            : openSounds;

        if (audioSource == null || sounds == null)
            return;

        AudioClip clip;

        if (velocity >= largeThreshold)
            clip = sounds.large;
        else if (velocity >= mediumThreshold)
            clip = sounds.medium;
        else
            clip = sounds.small;

        if (clip == null)
            return;

        float normalizedVelocity = Mathf.InverseLerp(
            volumeMinVelocity,
            volumeMaxVelocity,
            velocity
        );

        float volume = Mathf.Lerp(
            minVolume,
            maxVolume,
            normalizedVelocity
        );

        audioSource.PlayOneShot(clip, volume);
        PlayVisualFeedback();
    }

    private void PlayVisualFeedback()
    {
        if (visualModel == null)
            return;

        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        scaleCoroutine = StartCoroutine(ScalePulse());
    }

    private IEnumerator ScalePulse()
    {
        float halfDuration = Mathf.Max(
            scaleDuration * 0.5f, 0.001f
        );

        Vector3 targetScale = originalScale * scaleMultiplier;
        float elapsed = 0f;

        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;

            visualModel.localScale = Vector3.Lerp(
                originalScale,
                targetScale,
                elapsed / halfDuration
            );

            yield return null;
        }

        elapsed = 0f;

        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;

            visualModel.localScale = Vector3.Lerp(
                targetScale,
                originalScale,
                elapsed / halfDuration
            );

            yield return null;
        }

        visualModel.localScale = originalScale;
        scaleCoroutine = null;
    }
}
