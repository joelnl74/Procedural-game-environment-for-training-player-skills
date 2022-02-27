using System.Collections.Generic;
using System.Linq;

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

    public int difficultyScore = 0;

    public ChunkInformation chunkInformation = new ChunkInformation();

    private Dictionary<int, ChunkInformation> _previousChunkStats = new Dictionary<int, ChunkInformation>();

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

        _previousChunkStats.Add(chunkId, chunkInformation);
        chunkInformation = new ChunkInformation();
    }

    public int ReturnDifficultyOfMechanic(TranningType type)
    {
        switch (type)
        {
            case TranningType.None:
                return 0;
            case TranningType.Walking:
                return 0;
            case TranningType.Short_Jump:
                return 1;
            case TranningType.Medium_Jump:
                return 2;
            case TranningType.Enemies:
                return 4;
            case TranningType.Platform:
                return 6;
            case TranningType.Long_Jump:
                return 8;
            case TranningType.FireBar:
                return 8;
            default:
                return 0;
        }
    }

    public List<TranningType> GetTranningTypes(List<TranningType> previousTranningTypes)
    {
        var lastChunk = _previousChunkStats.Last();
        var completed = lastChunk.Value.completedChunk;

        // Increase difficulty;
        if (completed)
        {
            return GetTranningTypesForIncreasedDifficulty(previousTranningTypes);
        }
        // Check if chunk before that also failed in traning.
        else
        {
            if (_previousChunkStats.Count <= 1)
            {
                return previousTranningTypes;
            }

            var hasFailedPreviousChunk = _previousChunkStats[_previousChunkStats.Count - 2].completedChunk;

            // If chunk before last one also failed and total death count of last chunk is smaller or equal to two generate same type of level.
            if (hasFailedPreviousChunk == false && lastChunk.Value.deathCount <= 2)
            {
                return previousTranningTypes;
            }

            // If all conditions above lead to this code we decrease the difficulty for the player.
            return GetTranningTypesForDecreasedDifficulty(previousTranningTypes);
        }
    }

    private List<TranningType> GetTranningTypesForIncreasedDifficulty(List<TranningType> previousTranningTypes)
    {
        return new List<TranningType>();
    }

    private List<TranningType> GetTranningTypesForDecreasedDifficulty(List<TranningType> previousTranningTypes)
    {
        return new List<TranningType>();
    }
}