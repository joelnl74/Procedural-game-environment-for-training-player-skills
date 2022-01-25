using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TranningHandler : MonoBehaviour
{
    public int deathCount = 0;
    public int jumpDeaths = 0;
    public int enemiesDeaths = 0;

    [SerializeField] private LevelGenerator _levelGenerator;
    [SerializeField] private TranningModelHandler tranningModelHandler;

    private PCGEventManager _PCGEventManager;

    private List<TranningType> _currentTranningType;
    private List<TranningType> _failedTranningType;
    private List<TranningType> tranningTypes;

    private float _timer;
    private bool _outOfTime = false;
    private bool _buttonPressed;
    private bool _tranningChunkSucces;

    private void Awake()
    {
        _currentTranningType = new List<TranningType> { TranningType.Walking };
        _PCGEventManager = PCGEventManager.Instance;
        tranningTypes = _currentTranningType;

        _PCGEventManager.onReachedEndOfChunk += CheckEndOfChunk;
    }

    private void Start()
    {
        _PCGEventManager.onFallDeath += HandleDeathByFalling;
        _PCGEventManager.onDeathByEnemy += HandleDeathByEnemy;
        _PCGEventManager.onKilledEnemy += HandleKilledEnemy;

        tranningModelHandler.model.SetTranningType(_currentTranningType);
        tranningModelHandler.GenerateModelsBasedOnSkill();
        _levelGenerator.SetupLevel(this);
    }

    private void OnDestroy()
    {
        if (_PCGEventManager == null)
        {
            return;
        }

        _PCGEventManager.onFallDeath -= HandleDeathByFalling;
        _PCGEventManager.onDeathByEnemy -= HandleDeathByEnemy;
        _PCGEventManager.onKilledEnemy -= HandleKilledEnemy;
    }

    private void Update()
    {
        if (_timer < 0 && _outOfTime == false)
        {
            EndOfTimerReached();
        }

        if (_buttonPressed)
        {
            return;
        }

        // TODO make this boolean based to make it faster.
        if (_currentTranningType.Contains(TranningType.Walking))
        {
            if (_outOfTime)
            {
                // TODO show something that makes it clear the player has to move.
            }
        }
        
    }

    public List<TranningType> GetTranningTypes()
        => tranningTypes;

    public TranningModel GetTranningModel()
        => tranningModelHandler.model;

    public List<ElevationModel> GetElevationModels()
        => tranningModelHandler.elevationModels;

    public List<ChasmModel> GetChasmModels()
        => tranningModelHandler.chasmModels;

    public List<PlatformModel> GetPlatformModels()
        => tranningModelHandler.platformModels;

    public List<EnemyModel> GetEnemyModels()
    => tranningModelHandler.enemyModels;

    private void HandleDeathByFalling()
    {
        jumpDeaths++;
        deathCount++;
    }

    private void HandleDeathByEnemy(Enemytype obj)
    {
        enemiesDeaths++;
        deathCount++;
    }

    private void HandleKilledEnemy(Enemytype obj)
    {
    }

    private void ClearChunkStats()
    {
        deathCount = 0;
        enemiesDeaths = 0;
        jumpDeaths = 0;
    }

    private void EndOfTimerReached()
    {
        _outOfTime = true;
    }

    private void SetTimer(float time)
    {
        _timer = time;
        _outOfTime = false;
    }

    /// <summary>
    /// Check if the tranning was complete and if we should generate new tranning goals.
    /// </summary>
    /// <param name="chunkId">Chunk id.</param>
    /// <param name="isCoolDownChunk">Cooldown chunk.</param>
    /// <param name="chunkTranningTypes">Tranning types in the chunk.</param>
    private void CheckEndOfChunk(int chunkId, bool isCoolDownChunk, List<TranningType> chunkTranningTypes)
    {
        var playerSucces = false;

        if (_currentTranningType.Contains(TranningType.BasicsTest) == false && isCoolDownChunk == false)
        {
            foreach (var tranningType in chunkTranningTypes)
            {
                if(_currentTranningType.Contains(tranningType) == false)
                {
                    break;
                }

                playerSucces = DidCompleteTranningType(tranningType);

                if (playerSucces == false)
                {
                    break;
                }
            }
        }
        else
        {
            playerSucces = true;
        }
        
        if (isCoolDownChunk == false)
        {
            _failedTranningType = _currentTranningType;
            _tranningChunkSucces = playerSucces;
        }
  
        if (_tranningChunkSucces && isCoolDownChunk == false)
        {
            tranningTypes = GenerateTranningType(_currentTranningType);
            _currentTranningType = tranningTypes;

            tranningModelHandler.model.SetTranningType(_currentTranningType);
        }
        else if(_tranningChunkSucces == false && isCoolDownChunk == false)
        {
            tranningModelHandler.model.SetTranningType(_failedTranningType);
        }
        else
        {
            tranningModelHandler.model.SetTranningType(new List<TranningType> { TranningType.Short_Jump });
        }

        tranningModelHandler.GenerateModelsBasedOnSkill();
        _levelGenerator.ReachedEndOfChunk(chunkId, tranningTypes);

        SetTimer(30);
        ClearChunkStats();
    }

    /// <summary>
    /// Generate tranning types
    /// </summary>
    /// <param name="currentTypes">Previous chunk tranning types.</param>
    /// <returns></returns>
    private List<TranningType> GenerateTranningType(List<TranningType> previousTranningTypes)
    {
        var types = new List<TranningType>();
        var lastTranningType = previousTranningTypes.Max();

        if (lastTranningType != TranningType.BasicsTest)
        {
            var newTranningType = lastTranningType += 1;

            newTranningType = lastTranningType++;

            if (previousTranningTypes.Count == 1)
            {
                for(int i = previousTranningTypes.Count; i <= 0; i++)
                {
                    types.Add((TranningType)i);
                }
            }

            types.Add(newTranningType);
        }
        else
        {
            types.Add(lastTranningType);
        }


        return types;
    }

    private bool DidCompleteTranningType(TranningType type)
    {
        switch (type)
        {
            case TranningType.None:
                return true;
            case TranningType.Walking:
                return DidCompleteWalkingTranning();
            case TranningType.Short_Jump:
                return DidCompleteJumpTranning();
            case TranningType.Medium_Jump:
                return DidCompleteJumpTranning();
            case TranningType.Enemies:
                return DidCompleteEnemies();
            case TranningType.Long_Jump:
                return DidCompleteJumpTranning();
            case TranningType.Platform:
                return DidCompletePlatformTranning(); // TODO add something that check you have reached a platform.
            case TranningType.BasicsTest:
                break;
        }

        return false;
    }

    private bool DidCompleteWalkingTranning()
    {
        if (_outOfTime)
        {
            return false;
        }

        return true;
    }

    private bool DidCompleteJumpTranning()
    {
        if (jumpDeaths > 2 || _outOfTime)
        {
            return false;
        }

        return true;
    }

    private bool DidCompletePlatformTranning()
    {
        if (jumpDeaths > 2 || _outOfTime)
        {
            return false;
        }

        return true;
    }

    private bool DidCompleteEnemies()
    {
        if (enemiesDeaths > 2 && _outOfTime == false)
        {
            return false;
        }

        return true;
    }
}