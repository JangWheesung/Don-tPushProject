using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class DealDamageOnContact : NetworkBehaviour
{
    private PlayerMovement playerMovement;
    private Rigidbody2D _rigidbody2D;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    public void Slickback(Vector2 vec)
    {
        StartCoroutine(playerMovement.NuckBack(vec));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsOwner) return;

        if (collision.transform.TryGetComponent<Health>(out Health health))
        {
            health.TakeIDServerRPC(OwnerClientId);
        }

        if (playerMovement.isDash && collision.transform.TryGetComponent<DealDamageOnContact>
            (out DealDamageOnContact dealDamageOnContact))
        {
            StartCoroutine(playerMovement.Noise());
            dealDamageOnContact.KnockBackServerRPC(playerMovement.dashVec);
        }

    }

    [ServerRpc(RequireOwnership = false)]
    public void KnockBackServerRPC(Vector3 dir) 
    {
        KnockBackClientRPC(dir);
    }

    [ClientRpc]
    private void KnockBackClientRPC(Vector3 dir) 
    {
        Slickback(dir);
    }
}
