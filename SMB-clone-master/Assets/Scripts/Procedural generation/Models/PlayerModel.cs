using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    private const int _maxPlatforms = 2;

    private int _precentageEnemyDeaths;
    private int _precentageJumpDeaths;
    private int _precentageFireBarDeaths;

    public int currentDifficultyScore = 50;

    public ChunkInformation chunkInformation = new ChunkInformation();

    private Dictionary<int, ChunkInformation> _previousChunkStats = new Dictionary<int, ChunkInformation>();

    public PlayerModel(PCGEventManager eventManager)
    {
        eventManager.onDeathByEnemy += HandleDeathByEnemy;
        eventManager.onFallDeath += HandleDeathByFalling;
        eventManager.onKilledEnemy += HandleKilledEnemy;
        eventManager.onDeathByFireBar += HandleDeathByFireBar;
    }

    private void HandleDeathByFalling()
    {
        chunkInformation.deathCount++;
    }

    private void HandleDeathByFireBar()
    {
        chunkInformation.fireBarDeaths++;
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
        if(_previousChunkStats.Count >= 5)
        {
            _previousChunkStats.Remove(_previousChunkStats.First().Key);
        }

        chunkInformation.completedChunk = completed;

        _previousChunkStats.Add(chunkId, chunkInformation);
        CalculatePrecentages();

        chunkInformation = new ChunkInformation();
    }

    public (int, TranningType) ReturnDifficultyOfMechanic(int score)
    {
        // TODO frequencies take into account failures and take into account current difficulty level.
        int[] arr = {0, 1, 2, 3, 4};
        int[] freq = { 5, _precentageEnemyDeaths, _precentageJumpDeaths, 15, currentDifficultyScore > 59 ? _precentageFireBarDeaths : 0};

        var type = myRand(arr, freq);

        switch (type)
        {
            case 0:
                return (2, TranningType.Medium_Jump);
            case 1:
                return (6, TranningType.Enemies);
            case 2:
                return (8, TranningType.Long_Jump);
            case 3:
                return (8, TranningType.Platform);
            case 4:
                return (10, TranningType.FireBar);
            default:
                return (0, TranningType.None);
        }
    }

    public List<TranningType> GetTranningTypes(List<TranningType> previousTranningTypes)
    {
        var lastChunk = _previousChunkStats.Last();
        var completed = lastChunk.Value.completedChunk;

        // Increase difficulty;
        if (completed)
        {
            currentDifficultyScore += 5;

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

            currentDifficultyScore -= 5;

            // If all conditions above lead to this code we decrease the difficulty for the player.
            return GetTranningTypesForIncreasedDifficulty(previousTranningTypes); //GetTranningTypesForDecreasedDifficulty(previousTranningTypes);
        }
    }

    private void CalculatePrecentages()
    {
        var totalEnemyDeaths =_previousChunkStats.Sum(x => x.Value.enemiesDeaths);
        var totalJumpDeaths = _previousChunkStats.Sum(x => x.Value.jumpDeaths);
        var totalFireBarDeaths = _previousChunkStats.Sum(x => x.Value.fireBarDeaths);

        var total = totalEnemyDeaths + totalJumpDeaths + totalFireBarDeaths;

        var precentageEnemyDeaths = totalEnemyDeaths != 0 ? totalEnemyDeaths / total * 100 : 50;
        var precentageJumpDeaths = totalJumpDeaths != 0 ? totalJumpDeaths / total * 100 : 20;
        var precentageFireBarDeaths = totalFireBarDeaths != 0 ? totalFireBarDeaths / total * 100 : 30;

        _precentageEnemyDeaths = precentageEnemyDeaths;
        _precentageJumpDeaths = precentageJumpDeaths;
        _precentageFireBarDeaths = precentageFireBarDeaths;
    }

    private List<TranningType> GetTranningTypesForIncreasedDifficulty(List<TranningType> previousTranningTypes)
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

        return prefix[(Random.Range(0, prefix.Length - 1))];
    }
}