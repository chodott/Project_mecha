using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameResult { Playing, Player1Win, Player2Win, Draw }
    public NetworkVariable<GameResult> CurrentGameResult = new NetworkVariable<GameResult>(GameResult.Playing);


    [SerializeField] private CameraScroller _cameraScroller;
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

    void Update()
    {
        if(!IsServer || _isGameEnd)
        {
            return;
        }

        CheckConditions();
    }

    private void CheckConditions()
    {
        var clients = NetworkManager.Singleton.ConnectedClientsList;
        if(clients.Count < 2)
        {
            return;
        }

        ulong p1ID = clients[0].ClientId;
        ulong p2ID = clients[1].ClientId;

        Transform p1Transform = clients[0].PlayerObject.transform;
        Transform p2Transform = clients[1].PlayerObject.transform;

        float curDeathY = Camera.main.transform.position.y + _deathLine;

        if(p1Transform.position.y <curDeathY)
        {
            EndGame(GameResult.Player2Win);
        }
        else if(p2Transform.position.y < curDeathY)
        {
            EndGame(GameResult.Player1Win); 
        }
    }

    private void EndGame(GameResult gameResult)
    {
        _isGameEnd = true;
        _cameraScroller.enabled = false;
        CurrentGameResult.Value = gameResult;

        Debug.Log($"Game Ended with result: {gameResult}");
        //Stop Camera / Stop Player Movement etc.

        ShowResultClientRpc(gameResult);
    }

    [ClientRpc]
    private void ShowResultClientRpc(GameResult gameResult)
    {
        Debug.Log($"Game Result on Client: {gameResult}");
    }
}
