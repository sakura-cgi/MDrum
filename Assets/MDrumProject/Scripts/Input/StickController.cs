using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class StickController : MonoBehaviour
{
    [Header("Mouse Rotation")]
    [SerializeField] private float rotationSensitivity = 0.15f;
    [SerializeField] private float maxPitch = 80f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.5f;

    private Rigidbody rb;

    private Vector3 targetPosition;
    private Quaternion targetRotation;

    private float yaw;
    private float pitch;

    private Quaternion initialRotation;
    private bool mouseControlEnabled = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.useGravity = false;

        SyncControlToCurrentTransform();
    }

    public void ApplyMouseInput(
        Vector2 mouseDelta,
        Vector3 worldMoveInput)
    {
        if (!mouseControlEnabled)
            return;

        yaw += mouseDelta.x * rotationSensitivity;
        pitch -= mouseDelta.y * rotationSensitivity;
        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);

        targetRotation =
            initialRotation *
            Quaternion.Euler(pitch, yaw, 0f);

        targetPosition +=
            worldMoveInput * moveSpeed * Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (!mouseControlEnabled)
            return;

        rb.MovePosition(targetPosition);
        rb.MoveRotation(targetRotation);
    }

    // マウス操作の有効・無効を切り替える
    public void SetMouseControlEnabled(bool enabled)
    {
        mouseControlEnabled = enabled;
    }

    // 現在のTransformをマウス操作の開始位置にする
    public void SyncControlToCurrentTransform()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        targetPosition = transform.position;
        targetRotation = transform.rotation;

        rb.position = transform.position;
        rb.rotation = transform.rotation;

        initialRotation = transform.rotation;
        yaw = 0f;
        pitch = 0f;
    }

    public void SetSelected(bool selected)
    {
        // 選択中の表示などに使う
    }

    public void ResetControl()
    {
        SyncControlToCurrentTransform();
    }
}
