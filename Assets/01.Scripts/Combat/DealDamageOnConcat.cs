using Unity.Netcode;
using UnityEngine;

public class DealDamageOnContact : NetworkBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.attachedRigidbody == null) return;
        Debug.Log("NotReturn");
        if (collision.attachedRigidbody.TryGetComponent<Health>(out Health health))
        {
            Debug.Log("TakeID");
            health.TakeID(OwnerClientId);
        }
    }

}
