using Cinemachine;
using System;
using TMPro;
using Unity.Collections;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] private TextMeshPro _nameText;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private CinemachineVirtualCamera _followCam;
    [SerializeField] private ParticleSystem dieParticle;

    public static event Action<Player> OnPlayerSpawned;
    public static event Action<Player> OnPlayerDeSpawned;

    public Health HealthCompo { get; private set; }
    private PlayerMovement playerMovement;
    private NetworkVariable<FixedString32Bytes> _username = new NetworkVariable<FixedString32Bytes>();

    private void Awake()
    {
        HealthCompo = GetComponent<Health>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void HandleDie(Health health) //죽는효과
    {
        dieParticle.Play();
        StartCoroutine(playerMovement.Noise(false));
        spriteRenderer.color = Color.gray;
        if (IsServer)
        {
            StartCoroutine(DieDelay());
        }
        //여기다가 파티클이나 뭐 죽는 효과 같은게 나와야겠지만...일단은.
    }

    public override void OnNetworkSpawn()
    {
        
        _username.OnValueChanged += HandleNameChanged;
        HealthCompo.OnDie += HandleDie;
        HandleNameChanged("", _username.Value);
        if(IsOwner)
        {
            _followCam.Priority = 15;
        }

        if(IsServer)
        {
            OnPlayerSpawned?.Invoke(this);
        }
    }

    public override void OnNetworkDespawn()
    {
        Debug.Log("Player Die");
        _username.OnValueChanged -= HandleNameChanged;
        HealthCompo.OnDie -= HandleDie;
        if (IsServer)
        {
            OnPlayerDeSpawned?.Invoke(this);
        }
    }



    private void HandleNameChanged(FixedString32Bytes prev, FixedString32Bytes newValue)
    {
        _nameText.text = newValue.ToString();
    }

    public void SetUserName(string username)
    {
        _username.Value = username;
    }

    private IEnumerator DieDelay()
    {
        yield return new WaitForSeconds(0.3f);
        Destroy(gameObject);
    }
}
