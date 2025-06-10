using UnityEngine;

public enum GameState
{
    Idle,
    Animating,
}
public class GameManager : MonoBehaviourSingletonPersistent<GameManager>
{
    public GameState gameState = GameState.Idle;
}
