using System;
using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private float _movementSpeed;
    [SerializeField] private float _dashSpeed;
    [SerializeField] private float maxVelocity;

    [HideInInspector] public Rigidbody2D _rigidbody2D;
    private PlayerAnimation _playerAnimation;
    private Vector2 _movementInput;

    public bool isDash;

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _playerAnimation = transform.Find("Visual").GetComponent<PlayerAnimation>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        _inputReader.MovementEvent += HandleMovement;
        _inputReader.DashEvent += HandleDash;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;
        _inputReader.MovementEvent -= HandleMovement;
        _inputReader.DashEvent -= HandleDash;
    }

    private void HandleMovement(Vector2 movementInput)
    {
        _movementInput = movementInput;
    }

    private void HandleDash()
    {
        if (_movementInput == Vector2.zero || isDash) return;

        isDash = true;
        _rigidbody2D.velocity = _movementInput * _dashSpeed;
        StartCoroutine(DashColldown());
    }

    IEnumerator DashColldown()
    {
        yield return new WaitForSeconds(0.4f);
        _rigidbody2D.velocity = Vector2.zero;
        isDash = false;
    }

    private void FixedUpdate()
    {
        _playerAnimation.SetMove(_rigidbody2D.velocity.magnitude > 0.1f); 
        _playerAnimation.FlipController( _rigidbody2D.velocity.x );

        if (!IsOwner) return;
        if (isDash) return;

        Vector2 desiredVelocity = _movementInput.normalized * _movementSpeed;

        if ((_rigidbody2D.velocity.x < maxVelocity && _movementInput.x > 0) ||
            (_rigidbody2D.velocity.x > -maxVelocity && _movementInput.x < 0))
        {
            _rigidbody2D.velocity += new Vector2(desiredVelocity.x, 0);
        }

        // y 방향으로의 제한
        if ((_rigidbody2D.velocity.y < maxVelocity && _movementInput.y > 0) ||
            (_rigidbody2D.velocity.y > -maxVelocity && _movementInput.y < 0))
        {
            _rigidbody2D.velocity += new Vector2(0, desiredVelocity.y);
        }
    }
}
