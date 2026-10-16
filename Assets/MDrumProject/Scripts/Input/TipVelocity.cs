
using UnityEngine;

public class TipVelocity : MonoBehaviour
{
    [SerializeField] private Transform tipPoint;

    public Vector3 Velocity { get; private set; }
    public float Speed => Velocity.magnitude;

    private Vector3 previousPosition;
    private bool initialized;

    private void LateUpdate()
    {
        if (tipPoint == null) return;

        Vector3 currentPosition = tipPoint.position;

        if (initialized && Time.deltaTime > 0f)
        {
            Velocity =
                (currentPosition - previousPosition)
                / Time.deltaTime;
        }
        else
        {
            Velocity = Vector3.zero;
            initialized = true;
        }

        previousPosition = currentPosition;
    }

    public void ResetVelocity()
    {
        if (tipPoint == null) return;

        previousPosition = tipPoint.position;
        Velocity = Vector3.zero;
        initialized = true;
    }
}
