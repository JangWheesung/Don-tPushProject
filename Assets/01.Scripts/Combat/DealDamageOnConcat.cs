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
        Debug.Log("Trigger");
        if (collision.attachedRigidbody == null || !playerMovement.isDash) return;
        Debug.Log("NotReturn");
        if (collision.attachedRigidbody.TryGetComponent<Health>(out Health health))
        {
            Debug.Log("TakeID");
            health.TakeID(OwnerClientId);
        }
    }

}
