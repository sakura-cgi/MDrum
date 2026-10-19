
using UnityEngine;
using UnityEngine.InputSystem;

public class StickInputManager : MonoBehaviour
{
    public enum ControlMode
    {
        Mouse,
        Vicon
    }

    [Header("Stick Controllers - HoldPoints")]
    [SerializeField] private StickController leftStick;
    [SerializeField] private StickController rightStick;

    [Header("Marker Followers - Marker Objects")]
    [SerializeField] private MarkerFollower leftMarkerFollower;
    [SerializeField] private MarkerFollower rightMarkerFollower;

    [Header("Control Mode")]
    [SerializeField] private ControlMode controlMode = ControlMode.Mouse;

    private StickController activeStick;
    private bool leftSelected = true;

    private void Start()
    {
        activeStick = leftStick;
        UpdateSelection();

        // 起動時の設定に合わせて制御を初期化
        ApplyControlMode();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        // Vキー：Mouse / Vicon切り替え
        if (keyboard.vKey.wasPressedThisFrame)
        {
            controlMode = controlMode == ControlMode.Mouse
                ? ControlMode.Vicon
                : ControlMode.Mouse;

            ApplyControlMode();
        }

        // Viconモードではマウス操作を行わない
        if (controlMode != ControlMode.Mouse)
            return;

        // Tabキー：操作するスティックを切り替え
        if (keyboard.tabKey.wasPressedThisFrame)
        {
            leftSelected = !leftSelected;
            activeStick = leftSelected ? leftStick : rightStick;

            UpdateSelection();
        }

        if (activeStick == null)
            return;

        Mouse mouse = Mouse.current;

        Vector2 mouseDelta = mouse != null
            ? mouse.delta.ReadValue()
            : Vector2.zero;

        Vector3 moveInput = Vector3.zero;

        if (keyboard.wKey.isPressed) moveInput.z += 1f;
        if (keyboard.sKey.isPressed) moveInput.z -= 1f;
        if (keyboard.dKey.isPressed) moveInput.x += 1f;
        if (keyboard.aKey.isPressed) moveInput.x -= 1f;
        if (keyboard.eKey.isPressed) moveInput.y += 1f;
        if (keyboard.qKey.isPressed) moveInput.y -= 1f;

        if (moveInput.sqrMagnitude > 1f)
            moveInput.Normalize();

        activeStick.ApplyMouseInput(mouseDelta, moveInput);
    }

    private void ApplyControlMode()
    {
        bool useVicon = controlMode == ControlMode.Vicon;

        if (useVicon)
        {
            // 先にマウス操作を停止
            SetMouseControl(false);

            // その後、マーカー追従を開始
            SetMarkerFollowing(true);
        }
        else
        {
            // 先にマーカー追従を停止
            SetMarkerFollowing(false);

            // 現在位置をマウス操作の開始位置に同期
            SyncStickControllers();

            // 最後にマウス操作を再開
            SetMouseControl(true);
        }

        Debug.Log(
            $"[StickInputManager] 操作モード: {controlMode}");
    }

    private void SetMouseControl(bool enabled)
    {
        if (leftStick != null)
            leftStick.SetMouseControlEnabled(enabled);

        if (rightStick != null)
            rightStick.SetMouseControlEnabled(enabled);
    }

    private void SetMarkerFollowing(bool enabled)
    {
        if (leftMarkerFollower != null)
            leftMarkerFollower.SetFollowing(enabled);

        if (rightMarkerFollower != null)
            rightMarkerFollower.SetFollowing(enabled);
    }

    private void SyncStickControllers()
    {
        if (leftStick != null)
            leftStick.SyncControlToCurrentTransform();

        if (rightStick != null)
            rightStick.SyncControlToCurrentTransform();
    }

    private void UpdateSelection()
    {
        if (leftStick != null)
            leftStick.SetSelected(leftSelected);

        if (rightStick != null)
            rightStick.SetSelected(!leftSelected);
    }
}
