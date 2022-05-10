using System;
using System.Collections.Generic;

public enum Enemytype
{
    Goomba,
    Shell,
    FlyingShell
}

public class PCGEventManager : MonoSingleton<PCGEventManager>
{
    public Action<int, int, bool, List<TrainingType>> onReachedEndOfChunk;
    public Action<Enemytype> onDeathByEnemy;
    public Action<Enemytype> onKilledEnemy;
    public Action onFallDeath;
    public Action onDeathByFireBar;
    public Action onCollectedCoin;
    public Action<List<TrainingType>, int> onTranningGoalsGenerated;
    public Action<int> onRegenerateChunk;
    public Action<int> onPlayerModelUpdated;
    public Action<bool> onPlayerDeath;

    public Action<string> onShowMessage;
    public Action<int> onRegenerateLevelSelected;
    public Action<int> onShowRegenerateLevelMessage;

    public Action onSaveData;
}
