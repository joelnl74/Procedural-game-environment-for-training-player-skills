using System.Collections.Generic;
using UnityEngine;

public class TranningHandler : MonoBehaviour
{
    [SerializeField] private LevelGenerator _levelGenerator;
    
    private TranningModelHandler tranningModelHandler;
    private PCGEventManager _PCGEventManager;
    private TranningType _currentTranningType;

    private List<TranningType> tranningTypes;

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

    private void CheckEndOfChunk(int chunkId, List<TranningType> chunkTranningTypes)
    {
        if (deathCount == 0 
            && chunkTranningTypes.Contains(_currentTranningType)
            && _currentTranningType != TranningType.BasicsTest)
        {
            _currentTranningType += 1;
            tranningModelHandler.model.SetTranningType(_currentTranningType);
        }

        tranningTypes = new List<TranningType> { _currentTranningType };

        tranningModelHandler.GenerateModelsBasedOnSkill();
        _levelGenerator.ReachedEndOfChunk(chunkId, tranningTypes);

        ClearChunkStats();
    }
}
