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
    public Action<int, bool, List<TranningType>> onReachedEndOfChunk;
    public Action<Enemytype> onDeathByEnemy;
    public Action<Enemytype> onKilledEnemy;
    public Action onFallDeath;
    public Action onDeathByFireBar;
    public Action<List<TranningType>> onTranningGoalsGenerated;
    public Action<int> onRegenerateChunk;
    public Action<int> onPlayerModelUpdated;

    public Action<string> onShowMessage;

}
