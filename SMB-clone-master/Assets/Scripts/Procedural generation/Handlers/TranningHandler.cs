using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TranningHandler : MonoBehaviour
{
    [SerializeField] private LevelGenerator _levelGenerator;
    
    private TranningModelHandler tranningModelHandler;
    private PCGEventManager _PCGEventManager;

    private List<TranningType> _currentTranningType;
    private List<TranningType> tranningTypes;

    private float timer;
    private bool outOfTime = false;
    private bool buttonPressed;

    public int deathCount = 0;
    public int jumpDeaths = 0;
    public int enemiesDeaths = 0;

    private void Awake()
    {
        _currentTranningType = new List<TranningType> { TranningType.Walking };
        _PCGEventManager = PCGEventManager.Instance;
        tranningModelHandler = new TranningModelHandler();
        tranningTypes = _currentTranningType;

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

        // TODO make this boolean based to make it faster.
        if (_currentTranningType.Contains(TranningType.Walking))
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
  
        if (playerSucces && isCoolDownChunk == false)
        {
            tranningTypes = GenerateTranningType(_currentTranningType);
            _currentTranningType = tranningTypes;

            tranningModelHandler.model.SetTranningType(_currentTranningType);
        }
        else
        {
            tranningModelHandler.model.SetTranningType(new List<TranningType> { TranningType.Short_Jump});
        }

        tranningModelHandler.GenerateModelsBasedOnSkill();
        _levelGenerator.ReachedEndOfChunk(chunkId, tranningTypes);

        SetTimer(30);
        ClearChunkStats();
    }

    private List<TranningType> GenerateTranningType(List<TranningType> currentTypes)
    {
        var types = new List<TranningType>();
        var lastTranningType = currentTypes.Max();
        var newTranningType = lastTranningType += 1;

        if(lastTranningType != TranningType.BasicsTest)
        {
            newTranningType = lastTranningType++;
        }

        types.Add(newTranningType);

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

    private bool DidCompletePlatformTranning()
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