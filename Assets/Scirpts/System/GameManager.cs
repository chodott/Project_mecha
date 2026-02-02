using Unity.Netcode;
using UnityEngine;

public enum GameStatus { Ready, Playing, Player1Win, Player2Win, Draw }

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    private NetworkVariable<GameStatus> _currentGameStatus = new NetworkVariable<GameStatus>(GameStatus.Ready);

    private float _deathLine = -8f;
    private bool _isGameEnd = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private void OnEnable()
    {
        _currentGameStatus.OnValueChanged += OnStatusChanged;
    }

    private void OnDisable()
    {
        _currentGameStatus.OnValueChanged -= OnStatusChanged;
    }

    void Update()
    {
        if(!IsServer || _isGameEnd)
        {
            return;
        }

        CheckConditions();
    }

    private void OnStatusChanged(GameStatus oldStatus, GameStatus newStatus)
    {
        EventBus.OnChangedGameStatus?.Invoke(newStatus);
    }

    private void CheckConditions()
    {
        var clients = NetworkManager.Singleton.ConnectedClientsList;
        if(clients.Count < 2)
        {
            return;
        }

        _currentGameStatus.Value = GameStatus.Playing;

        ulong p1ID = clients[0].ClientId;
        ulong p2ID = clients[1].ClientId;

        Transform p1Transform = clients[0].PlayerObject.transform;
        Transform p2Transform = clients[1].PlayerObject.transform;

        float curDeathY = Camera.main.transform.position.y + _deathLine;

        if(p1Transform.position.y <curDeathY)
        {
            EndGame(GameStatus.Player2Win);
        }
        else if(p2Transform.position.y < curDeathY)
        {
            EndGame(GameStatus.Player1Win); 
        }
    }

    private void EndGame(GameStatus gameStatus)
    {
        _isGameEnd = true;
        _currentGameStatus.Value = gameStatus;

        Debug.Log($"Game Ended with Status: {gameStatus}");

        ShowStatusClientRpc(gameStatus);
    }

    [ClientRpc]
    private void ShowStatusClientRpc(GameStatus gameStatus)
    {
        Debug.Log($"Game Status on Client: {gameStatus}");
    }
}
