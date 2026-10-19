using UnityEngine;
using UnityEngine.InputSystem;

public class CalibrationManager : MonoBehaviour
{
    [Header("Pedal Markers")]
    [SerializeField] private Transform leftPedalMarker;  // HiHat
    [SerializeField] private Transform rightPedalMarker; // Kick

    [Header("Stick Markers")]
    [SerializeField] private Transform leftStickMarker;
    [SerializeField] private Transform rightStickMarker;

    [Header("Drum Set")]
    [SerializeField] private Transform drumSetRoot;

    [Header("Calibration Settings")]
    [SerializeField, Min(0.001f)]
    private float minimumPedalDistance = 0.05f;

    [SerializeField, Min(0.001f)]
    private float minimumSideDistance = 0.05f;

    [SerializeField]
    private bool calibrateOnStart = false;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    public bool IsCalibrated { get; private set; }

    public float HiHatRestY { get; private set; }
    public float KickRestY { get; private set; }

    public Vector3 CalibratedPosition { get; private set; }
    public Quaternion CalibratedRotation { get; private set; }

    private void Start()
    {
        if (calibrateOnStart)
        {
            TryCalibrate();
        }
    }

    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.cKey.wasPressedThisFrame)
        {
            TryCalibrate();
        }
    }

    [ContextMenu("Run Calibration")]
    public void TryCalibrate()
    {
        if (!ValidateReferences())
        {
            CalibrationFailed("Inspectorの参照が不足しています。");
            return;
        }

        // ペダルの左右方向を求める。
        // Y方向の高さの違いは無視して、水平面上で計算する。
        Vector3 leftPos = leftPedalMarker.position;
        Vector3 rightPos = rightPedalMarker.position;

        Vector3 pedalAxis = rightPos - leftPos;
        pedalAxis.y = 0f;

        if (pedalAxis.magnitude < minimumPedalDistance)
        {
            CalibrationFailed(
                "KickとHiHatの距離が近すぎます。マーカー位置を確認してください。");
            return;
        }

        Vector3 rightDirection = pedalAxis.normalized;

        // ペダルの直線に垂直な、前後2方向の候補を求める。
        Vector3 forwardCandidateA =
            Vector3.Cross(rightDirection, Vector3.up).normalized;

        Vector3 forwardCandidateB = -forwardCandidateA;

        // 左右のスティック位置から演奏者側を判定する。
        Vector3 stickCenter =
            (leftStickMarker.position + rightStickMarker.position) * 0.5f;

        Vector3 pedalCenter = (leftPos + rightPos) * 0.5f;

        Vector3 toStick = stickCenter - pedalCenter;
        toStick.y = 0f;

        if (toStick.magnitude < minimumSideDistance)
        {
            CalibrationFailed(
                "スティックがペダル直線に近すぎるため、前後を判定できません。");
            return;
        }

        float sideA = Vector3.Dot(toStick, forwardCandidateA);
        float sideB = Vector3.Dot(toStick, forwardCandidateB);

        // スティック側を後方とする。
        // ドラムセットの前方向は、その反対側にする。
        Vector3 backDirection =
            sideA > sideB ? forwardCandidateA : forwardCandidateB;

        Vector3 forwardDirection = -backDirection;

        // ワールドのY軸を上方向として、回転を作成する。
        Quaternion targetRotation =
            Quaternion.LookRotation(forwardDirection, Vector3.up);

        // Kickマーカーの位置にDrumSetRootの原点を合わせる。
        Vector3 targetPosition = rightPedalMarker.position;

        // ペダル初期位置を保存する。
        HiHatRestY = leftPedalMarker.position.y;
        KickRestY = rightPedalMarker.position.y;

        // キャリブレーション結果を反映する。
        drumSetRoot.SetPositionAndRotation(
            targetPosition,
            targetRotation);

        CalibratedPosition = targetPosition;
        CalibratedRotation = targetRotation;

        IsCalibrated = true;

        if (showDebugLogs)
        {
            Debug.Log(
                $"[CalibrationManager] キャリブレーション成功\n" +
                $"DrumSetRoot Position: {CalibratedPosition}\n" +
                $"DrumSetRoot Rotation: {CalibratedRotation.eulerAngles}\n" +
                $"HiHat Rest Y: {HiHatRestY:F4}\n" +
                $"Kick Rest Y: {KickRestY:F4}",
                this);
        }
    }

    private bool ValidateReferences()
    {
        return leftPedalMarker != null
            && rightPedalMarker != null
            && leftStickMarker != null
            && rightStickMarker != null
            && drumSetRoot != null;
    }

    private void CalibrationFailed(string reason)
    {
        IsCalibrated = false;

        Debug.LogWarning(
            $"[CalibrationManager] キャリブレーション失敗: {reason}",
            this);
    }
}
