using System.Collections.Generic;

public class ChunkInformation
{
    public int deathCount = 0;
    public int jumpDeaths = 0;
    public int enemiesDeaths = 0;
    public int goombaDeaths = 0;
    public int shellDeaths = 0;
    public int flyingShellDeaths = 0;
    public int fireBarDeaths = 0;
    public bool completedChunk = false;
}

public class PlayerModel
{
    public int deathTotalCount = 0;
    public int jumpTotalDeaths = 0;
    public int enemiesTotalDeaths = 0;
    public int totalFireBarDeaths = 0;

    public ChunkInformation chunkInformation = new ChunkInformation();

    private Dictionary<int, ChunkInformation> _chunkStats = new Dictionary<int, ChunkInformation>();

    public PlayerModel()
    {
        var eventManager = PCGEventManager.Instance;

        eventManager.onDeathByEnemy += HandleDeathByEnemy;
        eventManager.onFallDeath += HandleDeathByFalling;
        eventManager.onKilledEnemy += HandleKilledEnemy;
    }

    private void HandleDeathByFalling()
    {
        deathTotalCount++;
        chunkInformation.deathCount++;
        jumpTotalDeaths++;
    }

    private void HandleDeathByFireBar()
    {
        deathTotalCount++;
        chunkInformation.fireBarDeaths++;
        totalFireBarDeaths++;
    }

    private void HandleDeathByEnemy(Enemytype type)
    {
        deathTotalCount++;
        enemiesTotalDeaths++;

        chunkInformation.deathCount++;
        chunkInformation.enemiesDeaths++;

        switch (type)
        {
            case Enemytype.Goomba:
                chunkInformation.goombaDeaths++;
                break;
            case Enemytype.Shell:
                chunkInformation.shellDeaths++;
                break;
            case Enemytype.FlyingShell:
                chunkInformation.flyingShellDeaths++;
                break;
        }
    }
    private void HandleKilledEnemy(Enemytype type)
    {
        switch (type)
        {
            case Enemytype.Goomba:
                break;
            case Enemytype.Shell:
                break;
            case Enemytype.FlyingShell:
                break;
        }
    }

    public void UpdateChunkInformation(int chunkId, bool completed)
    {
        chunkInformation.completedChunk = completed;

        _chunkStats.Add(chunkId, chunkInformation);
        chunkInformation = new ChunkInformation();
    }

    public int ReturnDifficulty()
    {
        return 50;
    }
}