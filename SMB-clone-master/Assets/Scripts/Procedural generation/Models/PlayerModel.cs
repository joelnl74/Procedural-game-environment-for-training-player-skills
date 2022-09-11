using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChunkInformation
{
    public int jumpDeaths = 0;
    public int enemiesDeaths = 0;
    public int goombaDeaths = 0;
    public int shellDeaths = 0;
    public int flyingShellDeaths = 0;
    public int fireBarDeaths = 0;
    public int timeCompleted;

    public int totalCoinsAvailable = 0;
    public int totalCoinsCollected = 0;

    public int difficultyScore = 0;

    public int index = 0;

    public float averageVelocity = 0;

    public bool completedChunk = true;
    public bool outOfTime = false;

    public List<TrainingType> tranningTypes;

    public int GetTotalDeaths()
    {
        return jumpDeaths + enemiesDeaths + fireBarDeaths;
    }
}

public class GlobalPlayerResults
{
    public int HighestScoreVersionOne;
    public int HighestScoreVersionTwo;

    public int jumpDeathsVersionOne;
    public int jumpDeathsVersionTwo;

    public int EnemyDeathsVersionOne;
    public int EnemyDeathsVersionTwo;

    public int totalDeathsVersionOne;
    public int totalDeathsVersionTwo;

    public int fireBarDeathsVersionOne;
    public int fireBarDeathsVersionTwo;

    public int timeCompletionIntroductionVersionOne;
    public int timeCompletionIntroductionVersionTwo;

    public bool DidRegenerateLevelVersionOne;
    public bool DidRegenerateLevelVersionTwo;
    public bool DidFailTraingVersionOne;
    public bool DidFailTrainingVersionTwo;

    public bool DidSpeedRun;
    public bool DidCollectCoins;

    public void UpdateModel(ChunkInformation chunkInformation, int version)
    {
        if (version == 1)
        {
            jumpDeathsVersionOne += chunkInformation.jumpDeaths;
            EnemyDeathsVersionOne += chunkInformation.enemiesDeaths;
            fireBarDeathsVersionOne += chunkInformation.fireBarDeaths;
            totalDeathsVersionOne += chunkInformation.GetTotalDeaths();

            if (chunkInformation.difficultyScore > HighestScoreVersionOne)
            {
                HighestScoreVersionOne = chunkInformation.difficultyScore;
            }

            return;
        }

        jumpDeathsVersionTwo += chunkInformation.jumpDeaths;
        EnemyDeathsVersionTwo += chunkInformation.enemiesDeaths;
        fireBarDeathsVersionTwo += chunkInformation.fireBarDeaths;
        totalDeathsVersionTwo += chunkInformation.GetTotalDeaths();

        if (chunkInformation.difficultyScore > HighestScoreVersionTwo)
        {
            HighestScoreVersionTwo = chunkInformation.difficultyScore;
        }
    }
}

public class PlayerModel
{
    private const int _maxPlatforms = 1;
    private const int _maxFireBars = 3;

    private SerializeData serializeData;

    private int _precentageEnemyDeaths;
    private int _precentageJumpDeaths;
    private int _precentageFireBarDeaths;
    private int _precentageElevation;
    private int _precentagePlatform;

    public int currentDifficultyScore = 1;

    private bool setupCompleted = false;

    public ChunkInformation chunkInformation = new ChunkInformation();
    public GlobalPlayerResults _globalPlayerResults;

    public Dictionary<int, ChunkInformation> _previousChunkStats = new Dictionary<int, ChunkInformation>();
    public TrainingType lastTranningTypeFailure;

    private PCGEventManager eventManager;
    private int version;
    private bool completedTutorial;

    public PlayerModel()
    {
        eventManager = PCGEventManager.Instance;
        eventManager.onDeathByEnemy += HandleDeathByEnemy;
        eventManager.onFallDeath += HandleDeathByFalling;
        eventManager.onKilledEnemy += HandleKilledEnemy;
        eventManager.onDeathByFireBar += HandleDeathByFireBar;
        eventManager.onSaveData += SaveDataToFirebase;
        eventManager.onCollectedCoin += HandleOnCollectedCoin;

        serializeData = new SerializeData();
        _globalPlayerResults = FirebaseManager.Instance.GetGlobalResults();

        var scene = SceneManager.GetActiveScene();
        version = scene.name == "PCG" ? 1 : 2;

        if (serializeData.CheckSafe(version))
        {
            _previousChunkStats = serializeData.LoadData(version);

            if (_previousChunkStats == null)
            {
                _previousChunkStats = new Dictionary<int, ChunkInformation>();

                return;
            }

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
        eventManager.onCollectedCoin -= HandleOnCollectedCoin;
    }

    public bool HasSafe()
    {
        return _previousChunkStats.Count > 0;
    }

    private void HandleOnCollectedCoin()
    {
        chunkInformation.totalCoinsCollected++;
    }

    public void ResetChunkInformation()
    {
        _globalPlayerResults.UpdateModel(chunkInformation, version);

        if (version == 1)
        {
            _globalPlayerResults.DidRegenerateLevelVersionOne = true;
        }
        else
        {
            _globalPlayerResults.DidRegenerateLevelVersionTwo = true;
        }

        chunkInformation = new ChunkInformation();
    }

    public void UpdateChunkInformation(int chunkId, int index, int coins, bool completed, int time, bool outOfTime, List<TrainingType> tranningTypes)
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
        chunkInformation.totalCoinsAvailable = coins;
        chunkInformation.tranningTypes = tranningTypes;

        if (chunkInformation.totalCoinsAvailable == chunkInformation.totalCoinsCollected)
        {
            _globalPlayerResults.DidCollectCoins = true;
        }

        if (chunkInformation.completedChunk == false)
        {
            if (version == 1)
            {
                _globalPlayerResults.DidFailTraingVersionOne = true;
            }
            else
            {
                _globalPlayerResults.DidFailTrainingVersionTwo = true;
            }
        }

        _globalPlayerResults.UpdateModel(chunkInformation, version);

        _previousChunkStats.Add(chunkId, chunkInformation);
        CalculatePrecentages();

        _previousChunkStats = _previousChunkStats.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
        chunkInformation = new ChunkInformation();
    }

    public void SaveDataToFirebase()
    {
        var scene = SceneManager.GetActiveScene();
        int version = scene.name == "PCG" ? 1 : 2;

        if (_previousChunkStats.ContainsKey(int.MaxValue))
        {
            _previousChunkStats.Remove(int.MaxValue);
        }

        FirebaseManager.Instance.SetGlobalResults(_globalPlayerResults);
        serializeData.SaveData(_previousChunkStats, _globalPlayerResults, version);
    }

    public void SetTutorialCompletion(int time)
    {
        if (completedTutorial == false)
        {
            completedTutorial = true;

            if (version == 1)
            {
                _globalPlayerResults.timeCompletionIntroductionVersionOne = time;

                return;
            }

            _globalPlayerResults.timeCompletionIntroductionVersionTwo = time;
        }
    }

    public List<TrainingType> GetTranningTypes(List<TrainingType> previousTranningTypes, List<TrainingType> previousFailedTraningTypes)
    {
        var lastChunk = _previousChunkStats.Last();
        var completed = lastChunk.Value.completedChunk;

        var totalDeaths = lastChunk.Value.GetTotalDeaths();

        // Increase difficulty;
        if (totalDeaths < 3)
        {
            currentDifficultyScore = Mathf.Clamp(currentDifficultyScore + IncreaseDifficulty(lastChunk.Value), 1, 100);

            return GetTranningTypesForTargetedDifficulty();
        }
        else
        {
            currentDifficultyScore = Mathf.Clamp(currentDifficultyScore - DecreaseDifficulty(lastChunk.Value), 1, 100);

            // If all conditions above lead to this code we decrease the difficulty for the player.
            return GetTranningTypesForTargetedDifficulty();
        }
    }

    private int IncreaseDifficulty(ChunkInformation chunk)
    {
        return Random.Range(1, 5);
    }

    private int DecreaseDifficulty(ChunkInformation chunk)
    {
        return Random.Range(1 ,5);
    }

    private (int, TrainingType) ReturnDifficultyOfMechanic(int score)
    {
        // TODO frequencies take into account failures and take into account current difficulty level.
        int[] arr = { 0, 1, 2, 3, 4, 5};
        int[] freq = { _precentageElevation, _precentageEnemyDeaths, _precentageJumpDeaths, _precentagePlatform, currentDifficultyScore >= 30 ? _precentageFireBarDeaths : 0, 50 };

        var type = DistributionRand(arr, freq);

        return type switch
        {
            0 => (2, TrainingType.Medium_Jump),
            1 => (4, TrainingType.Enemies),
            2 => (6, TrainingType.Long_Jump),
            3 => (6, TrainingType.Platform),
            4 => (8, TrainingType.FireBar),
            5 => (1, TrainingType.Walking),
            _ => (0, TrainingType.None),
        };
    }

    private void HandleDeathByFalling()
    {
        chunkInformation.jumpDeaths++;
        lastTranningTypeFailure = TrainingType.Long_Jump;

        PCGEventManager.Instance.onPlayerDeath?.Invoke(true);
        PCGEventManager.Instance.onPlayerModelUpdated(_previousChunkStats.Keys.Max());
    }

    private void HandleDeathByFireBar()
    {
        chunkInformation.fireBarDeaths++;
        lastTranningTypeFailure = TrainingType.FireBar;

        PCGEventManager.Instance.onPlayerDeath?.Invoke(true);
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

        PCGEventManager.Instance.onPlayerDeath?.Invoke(true);
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
        var precentageEnemyDeaths = Random.Range(30, 50);
        var precentageJumpDeaths = Random.Range(30, 50);
        var precentageFireBarDeaths = Random.Range(30, 50);

        _precentagePlatform = Random.Range(15, 30);
        _precentageEnemyDeaths = precentageEnemyDeaths;
        _precentageJumpDeaths = precentageJumpDeaths;
        _precentageFireBarDeaths = precentageFireBarDeaths;
        _precentageElevation = Random.Range(10, 15);
    }

    private List<TrainingType> GetTranningTypesForTargetedDifficulty()
    {
        var current = 0;
        var completed = false;
        var currentAmountOfPlatforms = 0;
        var currentAmountOfFireBars = 0;
        List<TrainingType> tranningTypes = new List<TrainingType>();

        while (completed == false)
        {
            var difference = currentDifficultyScore - current;

            if (difference <= 0)
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

            if (type.Item2 == TrainingType.Platform)
            {
                currentAmountOfPlatforms++;
            }

            if (type.Item2 == TrainingType.FireBar)
            {
                currentAmountOfFireBars++;
            }

            if (currentAmountOfPlatforms >= _maxPlatforms)
            {
                _precentagePlatform = 0;
                _precentageEnemyDeaths += 5;
            }

            if (currentAmountOfFireBars >= _maxFireBars)
            {
                _precentageFireBarDeaths = 0;
                _precentageEnemyDeaths += 5;
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