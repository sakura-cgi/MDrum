using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class KickAudioHit : MonoBehaviour
{
    [Header("Sound Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip kickClip;

    [Header("Visual Feedback")]
    [SerializeField] private Transform visualModel;
    [SerializeField] private float scaleMultiplier = 1.08f;
    [SerializeField] private float scaleDuration = 0.12f;

    [Header("Hammer Rotation")]
    [SerializeField] private Transform hammer;
    [SerializeField] private Vector3 rotationAxis = Vector3.right;
    [SerializeField] private float rotationAngle = 35f;

    [Header("Input Settings")]
    [SerializeField] private bool enableKeyboardInput = true;

    [Header("Kick State")]
    public bool kickBool { get; private set; }

    private Vector3 originalScale;
    private Quaternion originalHammerRotation;

    private Coroutine feedbackCoroutine;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (visualModel != null)
            originalScale = visualModel.localScale;

        if (hammer != null)
            originalHammerRotation = hammer.localRotation;
    }

    private void Update()
    {
        if (!enableKeyboardInput || Keyboard.current == null)
            return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
            SetKickBool(true);

        if (Keyboard.current.spaceKey.wasReleasedThisFrame)
            SetKickBool(false);
    }

    // キーボード・Viconなど、外部からキック状態を変更する
    public void SetKickBool(bool value)
    {
        if (kickBool == value)
            return;

        kickBool = value;

        // Trueになった瞬間だけキックを発生させる
        if (kickBool)
            PlayKick();
    }

    private void PlayKick()
    {
        if (audioSource != null && kickClip != null)
            audioSource.PlayOneShot(kickClip);

        PlayVisualFeedback();
    }

    private void PlayVisualFeedback()
    {
        if (visualModel == null && hammer == null)
            return;

        // 連続入力時は前のアニメーションを再スタート
        if (feedbackCoroutine != null)
            StopCoroutine(feedbackCoroutine);

        if (visualModel != null)
            visualModel.localScale = originalScale;

        if (hammer != null)
            hammer.localRotation = originalHammerRotation;

        feedbackCoroutine = StartCoroutine(VisualFeedbackPulse());
    }

    private IEnumerator VisualFeedbackPulse()
    {
        float halfDuration = Mathf.Max(scaleDuration * 0.5f, 0.001f);

        Vector3 targetScale = originalScale * scaleMultiplier;

        Quaternion targetHammerRotation = originalHammerRotation;

        if (hammer != null && rotationAxis.sqrMagnitude > 0f)
        {
            targetHammerRotation =
                originalHammerRotation *
                Quaternion.AngleAxis(
                    rotationAngle,
                    rotationAxis.normalized
                );
        }

        // 拡大・ハンマー回転
        float elapsed = 0f;

        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / halfDuration);

            if (visualModel != null)
            {
                visualModel.localScale = Vector3.Lerp(
                    originalScale,
                    targetScale,
                    t
                );
            }

            if (hammer != null)
            {
                hammer.localRotation = Quaternion.Slerp(
                    originalHammerRotation,
                    targetHammerRotation,
                    t
                );
            }

            yield return null;
        }

        // 元の大きさ・角度に戻す
        elapsed = 0f;

        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / halfDuration);

            if (visualModel != null)
            {
                visualModel.localScale = Vector3.Lerp(
                    targetScale,
                    originalScale,
                    t
                );
            }

            if (hammer != null)
            {
                hammer.localRotation = Quaternion.Slerp(
                    targetHammerRotation,
                    originalHammerRotation,
                    t
                );
            }

            yield return null;
        }

        if (visualModel != null)
            visualModel.localScale = originalScale;

        if (hammer != null)
            hammer.localRotation = originalHammerRotation;

        feedbackCoroutine = null;
    }
}
