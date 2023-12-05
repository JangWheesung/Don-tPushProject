using System;
using Unity.Netcode;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using Cinemachine;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private InputReader _inputReader;
    [Header("자식 요소들")]
    [SerializeField] private CinemachineVirtualCamera vCam;
    [SerializeField] private ParticleSystem dashParticle;
    [SerializeField] private ParticleSystem hitParticle;
    [Header("수치")]
    [SerializeField] private float _movementSpeed;
    [SerializeField] private float _dashSpeed;
    [SerializeField] private float _dashTime;
    [SerializeField] private float _dashDelay;
    [SerializeField] private float _nuckbackDelay;
    [SerializeField] private float maxVelocity;
    [Header("참조값")]
    [SerializeField] private AudioClip dashClip;
    [SerializeField] private AudioClip hitClip;

    [HideInInspector] public Rigidbody2D _rigidbody2D;
    [HideInInspector] public Vector2 dashVec;

    private CinemachineBasicMultiChannelPerlin noise;
    private PlayerAnimation _playerAnimation;
    private AudioSource audioSource;

    private Vector2 _movementInput;
    private Camera _mainCam;

    public bool isDash { get; private set; }
    public bool canDash { get; private set; }
    public bool isBack { get; private set; }

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        noise = vCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        _playerAnimation = transform.Find("Visual").GetComponent<PlayerAnimation>();
        audioSource = GetComponent<AudioSource>();

        _mainCam = Camera.main;
        canDash = true;

        StartCoroutine(Noise(false, 0.5f));
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
        if (!IsOwner) return;
        if (!canDash || isDash || isBack) return;

        isDash = true;
        canDash = false;

        audioSource.clip = dashClip;
        audioSource.Play();
        dashParticle.Play();
        DashParticleServerRPC();

        //dashVec를 마우스 포인터가 있는 방향으로
        Vector2 mousePos = _inputReader.AimPosition;
        Vector3 worldPos = _mainCam.ScreenToWorldPoint(mousePos);
        worldPos.z = 0;

        Vector3 dir = (worldPos - transform.position).normalized;
        dashVec = dir;
        _rigidbody2D.velocity = dashVec * _dashSpeed;

        StartCoroutine(DashColldown());
    }

    [ServerRpc]
    public void DashParticleServerRPC()
    {
        DashParticleClientRpc();
    }

    [ClientRpc]
    private void DashParticleClientRpc()
    {
        if(!IsOwner)
            dashParticle.Play();
    }

    IEnumerator DashColldown()
    {
        yield return new WaitForSeconds(_dashTime);
        dashVec = Vector2.zero;
        _rigidbody2D.velocity = dashVec;
        isDash = false;
        StartCoroutine(DashDelay());
    }

    IEnumerator DashDelay()
    {
        yield return new WaitForSeconds(_dashDelay);
        canDash = true;
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        _playerAnimation.SetMove(_rigidbody2D.velocity.magnitude > 0.1f);
        _playerAnimation.FlipController(_rigidbody2D.velocity.x);

        Velocity();
    }

    private void Velocity()
    {
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
            if (Mathf.Abs(_rigidbody2D.velocity.x) <= 0.15f)
            {
                _rigidbody2D.velocity = new Vector2(0, _rigidbody2D.velocity.y);
                return;
            }
            float symbol = _rigidbody2D.velocity.x > 0 ? -1f : 1f;
            _rigidbody2D.velocity += new Vector2(symbol * _movementSpeed / 2, 0);
        }

        if (_movementInput.y == 0 && _rigidbody2D.velocity.y != 0)
        {
            if (Mathf.Abs(_rigidbody2D.velocity.y) <= 0.15f)
            {
                _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, 0);
                return;
            }
            float symbol = _rigidbody2D.velocity.y > 0 ? -1f : 1f;
            _rigidbody2D.velocity += new Vector2(0, symbol * _movementSpeed / 2);
        }
    }

    public IEnumerator NuckBack(Vector2 vec)
    {
        isBack = true;
        isDash = false;

        audioSource.clip = hitClip;
        audioSource.Play();

        StopCoroutine(DashColldown());
        StartCoroutine(Noise());

        _rigidbody2D.velocity = vec * _dashSpeed;

        yield return new WaitForSeconds(_nuckbackDelay);

        _rigidbody2D.velocity = Vector2.zero;
        isBack = false;
    }

    public IEnumerator Noise(bool isHit = true, float dalayTime = 0.3f)
    {
        if(isHit)
            hitParticle.Play();
        noise.m_FrequencyGain = 1;
        yield return new WaitForSeconds(dalayTime);
        noise.m_FrequencyGain = 0;
    }

    public void DieSound()
    {
        audioSource.clip = hitClip;
        audioSource.Play();
    }
}