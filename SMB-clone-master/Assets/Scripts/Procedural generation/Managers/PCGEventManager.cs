using System;
using System.Collections.Generic;

public class PCGEventManager : MonoSingleton<PCGEventManager>
{
    public Action<int, List<TranningType>> onReachedEndOfChunk;
}
