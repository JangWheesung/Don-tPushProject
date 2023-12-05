using TMPro;
using UnityEngine;

public class RecordUI : MonoBehaviour
{
    private TextMeshProUGUI _recordText;

    public ulong clientID;

    public int nowRank { get; private set; }
    public string nowUsername { get; private set; }
    public int nowScore { get; private set; }

    private void Awake()
    {
        _recordText = GetComponent<TextMeshProUGUI>();
    }

    public void SetOwner(ulong ownerID)
    {
        clientID = ownerID;
    }

    public void SetText(int rank, string username, int score, bool isOwn = false)
    {
        nowRank = rank;
        nowUsername = username;
        nowScore = score;

        if(isOwn)
            _recordText.color = Color.yellow;
        else
            _recordText.color = Color.white;
        _recordText.SetText($"#{rank.ToString()} {username}  [{score.ToString()}]");
    }
}
