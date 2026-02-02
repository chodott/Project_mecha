using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoomButton : NetworkBehaviour
{
    [SerializeField] private Text _codeText;
    [SerializeField] private InputField _codeInputField;
    [SerializeField] private Button _startGameButton;

    private void Start()
    {
        if (_startGameButton != null)
        {
            _startGameButton.interactable = false;
        }
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
        }
    }

    public async void OnClickCreateRoom()
    {
        string joinCode = await RelayManager.Instance.CreateRelay();

        _codeText.text = joinCode;
    }

    public void OnClickJoinRoom()
    {
        string code = _codeInputField.text;
        if(!string.IsNullOrEmpty(code))
        {
            RelayManager.Instance.JoinRelay(code);
        }
    }

    public void StartGame()
    {
        //BindStartButton
        if (!IsServer)
        {
            return;
        }

        var status = NetworkManager.Singleton.SceneManager.LoadScene(
            "MainScene",
            UnityEngine.SceneManagement.LoadSceneMode.Single);

        if (status != SceneEventProgressStatus.Started)
        {
            Debug.LogWarning($"Scene Loading Failed");
        }
    }

    private void HandleClientConnected(ulong clientID)
    {
        if (!IsServer)
        {
            return;
        }

        CheckPlayerCount();
    }

    private void HandleClientDisconnected(ulong clientID)
    {
        if (!IsServer)
        {
            return;
        }
        CheckPlayerCount();
    }

    private void CheckPlayerCount()
    {
        int playerCount = NetworkManager.Singleton.ConnectedClients.Count;
        if (playerCount >= 2)
        {
            _startGameButton.interactable = true;
        }
        else
        {
            _startGameButton.interactable = false;
        }
    }

    public override void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
        }
        base.OnDestroy();
    }
}
