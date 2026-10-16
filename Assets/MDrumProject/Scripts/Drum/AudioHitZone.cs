using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AudioHitZone : MonoBehaviour
{
    private AudioHit audioHit;
    private HihatAudioHit hihatAudioHit;
    private Collider zoneCollider;

    private void Awake()
    {
        zoneCollider = GetComponent<Collider>();

        audioHit = GetComponentInParent<AudioHit>();
        hihatAudioHit = GetComponentInParent<HihatAudioHit>();

        zoneCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Stick"))
            return;

        if (audioHit == null && hihatAudioHit == null)
        {
            Debug.LogWarning(
                $"AudioHit / HihatAudioHit が見つかりません: {name}",
                this
            );
            return;
        }

        // Stickの親から先端速度コンポーネントを取得
        TipVelocity tipVelocity =
            other.GetComponentInParent<TipVelocity>();

        if (tipVelocity == null)
        {
            Debug.LogWarning(
                $"StickTipVelocityが見つかりません: {other.name}",
                other
            );
            return;
        }

        // 棒の先端速度を取得
        float velocity = tipVelocity.Speed;

        Debug.Log(
            $"[AudioHitZone] {other.name} / Tip Speed: {velocity:F2} m/s"
            );

        // 既存のAudioHitに速度を渡す
        if (audioHit != null)
        {
            audioHit.ZoneEntered(zoneCollider, other, velocity);
        }
        else if (hihatAudioHit != null)
        {
            hihatAudioHit.ZoneEntered(zoneCollider, other, velocity);
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Stick"))
            return;

        if (audioHit != null)
        {
            audioHit.ZoneExited(zoneCollider, other);
        }
        else if (hihatAudioHit != null)
        {
            hihatAudioHit.ZoneExited(zoneCollider, other);
        }

    }
}
