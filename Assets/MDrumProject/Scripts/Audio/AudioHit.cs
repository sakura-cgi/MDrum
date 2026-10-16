using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioHit : MonoBehaviour
{
    [System.Serializable]
    public class HitSoundSet
    {
        public AudioClip small;
        public AudioClip medium;
        public AudioClip large;
    }

    [Header("Hit Zones")]
    [SerializeField] private Collider innerZone;
    [SerializeField] private Collider outerZone; // 未設定でもOK

    [Header("Sound Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private HitSoundSet innerSounds;
    [SerializeField] private HitSoundSet outerSounds; // 未設定でもOK

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

    [Header("Hit Priority")]
    [SerializeField] private float priorityWindow = 0.04f;

    private Vector3 originalScale;
    private Coroutine scaleCoroutine;

    private class PendingHit
    {
        public Collider stick;
        public Collider zone;
        public float velocity;
        public int priority;
    }

    // スティックごとに接触中の領域を記録
    private readonly Dictionary<Collider, HashSet<Collider>> activeZones
        = new Dictionary<Collider, HashSet<Collider>>();

    private readonly Dictionary<Collider, PendingHit> pendingHits
        = new Dictionary<Collider, PendingHit>();

    // 音を再生済みのスティック
    private readonly HashSet<Collider> playedSticks
        = new HashSet<Collider>();

    private void Awake()
    {
        if (visualModel != null)
            originalScale = visualModel.localScale;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void ZoneEntered(
        Collider zone,
        Collider stick,
        float velocity)
    {
        // 登録されている領域だけを対象にする
        if (zone != innerZone && zone != outerZone)
            return;

        // 接触状態は速度に関係なく記録する
        if (!activeZones.TryGetValue(stick, out var zones))
        {
            zones = new HashSet<Collider>();
            activeZones.Add(stick, zones);
        }

        zones.Add(zone);

        // 速度不足、または発音済みなら発音しない
        if (velocity < hitThreshold || playedSticks.Contains(stick))
            return;

        int priority = zone == innerZone ? 2 : 1;

        if (pendingHits.TryGetValue(stick, out var pending))
        {
            // Innerを優先
            if (priority > pending.priority)
            {
                pending.zone = zone;
                pending.velocity = velocity;
                pending.priority = priority;
            }

            return;
        }

        PendingHit newHit = new PendingHit
        {
            stick = stick,
            zone = zone,
            velocity = velocity,
            priority = priority
        };

        pendingHits.Add(stick, newHit);
        StartCoroutine(ResolveHit(stick, newHit));
    }

    public void ZoneExited(Collider zone, Collider stick)
    {
        if (!activeZones.TryGetValue(stick, out var zones))
            return;

        // 退出した領域だけを解除
        zones.Remove(zone);

        // まだ別の領域に接触しているなら解除しない
        if (zones.Count > 0)
            return;

        // 全領域から退出したときだけ再発音可能にする
        activeZones.Remove(stick);
        playedSticks.Remove(stick);

        // 未確定の発音もキャンセル
        pendingHits.Remove(stick);
    }

    private IEnumerator ResolveHit(
        Collider stick,
        PendingHit hit)
    {
        yield return new WaitForSeconds(priorityWindow);

        // すでにキャンセルされた判定なら何もしない
        if (!pendingHits.TryGetValue(stick, out var current)
            || current != hit)
            yield break;

        pendingHits.Remove(stick);

        if (!activeZones.TryGetValue(stick, out var zones)
            || zones.Count == 0
            || playedSticks.Contains(stick))
            yield break;

        playedSticks.Add(stick);
        PlayHit(hit.zone, hit.velocity);
    }

    private void PlayHit(Collider zone, float velocity)
    {
        HitSoundSet sounds;

        if (zone == innerZone)
        {
            sounds = innerSounds;
        }
        else if (zone == outerZone)
        {
            sounds = outerSounds;
        }
        else
        {
            return;
        }

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
        float halfDuration = Mathf.Max(scaleDuration * 0.5f, 0.001f);
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
