using UnityEngine;

public class PlayerStateManager : MonoBehaviour
{
    public PlayerState currentState = PlayerState.Default;

    public bool IsDefault()
    {
        return currentState == PlayerState.Default;
    }

    public bool IsMenu()
    {
        return currentState == PlayerState.Menu;
    }

    public bool IsCutscene()
    {
        return currentState == PlayerState.Cutscene;
    }

    public void SetState(PlayerState newState)
    {
        currentState = newState;
    }
}