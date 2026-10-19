using System;
using UnityEngine;

public class PedalPositionDetector : MonoBehaviour
{
    public enum PedalType
    {
        Kick,
        HiHat
    }

    [Header("Pedal Settings")]
    [SerializeField] private PedalType pedalType;

    [SerializeField] private Transform pedalMarker;

    [SerializeField] private CalibrationManager calibrationManager;

    [Header("Thresholds (meters)")]
    [Tooltip("この距離以上下がったらON")]
    [SerializeField, Min(0f)]
    private float pressThreshold = 0.02f;

    [Tooltip("この距離以下まで戻ったらOFF")]
    [SerializeField, Min(0f)]
    private float releaseThreshold = 0.005f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    public bool IsPressed { get; private set; }

    public float Displacement { get; private set; }

    public event Action<bool> OnPressedChanged;

    private bool wasCalibrated;
    private bool hasRestPosition;
    private float cachedRestY;

    private void Update()
    {
        if (pedalMarker == null || calibrationManager == null)
            return;

        if (!calibrationManager.IsCalibrated)
        {
            wasCalibrated = false;
            hasRestPosition = false;
            SetPressed(false);
            return;
        }

        float restY = GetRestY();

        // 初回キャリブレーション、または基準位置が更新されたとき
        if (!wasCalibrated ||
            !hasRestPosition ||
            Mathf.Abs(restY - cachedRestY) > 0.0001f)
        {
            wasCalibrated = true;
            hasRestPosition = true;
            cachedRestY = restY;

            Displacement = 0f;
            SetPressed(false);

            if (showDebugLogs)
            {
                Debug.Log(
                    $"[{pedalType}] 基準Y座標を設定: {restY:F4}",
                    this);
            }

            return;
        }

        // 下方向への移動量を計算する
        Displacement = restY - pedalMarker.position.y;

        // ヒステリシスによるON/OFF判定
        if (!IsPressed && Displacement >= pressThreshold)
        {
            SetPressed(true);
        }
        else if (IsPressed && Displacement <= releaseThreshold)
        {
            SetPressed(false);
        }
    }

    private float GetRestY()
    {
        return pedalType == PedalType.Kick
            ? calibrationManager.KickRestY
            : calibrationManager.HiHatRestY;
    }

    private void SetPressed(bool pressed)
    {
        if (IsPressed == pressed)
            return;

        IsPressed = pressed;

        if (showDebugLogs)
        {
            Debug.Log(
                $"[{pedalType}] {(IsPressed ? "ON（踏み込み）" : "OFF（復帰）")} " +
                $"変位: {Displacement * 100f:F1} cm",
                this);
        }

        OnPressedChanged?.Invoke(IsPressed);
    }
}
