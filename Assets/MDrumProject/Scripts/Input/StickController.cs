
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

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // マウス操作で位置・回転を制御する
        rb.isKinematic = true;
        rb.useGravity = false;

        targetPosition = rb.position;
        targetRotation = rb.rotation;
        initialRotation = rb.rotation;

        // 角度操作の初期値をリセット
        yaw = 0f;
        pitch = 0f;
    }

    public void ApplyMouseInput(
        Vector2 mouseDelta,
        Vector3 worldMoveInput)
    {
        // マウス移動量からスティックの角度を計算
        yaw += mouseDelta.x * rotationSensitivity;
        pitch -= mouseDelta.y * rotationSensitivity;

        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);

        targetRotation =
            initialRotation *
            Quaternion.Euler(pitch, yaw, 0f);

        // ワールド座標基準で移動
        targetPosition +=
            worldMoveInput * moveSpeed * Time.deltaTime;
    }

    private void FixedUpdate()
    {
        rb.MovePosition(targetPosition);
        rb.MoveRotation(targetRotation);
    }

    public void SetSelected(bool selected)
    {
        // 必要なら選択中のスティックを光らせる等の拡張に使う
    }

    public void ResetControl()
    {
        targetPosition = rb.position;
        targetRotation = rb.rotation;

        initialRotation = rb.rotation;
        yaw = 0f;
        pitch = 0f;
    }
}
