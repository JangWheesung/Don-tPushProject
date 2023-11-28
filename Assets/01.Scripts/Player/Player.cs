using Cinemachine;
using System;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] private TextMeshPro _nameText;
    [SerializeField] private CinemachineVirtualCamera _followCam;

    public static event Action<Player> OnPlayerSpawned;
    public static event Action<Player> OnPlayerDeSpawned;

    private NetworkObject networkObject;

    public Health HealthCompo { get; private set; }
    private NetworkVariable<FixedString32Bytes> _username = new NetworkVariable<FixedString32Bytes>();

    private void Awake()
    {
        networkObject = GetComponent<NetworkObject>();
        HealthCompo = GetComponent<Health>();
    }

    private void HandleDie(Health health) //�״�ȿ��
    {
        networkObject.Despawn();
        //����ٰ� ��ƼŬ�̳� �� �״� ȿ�� ������ ���;߰�����...�ϴ���.
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
}
