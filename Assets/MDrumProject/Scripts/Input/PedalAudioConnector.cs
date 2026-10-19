using UnityEngine;

public class PedalAudioConnector : MonoBehaviour
{
    [Header("Kick Pedal")]
    [SerializeField] private PedalPositionDetector kickPedal;
    [SerializeField] private KickAudioHit kickAudio;

    [Header("Hi-Hat Pedal")]
    [SerializeField] private PedalPositionDetector hiHatPedal;
    [SerializeField] private HihatAudioHit hiHatAudio;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private void OnEnable()
    {
        if (kickPedal != null)
            kickPedal.OnPressedChanged += OnKickPressedChanged;

        if (hiHatPedal != null)
            hiHatPedal.OnPressedChanged += OnHiHatPressedChanged;
    }

    private void Start()
    {
        // 接続時点ですでに踏まれている場合も状態を同期する
        if (kickPedal != null)
            OnKickPressedChanged(kickPedal.IsPressed);

        if (hiHatPedal != null)
            OnHiHatPressedChanged(hiHatPedal.IsPressed);
    }

    private void OnDisable()
    {
        if (kickPedal != null)
            kickPedal.OnPressedChanged -= OnKickPressedChanged;

        if (hiHatPedal != null)
            hiHatPedal.OnPressedChanged -= OnHiHatPressedChanged;
    }

    private void OnKickPressedChanged(bool pressed)
    {
        if (kickAudio == null)
            return;

        kickAudio.SetKickBool(pressed);

        if (showDebugLogs)
            Debug.Log($"[PedalAudioConnector] Kick: {(pressed ? "ON" : "OFF")}", this);
    }

    private void OnHiHatPressedChanged(bool pressed)
    {
        if (hiHatAudio == null)
            return;

        hiHatAudio.SetPedalClosed(pressed);

        if (showDebugLogs)
            Debug.Log($"[PedalAudioConnector] HiHat: {(pressed ? "CLOSED" : "OPEN")}", this);
    }
}
