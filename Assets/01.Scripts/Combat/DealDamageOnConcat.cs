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

    private void OnCollisionEnter2D(Collision2D collision)
    {

        Debug.Log(123);

        if (!IsOwner) return;
        //if (collision.attachedRigidbody == null) return;

        if (collision.transform.TryGetComponent<Health>(out Health health))
        {
            health.TakeID(OwnerClientId);
        }

        Debug.Log(playerMovement.isDash);

        if (playerMovement.isDash && collision.transform.TryGetComponent<DealDamageOnContact>
            (out DealDamageOnContact dealDamageOnContact))
        {//이로써 3번의 충돌 중 한번은 자기 참조
            //서버rpc 날리기
            //Instantiate(test1, dealDamageOnContact.transform.position, Quaternion.identity);
            //dealDamageOnContact.Slickback(Vector2.left);
            Debug.Log("F");
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

        Instantiate(test2, transform.position, Quaternion.identity);
        Slickback(dir);

    }

}
