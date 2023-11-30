using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class DealDamageOnContact : NetworkBehaviour
{
    private PlayerMovement playerMovement;
    private Rigidbody2D _rigidbody2D;

    [SerializeField] GameObject test1;
    [SerializeField] GameObject test2;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    public void Slickback(Vector2 vec)
    {
        Instantiate(test2, transform.position, Quaternion.identity);
        StartCoroutine(playerMovement.NuckBack(vec));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.attachedRigidbody == null) return;

        if (collision.attachedRigidbody.TryGetComponent<Health>(out Health health))
        {
            health.TakeID(OwnerClientId);
        }

        if (playerMovement.isDash && collision.attachedRigidbody.TryGetComponent<DealDamageOnContact>
            (out DealDamageOnContact dealDamageOnContact))
        {
            Instantiate(test1, dealDamageOnContact.transform.position, Quaternion.identity);
            dealDamageOnContact.Slickback(Vector2.left);
        }
    }
}
