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
    public Action<int, List<TranningType>> onReachedEndOfChunk;
    public Action<Enemytype> onDeathByEnemy;
    public Action<Enemytype> onKilledEnemy;
    public Action onFallDeath;
}
