using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class RankBoardBehaviour : NetworkBehaviour
{
    [SerializeField] private RecordUI _recordPrefab;
    [SerializeField] private RectTransform _recordParentTrm;

    private NetworkList<RankBoardEntityState> _rankList;

    private List<RecordUI> _rankUIList = new List<RecordUI>();

    private void Awake()
    {
        _rankList = new NetworkList<RankBoardEntityState>();
    }

    public override void OnNetworkSpawn()
    {
        //Ŭ���̾�Ʈ�� 
        // ��ũ����Ʈ�� ��ȭ�� �������� ����� ����?
        // �� ó�� ���ӽÿ��� ����Ʈ�� �ִ� ��� �ֵ��� �߰��ϴ� �۾��� �ؾ���
        if(IsClient)
        {
            _rankList.OnListChanged += HandleRankListChanged;
            foreach(var entity in _rankList)
            {
                HandleRankListChanged(new NetworkListEvent<RankBoardEntityState>
                {
                    Type = NetworkListEvent<RankBoardEntityState>.EventType.Add,
                    Value = entity
                });
            }
        }


        //������ 

        if(IsServer)
        {
            ServerSingleton.Instance.NetServer.OnUserJoin += HandleUserJoin;
            ServerSingleton.Instance.NetServer.OnUserLeft += HandleUserLeft;
        }
    }

    public override void OnNetworkDespawn()
    {
        //����� Ŭ�� ���ߵ� ������� �Ѵ�.
        if(IsClient)
        {
            _rankList.OnListChanged -= HandleRankListChanged;
        }

        if (IsServer)
        {
            ServerSingleton.Instance.NetServer.OnUserJoin -= HandleUserJoin;
            ServerSingleton.Instance.NetServer.OnUserLeft -= HandleUserLeft;
        }
    }

    private void HandleUserJoin(ulong clientID, UserData userData)
    {
        //��ŷ���忡 �߰��� ����߰���? ���ߵ�����(����Ʈ����)
        _rankList.Add(new RankBoardEntityState
        {
            clientID = clientID,
            playerName = userData.username,
            score = 0
        });
    }

    private void HandleUserLeft(ulong clientID, UserData userData)
    {
        //��ŷ���忡�� �ش� Ŭ���̾�Ʈ ���̵� ��������߰���?(����Ʈ����)
        foreach(RankBoardEntityState entity in _rankList)
        {
            if (entity.clientID != clientID) continue;

            try
            {
                _rankList.Remove(entity);
            }catch (Exception ex)
            {
                Debug.LogError(
                    $"{entity.playerName} [ {entity.clientID} ] : ������ �����߻�\n {ex.Message}");
            }
            break;
        }
    }


    //������ �̰� �����ϴ°���. Ŭ��� �Ȱǵ��
    public void HandleChangeScore(ulong clientID)
    {
        for(int i = 0; i < _rankList.Count; ++i)
        {
            if (_rankList[i].clientID != clientID) continue;

            var oldItem = _rankList[i];
            var oldScore = oldItem.score;
            _rankList[i] = new RankBoardEntityState
            {
                clientID = clientID,
                playerName = oldItem.playerName,
                score = oldScore + 1,
            };
            break;
        }
        //_rankUIList.Sort();
        //_rankList.
        //_rankList.Sort((x, y) => x.score.CompareTo(y.score));
    }


    private void HandleRankListChanged(NetworkListEvent<RankBoardEntityState> evt)
    {
        switch (evt.Type)
        {
            case NetworkListEvent<RankBoardEntityState>.EventType.Add:
                AddUIToList(evt.Value);
                break;
            case NetworkListEvent<RankBoardEntityState>.EventType.Remove:
                RemoveFromUIList(evt.Value.clientID);
                break;
            case NetworkListEvent<RankBoardEntityState>.EventType.Value:
                AdjustScoreToUIList(evt.Value);
                break;
        }
    }

    private void AdjustScoreToUIList(RankBoardEntityState value)//�ȴ� ���η�
    {
        var target = _rankUIList.Find(x => x.clientID == value.clientID);
        target.SetText(1, value.playerName.ToString(), value.score);
        //���� �޾Ƽ� �ش� UI�� ã�Ƽ� (�ùٸ� Ŭ���̾�Ʈ ID) score�� �����Ѵ�.
        // ���� : �����Ŀ��� UIList�� �����ϰ� 
        // ���ĵ� ������ ���缭 ���� UI�� ������ �����Ѵ�.
        // RemoveFromParent => Add
        _rankUIList.Sort((x, y) => x.nowScore.CompareTo(y.nowScore));
        for (int i = 0; i < _rankUIList.Count; i++)
        {
            var ui = _rankUIList[i];
            //ui.SetText(i + 1, ui.p);
        }
        }

        private void AddUIToList(RankBoardEntityState value)
    {
        //�ߺ��� �ִ��� �˻��Ŀ� ���� 
        var target = _rankUIList.Find(x => x.clientID == value.clientID);
        if (target != null) return;

        RecordUI newUI = Instantiate(_recordPrefab, _recordParentTrm);
        newUI.SetOwner(value.clientID);
        newUI.SetText(1, value.playerName.ToString(), value.score);
        //���鶧 clientID�־��ִ°� ��������.
        //UI�� �߰��ϰ� ���� �ߺ��˻縦 ���ؼ� _rankUIList ���� �־��ش�.
        _rankUIList.Add(newUI);
    }

    private void RemoveFromUIList(ulong clientID)
    {
        //_rankUIList ���� clientID�� ��ġ�ϴ� �༮�� ã�Ƽ� ����Ʈ���� �����ϰ�
        var target = _rankUIList.Find(x => x.clientID == clientID);
        if(target != null)
        {
            Debug.Log("UIDestroy");
            _rankUIList.Remove(target);
            Destroy(target.gameObject);
        }
        // �ش� ���ӿ�����Ʈ�� destroy()
    }
}
