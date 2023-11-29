using TMPro;
using UnityEngine;

public class RecordUI : MonoBehaviour
{
    private TextMeshProUGUI _recordText;

    public ulong clientID;

    public int nowRank { get; private set; }
    public int nowScore { get; private set; }
    public int nowUsername { get; private set; }

    private void Awake()
    {
        _recordText = GetComponent<TextMeshProUGUI>();
    }

    public void SetOwner(ulong ownerID)
    {
        clientID = ownerID;
    }

    public void SetText(int rank, string username, int score)
    {
        nowScore = score;
        _recordText.SetText($"{rank.ToString()} . {username} [ {score.ToString()} ]");
    }
}
