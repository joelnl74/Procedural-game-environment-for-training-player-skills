using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class ChunkInformation
{
    public int deathCount = 0;
    public int jumpDeaths = 0;
    public int enemiesDeaths = 0;
    public int goombaDeaths = 0;
    public int shellDeaths = 0;
    public int flyingShellDeaths = 0;
    public int fireBarDeaths = 0;

    public int difficultyScore = 0;

    public int index = 0;

    public bool completedChunk = false;

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

    public int currentDifficultyScore = 25;

    public ChunkInformation chunkInformation = new ChunkInformation();

    public Dictionary<int, ChunkInformation> _previousChunkStats = new Dictionary<int, ChunkInformation>();
    public TranningType lastTranningTypeFailure;

    public PlayerModel(PCGEventManager eventManager)
    {
        eventManager.onDeathByEnemy += HandleDeathByEnemy;
        eventManager.onFallDeath += HandleDeathByFalling;
        eventManager.onKilledEnemy += HandleKilledEnemy;
        eventManager.onDeathByFireBar += HandleDeathByFireBar;

        serializeData = new SerializeData();

        if (serializeData.CheckSafe())
        {
            _previousChunkStats = serializeData.LoadData();
        }
    }

    public bool HasSafe()
    {
        return _previousChunkStats.Count > 0;
    }

    public void ResetChunkInformation()
        => chunkInformation = new ChunkInformation();

    public void UpdateChunkInformation(int chunkId, int index, bool completed)
    {
        if (_previousChunkStats.ContainsKey(chunkId))
        {
            return;
        }
        if (_previousChunkStats.Count >= 5)
        {
            _previousChunkStats.Remove(_previousChunkStats.First().Key);
        }

        chunkInformation.completedChunk = completed;
        chunkInformation.difficultyScore = currentDifficultyScore;
        chunkInformation.index = index;

        _previousChunkStats.Add(chunkId, chunkInformation);
        CalculatePrecentages();

        serializeData.SaveData(_previousChunkStats);

        chunkInformation = new ChunkInformation();
    }

    public List<TranningType> GetTranningTypes(List<TranningType> previousTranningTypes)
    {
        var lastChunk = _previousChunkStats.Last();
        var completed = lastChunk.Value.completedChunk;

        // Increase difficulty;
        if (completed)
        {
            currentDifficultyScore += 5;

            return GetTranningTypesForIncreasedDifficulty();
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

            currentDifficultyScore -= 5;

            // If all conditions above lead to this code we decrease the difficulty for the player.
            return GetTranningTypesForIncreasedDifficulty();
        }
    }

    private (int, TranningType) ReturnDifficultyOfMechanic(int score)
    {
        // TODO frequencies take into account failures and take into account current difficulty level.
        int[] arr = { 0, 1, 2, 3, 4 };
        int[] freq = { _precentageElevation, _precentageEnemyDeaths, _precentageJumpDeaths, 15, currentDifficultyScore > 59 ? _precentageFireBarDeaths : 0 };

        var type = myRand(arr, freq);

        return type switch
        {
            0 => (2, TranningType.Medium_Jump),
            1 => (6, TranningType.Enemies),
            2 => (8, TranningType.Long_Jump),
            3 => (8, TranningType.Platform),
            4 => (10, TranningType.FireBar),
            _ => (0, TranningType.None),
        };
    }

    private void HandleDeathByFalling()
    {
        chunkInformation.jumpDeaths++;
        lastTranningTypeFailure = TranningType.Long_Jump;

        PCGEventManager.Instance.onPlayerModelUpdated(_previousChunkStats.Keys.Max());
    }

    private void HandleDeathByFireBar()
    {
        chunkInformation.fireBarDeaths++;
        lastTranningTypeFailure = TranningType.FireBar;

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

        lastTranningTypeFailure = TranningType.Enemies;

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

        // TODO add clamps.
        var precentageEnemyDeaths = totalEnemyDeaths != 0 ? Mathf.Clamp(totalEnemyDeaths / total * 100, 10, 50) : 50;
        var precentageJumpDeaths = totalJumpDeaths != 0 ? Mathf.Clamp(totalJumpDeaths / total * 100, 10, 30) : 20;
        var precentageFireBarDeaths = totalFireBarDeaths != 0 ? Mathf.Clamp(totalFireBarDeaths / total * 100, 10, 35) : 30;

        _precentageEnemyDeaths = precentageEnemyDeaths;
        _precentageJumpDeaths = precentageJumpDeaths;
        _precentageFireBarDeaths = precentageFireBarDeaths;
        _precentageElevation = 10;
    }

    private List<TranningType> GetTranningTypesForIncreasedDifficulty()
    {
        var current = 0;
        var completed = false;
        List<TranningType> tranningTypes = new List<TranningType>();

        while(completed == false)
        {
            var difference = currentDifficultyScore - current;

            if (difference < 0)
            {
                completed = true;
                return tranningTypes;
            }

            var type = ReturnDifficultyOfMechanic(difference);

            tranningTypes.Add(type.Item2);
            current += type.Item1;
        }


        return tranningTypes;
    }

    private List<TranningType> GetTranningTypesForDecreasedDifficulty(List<TranningType> previousTranningTypes)
    {
        return new List<TranningType>();
    }

    // The main function that returns a random number
    // from arr[] according to distribution array 
    // defined by freq[]. n is size of arrays. 
    static int myRand(int[] arr, int[] freq)
    {
        int[] prefix = new int[freq.Sum()];
        var index = 0;

        for (int x = 0; x < freq.Length; x++)
        {
            var frequency = freq[x];

            for(int y = 0; y < frequency; y++)
            {
                prefix[index + y] = arr[x];
            }

            index += frequency;
        }

        return prefix[(UnityEngine.Random.Range(0, prefix.Length - 1))];
    }
}