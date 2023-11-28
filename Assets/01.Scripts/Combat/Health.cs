using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class Health : NetworkBehaviour
{
    [SerializeField] private float deleteIDTime;

    private Coroutine resetCoroutine;
    private bool _isDead = false;

    public Action<Health> OnDie;

    public ulong LastHitDealerID { get; private set; }

    public const ulong beenID = 0;

    private void Awake()
    {
        LastHitDealerID = beenID;
    }

    private void Update()
    {
        if (!Physics2D.OverlapCircle(transform.position, transform.localScale.x, LayerMask.GetMask("SafeRange")) && !_isDead)
        {
            Debug.Log("аж╠щ");
            _isDead = true;
            OnDie?.Invoke(this);
        }
    }

    public void TakeID(ulong dealerID)
    {
        if (_isDead) return;

        LastHitDealerID = dealerID;

        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
        }
        resetCoroutine = StartCoroutine(DeleteID());
    }

    private IEnumerator DeleteID()
    {
        yield return new WaitForSeconds(deleteIDTime);
        LastHitDealerID = beenID;
    }
}
