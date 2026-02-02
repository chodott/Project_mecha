using Unity.VisualScripting;
using UnityEngine;

public static class EventBus
{
    public static System.Action<GamePhase> OnChangedGamePhase;
    public static System.Action<GameResultArgs> OnGameEnded;
}
