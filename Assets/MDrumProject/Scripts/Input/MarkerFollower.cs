using UnityEngine;

[ExecuteAlways]
public class MarkerFollower : MonoBehaviour
{
    [Header("Follow Target")]
    [SerializeField] private Transform targetTransform;

    [Header("Follow Settings")]
    [SerializeField] private bool followPosition = true;
    [SerializeField] private bool followRotation = true;

    [Header("Offset")]
    [SerializeField] private bool keepOffset = false;
    [SerializeField] private Vector3 positionOffset = Vector3.zero;
    [SerializeField] private Vector3 rotationOffset = Vector3.zero;

    [Header("Editor Settings")]
    [SerializeField] private bool followInEditMode = true;

    [Header("Runtime Control")]
    [SerializeField] private bool followingEnabled = true;

    private Transform cachedTarget;
    private bool offsetInitialized;

    private Vector3 savedPositionOffset;
    private Quaternion savedRotationOffset;

    private void OnEnable()
    {
        ResetOffsetCache();
    }

    private void LateUpdate()
    {
        if (!followingEnabled)
            return;

        if (!Application.isPlaying && !followInEditMode)
            return;

        if (targetTransform == null || targetTransform == transform)
            return;

        if (cachedTarget != targetTransform)
        {
            cachedTarget = targetTransform;
            offsetInitialized = false;
        }

        if (keepOffset && !offsetInitialized)
            CaptureOffset();

        if (keepOffset)
        {
            if (followPosition)
            {
                targetTransform.position =
                    transform.TransformPoint(savedPositionOffset);
            }

            if (followRotation)
            {
                targetTransform.rotation =
                    transform.rotation * savedRotationOffset;
            }
        }
        else
        {
            if (followPosition)
            {
                targetTransform.position =
                    transform.TransformPoint(positionOffset);
            }

            if (followRotation)
            {
                targetTransform.rotation =
                    transform.rotation *
                    Quaternion.Euler(rotationOffset);
            }
        }
    }

    public void SetFollowing(bool enabled)
    {
        followingEnabled = enabled;
    }

    public bool IsFollowing()
    {
        return followingEnabled;
    }

    private void CaptureOffset()
    {
        savedPositionOffset =
            transform.InverseTransformPoint(targetTransform.position);

        savedRotationOffset =
            Quaternion.Inverse(transform.rotation) *
            targetTransform.rotation;

        offsetInitialized = true;
    }

    public void RecaptureOffset()
    {
        if (targetTransform == null || targetTransform == transform)
            return;

        CaptureOffset();
    }

    private void ResetOffsetCache()
    {
        cachedTarget = null;
        offsetInitialized = false;
    }
}
