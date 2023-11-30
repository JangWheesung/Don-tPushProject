using System;
using Unity.Netcode;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private float _movementSpeed;
    [SerializeField] private float _dashSpeed;
    [SerializeField] private float _nuckbackDelay;
    [SerializeField] private float maxVelocity;
    
    [HideInInspector] public Rigidbody2D _rigidbody2D;
    [HideInInspector] public Vector2 dashVec;
    private PlayerAnimation _playerAnimation;
    private Vector2 _movementInput;
    [SerializeField] private GameObject dast;

    public bool isDash { get; private set; }
    public bool isBack { get; private set; }

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
        if (_rigidbody2D.velocity == Vector2.zero || isDash || isBack) return;

        isDash = true;
        dashVec = _rigidbody2D.velocity.normalized;
        _rigidbody2D.velocity = dashVec * _dashSpeed;
        StartCoroutine(DashColldown());
    }

    IEnumerator DashColldown()
    {
        yield return new WaitForSeconds(0.2f);
        dashVec = Vector2.zero;
        _rigidbody2D.velocity = dashVec;
        isDash = false;
    }

    private void FixedUpdate()
    {
        _playerAnimation.SetMove(_rigidbody2D.velocity.magnitude > 0.1f); 
        _playerAnimation.FlipController( _rigidbody2D.velocity.x );

        Velocity();
    }

    private void Velocity()
    {
        if (!IsOwner) return;
        if (isDash || isBack) return;

        Vector2 desiredVelocity = _movementInput.normalized * _movementSpeed;

        if ((_rigidbody2D.velocity.x < maxVelocity && _movementInput.x > 0) ||
            (_rigidbody2D.velocity.x > -maxVelocity && _movementInput.x < 0))
        {
            _rigidbody2D.velocity += new Vector2(desiredVelocity.x, 0);
        }

        if ((_rigidbody2D.velocity.y < maxVelocity && _movementInput.y > 0) ||
            (_rigidbody2D.velocity.y > -maxVelocity && _movementInput.y < 0))
        {
            _rigidbody2D.velocity += new Vector2(0, desiredVelocity.y);
        }

        if (_movementInput.x == 0 && _rigidbody2D.velocity.x != 0)
        {
            float symbol = _rigidbody2D.velocity.x > 0 ? -1f : 1f;
            _rigidbody2D.velocity += new Vector2(symbol * _movementSpeed / 2, 0);
        }

        if (_movementInput.y == 0 && _rigidbody2D.velocity.y != 0)
        {
            float symbol = _rigidbody2D.velocity.y > 0 ? -1f : 1f;
            _rigidbody2D.velocity += new Vector2(0, symbol * _movementSpeed / 2);
        }
    }

    public IEnumerator NuckBack(Vector2 vec)
    {
        isBack = true;
        isDash = false;
        StopCoroutine(DashColldown());
        //Instantiate(dast, vec, Quaternion.identity);
        _rigidbody2D.velocity = vec * _dashSpeed;
        yield return new WaitForSeconds(_nuckbackDelay);
        _rigidbody2D.velocity = Vector2.zero;
        isBack = false;
    }
}
