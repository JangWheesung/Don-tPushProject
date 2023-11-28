using Unity.Netcode;
using UnityEngine;

public class DealDamageOnContact : NetworkBehaviour
{
    private PlayerMovement playerMovement;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.attachedRigidbody == null || playerMovement.isDash) return;

        if (collision.attachedRigidbody.TryGetComponent<Health>(out Health health))
        {
            health.TakeID(OwnerClientId);
        }
    }

}
