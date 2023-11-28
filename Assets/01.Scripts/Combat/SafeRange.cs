using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeRange : MonoBehaviour
{
    [SerializeField] private float safeRangeRadius;
    [SerializeField] private LayerMask safeRangeLayer;

    public bool IsOverRange()
    {
        return !Physics2D.OverlapCircle(transform.position, safeRangeRadius, safeRangeLayer);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, safeRangeRadius);
    }
#endif
}
