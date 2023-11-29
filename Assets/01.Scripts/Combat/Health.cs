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

    public ulong beenID { get; private set; }

    private void Awake()
    {
        beenID = 0;
        LastHitDealerID = beenID;
    }

    private void Update()
    {
        if (!Physics2D.OverlapCircle(transform.position, transform.localScale.x, LayerMask.GetMask("SafeRange")) && !_isDead)
        {
            Debug.Log("overDead");
            _isDead = true;
            OnDie?.Invoke(this);
        }
    }

    public void TakeID(ulong dealerID)
    {
        if (_isDead) return;
        Debug.Log("NoDead");
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
        Debug.Log("ReBeen");
        LastHitDealerID = beenID;
    }
}
