using Unity.Netcode;
using UnityEngine;

public enum GamePhase { Ready, Playing, End }
public enum GameResult { Player1Win, Player2Win, Draw }
public struct GameResultArgs
{
    public GameResult Result;
    public ulong WinnerID;

    public GameResultArgs(GameResult result, ulong winnerID)
    {
        Result = result;
        WinnerID = winnerID;
    }
}

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    private NetworkVariable<GamePhase> _currentGamePhase = new NetworkVariable<GamePhase>(GamePhase.Ready);
    private NetworkVariable<ulong> _winnerID = new NetworkVariable<ulong>();
    private float _deathLine = -8f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void OnEnable()
    {
        _currentGamePhase.OnValueChanged += OnPhaseChanged;
    }

    private void OnDisable()
    {
        _currentGamePhase.OnValueChanged -= OnPhaseChanged;
    }

    void Update()
    {
        if (!IsServer || _currentGamePhase.Value== GamePhase.End)
        {
            return;
        }

        CheckConditions();
    }

    private void OnPhaseChanged(GamePhase oldStatus, GamePhase newStatus)
    {
        EventBus.OnChangedGamePhase?.Invoke(newStatus);
    }

    private void CheckConditions()
    {
        var clients = NetworkManager.Singleton.ConnectedClientsList;
        if (clients.Count < 2)
        {
            return;
        }

        _currentGamePhase.Value = GamePhase.Playing;

        ulong p1ID = clients[0].ClientId;
        ulong p2ID = clients[1].ClientId;

        Transform p1Transform = clients[0].PlayerObject.transform;
        Transform p2Transform = clients[1].PlayerObject.transform;

        float curDeathY = Camera.main.transform.position.y + _deathLine;

        if (p1Transform.position.y < curDeathY)
        {
            EndGame(p2ID);
        }
        else if (p2Transform.position.y < curDeathY)
        {
            EndGame(p1ID);
        }
    }

    private void EndGame(ulong winnerID)
    {
        _winnerID.Value = winnerID;

        _currentGamePhase.Value = GamePhase.End;

        Debug.Log($"Winner Issssssss: {winnerID}");

        NotifyGameEndClientRpc(winnerID);
    }

    [ClientRpc]
    private void NotifyGameEndClientRpc(ulong winnerID)
    {
        GameResultArgs args = new GameResultArgs { WinnerID = winnerID };
        EventBus.OnGameEnded?.Invoke(args);
    }
}
