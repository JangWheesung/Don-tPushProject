using System;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class PlayerAnimation : NetworkBehaviour
{
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;

    private readonly int _isMoveHash = Animator.StringToHash("is_move");

    private NetworkVariable<bool> _isFlip;
    private NetworkVariable<bool> _isMove;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        _isFlip = new NetworkVariable<bool>();
        _isMove = new NetworkVariable<bool>();
    }


    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            _isFlip.OnValueChanged += HandleFlipChanged;
            _isMove.OnValueChanged += HandleMoveChanged;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner)
        {
            _isFlip.OnValueChanged -= HandleFlipChanged;
            _isMove.OnValueChanged -= HandleMoveChanged;
        }
    }

    private void HandleFlipChanged(bool previousValue, bool newValue)
    {
        _spriteRenderer.flipX = newValue;
    }

    private void HandleMoveChanged(bool previousValue, bool newValue)
    {
        _animator.SetBool(_isMoveHash, newValue);
    }

    // 이 함수는 오너만 실행한다.
    public void SetMove(bool value)
    {
        if (_isMove.Value != value)
        {
            SetIsMoveServerRpc(value);
        }
        _animator.SetBool(_isMoveHash, value);
    }

    [ServerRpc]
    private void SetIsMoveServerRpc(bool value)
    {
        _isMove.Value = value;
    }

    [ServerRpc]
    private void SetIsFlipServerRpc(bool value)
    {
        _isFlip.Value = value;
    }

    // 이 함수는 오너만 실행한다.
    public void FlipController(float xDirection)
    {
        bool isRightTurn = xDirection > 0 && _spriteRenderer.flipX;
        bool isLeftTurn = xDirection < 0 && !_spriteRenderer.flipX;
        if (isRightTurn || isLeftTurn)
        {
            Flip();
        }
    }

    public void Flip()
    {
        _spriteRenderer.flipX = !_spriteRenderer.flipX;
        SetIsFlipServerRpc(_spriteRenderer.flipX);
    }
}