using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChunkInformation
{
    public int jumpDeaths = 0;
    public int enemiesDeaths = 0;
    public int goombaDeaths = 0;
    public int shellDeaths = 0;
    public int flyingShellDeaths = 0;
    public int fireBarDeaths = 0;
    public int timeCompleted;

    public int totalCoinsInChunk = 0;
    public int totalCoinsCollected = 0;

    public int difficultyScore = 0;

    public int index = 0;

    public float averageVelocity = 0;

    public bool completedChunk = false;
    public bool outOfTime = false;

    public List<TrainingType> tranningTypes;

    public int GetTotalDeaths()
    {
        return jumpDeaths + enemiesDeaths + fireBarDeaths;
    }
}

public class PlayerModel
{
    private const int _maxPlatforms = 2;

    private SerializeData serializeData;

    private int _precentageEnemyDeaths;
    private int _precentageJumpDeaths;
    private int _precentageFireBarDeaths;
    private int _precentageElevation;

    public int currentDifficultyScore = 20;

    private bool setupCompleted = false;

    public ChunkInformation chunkInformation = new ChunkInformation();

    public Dictionary<int, ChunkInformation> _previousChunkStats = new Dictionary<int, ChunkInformation>();
    public TrainingType lastTranningTypeFailure;

    private PCGEventManager eventManager;

    public PlayerModel(PCGEventManager pCGEventManager)
    {
        eventManager = pCGEventManager;
        eventManager.onDeathByEnemy += HandleDeathByEnemy;
        eventManager.onFallDeath += HandleDeathByFalling;
        eventManager.onKilledEnemy += HandleKilledEnemy;
        eventManager.onDeathByFireBar += HandleDeathByFireBar;
        eventManager.onSaveData += SaveDataToFirebase;

        serializeData = new SerializeData();

        if (serializeData.CheckSafe())
        {
            _previousChunkStats = serializeData.LoadData();
            _previousChunkStats = _previousChunkStats.OrderBy(x => x.Value.difficultyScore).ToDictionary(x => x.Key, x => x.Value);
        }
    }

    ~PlayerModel()
    {
        if (eventManager == null)
        {
            return;
        }

        eventManager.onDeathByEnemy -= HandleDeathByEnemy;
        eventManager.onFallDeath -= HandleDeathByFalling;
        eventManager.onKilledEnemy -= HandleKilledEnemy;
        eventManager.onDeathByFireBar -= HandleDeathByFireBar;
        eventManager.onSaveData -= SaveDataToFirebase;
    }

    public bool HasSafe()
    {
        return _previousChunkStats.Count > 0;
    }

    public void ResetChunkInformation()
        => chunkInformation = new ChunkInformation();

    public void UpdateChunkInformation(int chunkId, int index, bool completed, int time, bool outOfTime, List<TrainingType> tranningTypes)
    {
        if (_previousChunkStats.ContainsKey(chunkId))
        {
            return;
        }
        if (_previousChunkStats.Count >= 5)
        {
            _previousChunkStats.Remove(_previousChunkStats.First().Key);
        }

        chunkInformation.timeCompleted = time;
        chunkInformation.outOfTime = outOfTime;
        chunkInformation.completedChunk = completed;
        chunkInformation.difficultyScore = currentDifficultyScore;
        chunkInformation.index = index;
        chunkInformation.tranningTypes = tranningTypes;

        _previousChunkStats.Add(chunkId, chunkInformation);
        CalculatePrecentages();

        _previousChunkStats = _previousChunkStats.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
        chunkInformation = new ChunkInformation();
    }

    public void SaveDataToFirebase()
    {
        serializeData.SaveData(_previousChunkStats);
    }

    public List<TrainingType> GetTranningTypes(List<TrainingType> previousTranningTypes)
    {
        var lastChunk = _previousChunkStats.Last();
        var completed = lastChunk.Value.completedChunk;

        if (lastChunk.Value.tranningTypes != null && previousTranningTypes.Count == 0 && setupCompleted == false)
        {
            setupCompleted = true;
            currentDifficultyScore = lastChunk.Value.difficultyScore;

            return GetTranningTypesForIncreasedDifficulty();
        }
        // Increase difficulty;
        if (completed)
        {
            currentDifficultyScore = Mathf.Clamp(currentDifficultyScore + IncreaseDifficulty(lastChunk.Value), 1, 100);

            return GetTranningTypesForIncreasedDifficulty();
        }

        // Check if chunk before that also failed in traning.
        else
        {
            if (_previousChunkStats.Count <= 1)
            {
                return previousTranningTypes;
            }

            var hasFailedPreviousChunk = lastChunk.Value.completedChunk;

            // If chunk before last one also failed and total death count of last chunk is smaller or equal to two generate same type of level.
            if (hasFailedPreviousChunk == false && lastChunk.Value.GetTotalDeaths() <= 2)
            {
                return previousTranningTypes;
            }

            currentDifficultyScore = Mathf.Clamp(currentDifficultyScore - DecreaseDifficulty(lastChunk.Value), 1, 100);

            // If all conditions above lead to this code we decrease the difficulty for the player.
            return GetTranningTypesForIncreasedDifficulty();
        }
    }

    private int IncreaseDifficulty(ChunkInformation chunk)
    {
        var increaseDifficulty = 0;
        var totalDeaths = chunk.GetTotalDeaths();

        increaseDifficulty += chunk.timeCompleted < 5 ? 5 : 0;
        increaseDifficulty += totalDeaths <= 1 ? 5 : 0;
        // 5.86 is the max speed mario can reach without using dash.
        increaseDifficulty += chunk.averageVelocity > 5.86f && totalDeaths < 3 ? 10 : 0;

        return increaseDifficulty;
    }

    private int DecreaseDifficulty(ChunkInformation chunk)
    {
        var decreaseDifficulty = 0;
        var totalDeaths = chunk.GetTotalDeaths();

        decreaseDifficulty += chunk.completedChunk ? 0 : 5;
        decreaseDifficulty += chunk.timeCompleted < 10 ? 0 : 5;
        decreaseDifficulty += totalDeaths > 5 ? 10 : 0;
        decreaseDifficulty += totalDeaths > 1 ? 5 : 0;

        return decreaseDifficulty;
    }

    private (int, TrainingType) ReturnDifficultyOfMechanic(int score)
    {
        // TODO frequencies take into account failures and take into account current difficulty level.
        int[] arr = { 0, 1, 2, 3, 4 };
        int[] freq = { _precentageElevation, _precentageEnemyDeaths, _precentageJumpDeaths, 25, currentDifficultyScore > 59 ? _precentageFireBarDeaths : 0 };

        var type = DistributionRand(arr, freq);

        return type switch
        {
            0 => (2, TrainingType.Medium_Jump),
            1 => (6, TrainingType.Enemies),
            2 => (8, TrainingType.Long_Jump),
            3 => (8, TrainingType.Platform),
            4 => (10, TrainingType.FireBar),
            _ => (0, TrainingType.None),
        };
    }

    private void HandleDeathByFalling()
    {
        chunkInformation.jumpDeaths++;
        lastTranningTypeFailure = TrainingType.Long_Jump;

        PCGEventManager.Instance.onPlayerDeath?.Invoke(chunkInformation.enemiesDeaths < 4);
        PCGEventManager.Instance.onPlayerModelUpdated(_previousChunkStats.Keys.Max());
    }

    private void HandleDeathByFireBar()
    {
        chunkInformation.fireBarDeaths++;
        lastTranningTypeFailure = TrainingType.FireBar;

        PCGEventManager.Instance.onPlayerDeath?.Invoke(chunkInformation.enemiesDeaths < 4);
        PCGEventManager.Instance.onPlayerModelUpdated(_previousChunkStats.Keys.Max());
    }

    private void HandleDeathByEnemy(Enemytype type)
    {
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

        lastTranningTypeFailure = TrainingType.Enemies;

        PCGEventManager.Instance.onPlayerDeath?.Invoke(chunkInformation.enemiesDeaths < 4);
        PCGEventManager.Instance.onPlayerModelUpdated(_previousChunkStats.Keys.Max());
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

    private void CalculatePrecentages()
    {
        var totalEnemyDeaths =_previousChunkStats.Sum(x => x.Value.enemiesDeaths);
        var totalJumpDeaths = _previousChunkStats.Sum(x => x.Value.jumpDeaths);
        var totalFireBarDeaths = _previousChunkStats.Sum(x => x.Value.fireBarDeaths);

        var total = totalEnemyDeaths + totalJumpDeaths + totalFireBarDeaths;

        var precentageEnemyDeaths = totalEnemyDeaths != 0 ? Mathf.Clamp(totalEnemyDeaths / total * 100, 20, 90) : 20;
        var precentageJumpDeaths = totalJumpDeaths != 0 ? Mathf.Clamp(totalJumpDeaths / total * 100, 20, 90) : 20;
        var precentageFireBarDeaths = totalFireBarDeaths != 0 ? Mathf.Clamp(totalFireBarDeaths / total * 100, 5, 40) : 5;

        _precentageEnemyDeaths = precentageEnemyDeaths;
        _precentageJumpDeaths = precentageJumpDeaths;
        _precentageFireBarDeaths = precentageFireBarDeaths;
        _precentageElevation = 10;
    }

    private List<TrainingType> GetTranningTypesForIncreasedDifficulty()
    {
        var current = 0;
        var completed = false;
        List<TrainingType> tranningTypes = new List<TrainingType>();

        while (completed == false)
        {
            var difference = currentDifficultyScore - current;

            if (difference < 0)
            {
                completed = true;
                return tranningTypes;
            }

            var type = ReturnDifficultyOfMechanic(difference);

            if (type.Item2 == TrainingType.Enemies)
            {
                var count = tranningTypes.Count(x => x == TrainingType.Enemies);
                current += count;
            }

            tranningTypes.Add(type.Item2);
            current += type.Item1;
        }


        return tranningTypes;
    }

    // The main function that returns a random number
    // from arr[] according to distribution array 
    // defined by freq[]. n is size of arrays. 
    static int DistributionRand(int[] arr, int[] freq)
    {
        int[] prefix = new int[freq.Sum()];
        var index = 0;

        for (int x = 0; x < freq.Length; x++)
        {
            var frequency = freq[x];

            for (int y = 0; y < frequency; y++)
            {
                prefix[index + y] = arr[x];
            }

            index += frequency;
        }

        return prefix[(UnityEngine.Random.Range(0, prefix.Length - 1))];
    }
}