using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class RankBoardBehaviour : NetworkBehaviour
{
    [SerializeField] private RecordUI _recordPrefab;
    [SerializeField] private RecordUI _myRecord;
    [SerializeField] private RectTransform _recordParentTrm;

    private NetworkList<RankBoardEntityState> _rankList;

    private List<RecordUI> _rankUIList = new List<RecordUI>();

    private void Awake()
    {
        _rankList = new NetworkList<RankBoardEntityState>();
    }

    public override void OnNetworkSpawn()
    {
        //클라이언트면 
        // 랭크리스트에 변화에 리스닝을 해줘야 겠지?
        // 맨 처음 접속시에는 리스트에 있는 모든 애들을 추가하는 작업도 해야해
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


        //서버면 

        if(IsServer)
        {
            ServerSingleton.Instance.NetServer.OnUserJoin += HandleUserJoin;
            ServerSingleton.Instance.NetServer.OnUserLeft += HandleUserLeft;
        }
    }

    public override void OnNetworkDespawn()
    {
        //여기다 클라도 알잘딱 끊어줘야 한다.
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
        //랭킹보드에 추가를 해줘야겠지? 알잘딱으로(리스트에서)
        _rankList.Add(new RankBoardEntityState
        {
            clientID = clientID,
            playerName = userData.username,
            score = 0
        });
    }

    private void HandleUserLeft(ulong clientID, UserData userData)
    {
        //랭킹보드에서 해당 클라이언트 아이디를 제거해줘야겠지?(리스트에서)
        foreach(RankBoardEntityState entity in _rankList)
        {
            if (entity.clientID != clientID) continue;

            try
            {
                _rankList.Remove(entity);
            }catch (Exception ex)
            {
                Debug.LogError(
                    $"{entity.playerName} [ {entity.clientID} ] : 삭제중 오류발생\n {ex.Message}");
            }
            break;
        }
    }


    //서버가 이걸 실행하는거임. 클라는 안건드려
    public void HandleChangeScore(ulong clientID, bool reset = false)
    {
        for(int i = 0; i < _rankList.Count; ++i)
        {
            if (_rankList[i].clientID != clientID) continue;

            var oldItem = _rankList[i];
            var oldScore = reset == true ? -1 : oldItem.score;
            _rankList[i] = new RankBoardEntityState
            {
                clientID = clientID,
                playerName = oldItem.playerName,
                score = oldScore + 1,
            };
            break;
        }
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

    private void AdjustScoreToUIList(RankBoardEntityState value)//된다 씨부럴
    {
        var target = _rankUIList.Find(x => x.clientID == value.clientID);
        target.SetText(1, value.playerName.ToString(), value.score);
        //값을 받아서 해당 UI를 찾아서 (올바른 클라이언트 ID) score를 갱신한다.
        // 선택 : 갱신후에는 UIList를 정렬하고 
        // 정렬된 순서에 맞춰서 실제 UI의 순서도 변경한다.
        // RemoveFromParent => Add
        _rankUIList.Sort((x, y) => y.nowScore.CompareTo(x.nowScore));
        for (int i = 0; i < _rankUIList.Count; i++)
        {
            var ui = _rankUIList[i];
            bool numberOwn = i == 0 ? true : false;
            ui.SetText(i + 1, ui.nowUsername, ui.nowScore, numberOwn);
            ui.transform.SetSiblingIndex(i);

            if (_rankUIList[i].clientID == NetworkManager.Singleton.LocalClientId)
            {
                _myRecord.SetText(i + 1, ui.nowUsername, ui.nowScore);
            }
        }
    }

    private void AddUIToList(RankBoardEntityState value)
    {
        //중복이 있는지 검사후에 만들어서 
        var target = _rankUIList.Find(x => x.clientID == value.clientID);
        if (target != null) return;

        RecordUI newUI = Instantiate(_recordPrefab, _recordParentTrm);
        newUI.SetOwner(value.clientID);
        newUI.SetText(1, value.playerName.ToString(), value.score);
        //만들때 clientID넣어주는거 잊지말자.
        //UI에 추가하고 차후 중복검사를 위해서 _rankUIList 에도 넣어준다.
        _rankUIList.Add(newUI);
        AdjustScoreToUIList(value);
    }

    private void RemoveFromUIList(ulong clientID)
    {
        //_rankUIList 에서 clientID가 일치하는 녀석을 찾아서 리스트에서 제거하고
        var target = _rankUIList.Find(x => x.clientID == clientID);
        if(target != null)
        {
            Debug.Log("UIDestroy");
            _rankUIList.Remove(target);
            Destroy(target.gameObject);
        }
        // 해당 게임오브젝트를 destroy()
    }
}
