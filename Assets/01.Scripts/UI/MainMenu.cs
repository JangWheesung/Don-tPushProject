using TMPro;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject boardCanvas;
    [SerializeField] private TMP_InputField _txtUsername;


    public void ConnectedToServer()
    {
        string name = _txtUsername.text;
        UserData userData = new UserData
        {
            username = name
        };

        ClientSingleton.Instance.StartClient(userData);
        gameObject.SetActive(false);
        boardCanvas.SetActive(true);
    }
}
