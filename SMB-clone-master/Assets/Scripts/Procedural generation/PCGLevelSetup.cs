using UnityEngine;

public class PCGLevelSetup : MonoBehaviour
{
    private GameStateManager stateManager;

    private void Awake()
    {
        stateManager = FindObjectOfType<GameStateManager>();

        stateManager.lives = 1000;
        stateManager.timeLeft = 300;
    }
}
