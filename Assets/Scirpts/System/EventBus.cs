using Unity.VisualScripting;
using UnityEngine;

public static class EventBus
{
    public static System.Action<GameStatus> OnChangedGameStatus;
}
