using System.Collections.Generic;
using UnityEngine;

public class TranningHandler : MonoBehaviour
{
    [SerializeField] private LevelGenerator _levelGenerator;
    
    private TranningModelHandler tranningModelHandler;
    private PCGEventManager _PCGEventManager;
    private TranningType _currentTranningType;

    private List<TranningType> tranningTypes;

    private float timer;
    private bool outOfTime = false;
    private bool buttonPressed;

    public int deathCount = 0;
    public int jumpDeaths = 0;
    public int enemiesDeaths = 0;

    private void Awake()
    {
        _currentTranningType = TranningType.Walking;
        _PCGEventManager = PCGEventManager.Instance;
        tranningModelHandler = new TranningModelHandler();
        tranningTypes = new List<TranningType> { _currentTranningType };

        tranningModelHandler.model.SetTranningType(_currentTranningType);
        tranningModelHandler.GenerateModelsBasedOnSkill();
        _levelGenerator.SetupLevel(this);

        _PCGEventManager.onReachedEndOfChunk += CheckEndOfChunk;
    }

    private void Start()
    {
        _PCGEventManager.onFallDeath += HandleDeathByFalling;
        _PCGEventManager.onDeathByEnemy += HandleDeathByEnemy;
        _PCGEventManager.onKilledEnemy += HandleKilledEnemy;
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
        if (timer < 0 && outOfTime == false)
        {
            EndOfTimerReached();
        }

        if (buttonPressed)
        {
            return;
        }

        if (_currentTranningType == TranningType.Walking)
        {
            if (outOfTime)
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
        outOfTime = true;
    }

    private void SetTimer(float time)
    {
        timer = time;
        outOfTime = false;
    }

    private void CheckEndOfChunk(int chunkId, List<TranningType> chunkTranningTypes)
    {
        var playerSucces = false;
        var containsTranningType = chunkTranningTypes.Contains(_currentTranningType);

        if (_currentTranningType == TranningType.BasicsTest)
        {
            containsTranningType = false;
        }

        if (containsTranningType)
        {
            foreach (var type in tranningTypes)
            {
                playerSucces = DidCompleteTranningType(type);

                if (playerSucces == false)
                {
                    break;
                }
            }
        }
  
        if (playerSucces)
        {
            _currentTranningType += 1;
        }

        tranningModelHandler.model.SetTranningType(_currentTranningType);
        tranningTypes = new List<TranningType> { _currentTranningType };

        tranningModelHandler.GenerateModelsBasedOnSkill();
        _levelGenerator.ReachedEndOfChunk(chunkId, tranningTypes);

        SetTimer(30);
        ClearChunkStats();
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
                break;
            case TranningType.BasicsTest:
                break;
        }

        return false;
    }

    private bool DidCompleteWalkingTranning()
    {
        if (outOfTime)
        {
            return false;
        }

        return true;
    }

    private bool DidCompleteJumpTranning()
    {
        if (jumpDeaths > 2 || outOfTime)
        {
            return false;
        }

        return true;
    }

    private bool DidCompleteEnemies()
    {
        if (enemiesDeaths > 2 && outOfTime == false)
        {
            return false;
        }

        return true;
    }
}