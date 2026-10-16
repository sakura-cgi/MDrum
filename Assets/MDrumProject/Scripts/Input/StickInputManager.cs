
using UnityEngine;
using UnityEngine.InputSystem;

public class StickInputManager : MonoBehaviour
{
    public enum ControlMode
    {
        Mouse,
        VR
    }

    [Header("Stick References")]
    [SerializeField] private StickController leftStick;
    [SerializeField] private StickController rightStick;

    [Header("Control Mode")]
    [SerializeField]
    private ControlMode controlMode =
        ControlMode.Mouse;

    private StickController activeStick;
    private bool leftSelected = true;

    private void Start()
    {
        activeStick = leftStick;
        UpdateSelection();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;

        if (keyboard == null)
            return;

        // Vキー：マウス／VRモード切替
        if (keyboard.vKey.wasPressedThisFrame)
        {
            controlMode =
                controlMode == ControlMode.Mouse
                ? ControlMode.VR
                : ControlMode.Mouse;
        }

        // Tabキー：操作するスティックを切替
        if (keyboard.tabKey.wasPressedThisFrame)
        {
            leftSelected = !leftSelected;
            activeStick =
                leftSelected ? leftStick : rightStick;

            UpdateSelection();
        }

        // VRモードではマウス入力を受け付けない
        if (controlMode != ControlMode.Mouse ||
            activeStick == null)
        {
            return;
        }

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

        // 斜め移動が速くならないようにする
        if (moveInput.sqrMagnitude > 1f)
            moveInput.Normalize();

        activeStick.ApplyMouseInput(mouseDelta, moveInput);
    }

    private void UpdateSelection()
    {
        if (leftStick != null)
            leftStick.SetSelected(leftSelected);

        if (rightStick != null)
            rightStick.SetSelected(!leftSelected);
    }
}
