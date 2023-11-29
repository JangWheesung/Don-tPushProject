using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class RespawnManager : NetworkBehaviour
{
    [SerializeField] private RankBoardBehaviour rankBoardBehaviour;

    public override void OnNetworkSpawn()
    {
        // Player�� �ִ�  OnPlayerDespawn�� �����ϰ�
        // �� ���������� �ؾ���. ����? ������ �ؾ���
        if (!IsServer) return;
        Player.OnPlayerDeSpawned += HandlePlayerDeSpawn;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;
        Player.OnPlayerDeSpawned -= HandlePlayerDeSpawn;
    }


    private void HandlePlayerDeSpawn(Player player)
    {
        ulong killerID = player.HealthCompo.LastHitDealerID;
        ulong beenID = player.HealthCompo.beenID;
        Debug.Log(killerID);

        UserData killerUserdata = ServerSingleton.Instance.NetServer.GetUserDataByClientID(killerID);
        UserData victimUserData = ServerSingleton.Instance.NetServer.GetUserDataByClientID(player.OwnerClientId);
        if(victimUserData != null)
        {
            if (killerID != beenID)
            {
                Debug.Log($"{victimUserData.username} is dead by {killerUserdata.username} [{killerID}]");
                rankBoardBehaviour.HandleChangeScore(killerID);
                rankBoardBehaviour.HandleChangeScore(player.OwnerClientId, true);
            }

            //������ �������� 3���� ������ �ǵ��� �Լ��� �����
            StartCoroutine(DelayRespawn(player.OwnerClientId));
        }

    }


    IEnumerator DelayRespawn(ulong clientID)
    {
        yield return new WaitForSeconds(3f);
        ServerSingleton.Instance.NetServer.RespawnPlayer(clientID);
    }
}
