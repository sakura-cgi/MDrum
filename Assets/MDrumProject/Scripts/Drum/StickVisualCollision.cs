/*
using UnityEngine;

public class StickVisualCollision : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform holdPoint;
    [SerializeField] private Transform visualTransform;

    [Header("Collision")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float castRadius = 0.015f;
    [SerializeField] private float skinWidth = 0.005f;

    private bool isBlocked;
    private Vector3 visualPosition;

    private void Start()
    {
        if (holdPoint == null || visualTransform == null)
        {
            Debug.LogError(
                "[StickVisualCollision] References are missing.",
                this
            );
            enabled = false;
            return;
        }

        visualPosition = visualTransform.position;
    }

    private void LateUpdate()
    {
        Vector3 targetPosition = holdPoint.TransformPoint(
            visualTransform.localPosition
        );

        // 現在の表示位置から入力位置への移動を検査
        Vector3 movement = targetPosition - visualPosition;
        float distance = movement.magnitude;

        if (distance > 0.0001f)
        {
            Vector3 direction = movement / distance;

            if (Physics.SphereCast(
                visualPosition,
                castRadius,
                direction,
                out RaycastHit hit,
                distance,
                obstacleLayer,
                QueryTriggerInteraction.Ignore))
            {
                isBlocked = true;

                float allowedDistance =
                    Mathf.Max(0f, hit.distance - skinWidth);

                visualPosition += direction * allowedDistance;
            }
            else
            {
                isBlocked = false;
                visualPosition = targetPosition;
            }
        }
        else
        {
            isBlocked = false;
        }

        // 親の移動に関係なく、表示位置をワールド座標で維持
        visualTransform.position = visualPosition;
        visualTransform.rotation = holdPoint.rotation;
    }
}
*/