using System.Collections.Generic;
using UnityEngine;

public class PlayerModelHandler : MonoBehaviour
{
    [SerializeField] private LevelGenerator _levelGenerator;
    [SerializeField] private TranningModelHandler tranningModelHandler;

    private PCGEventManager _PCGEventManager;
    private PlayerModel _playerModel;

    private int _index;
    private int _failedIndex;

    private List<TranningType> _tranningTypes;

    private float _timer;
    private bool _outOfTime = false;
    private bool _buttonPressed;
    private bool _tranningChunkSucces;

    private int _chunkJumpDeaths = 0;
    private int _chunkEnemiesDeaths = 0;

    private void Awake()
    {
        _playerModel = new PlayerModel();
        _index = (int)TranningType.Walking;
        _PCGEventManager = PCGEventManager.Instance;
        _tranningTypes = new List<TranningType> { TranningType.Walking};

        _PCGEventManager.onReachedEndOfChunk += CheckEndOfChunk;
    }

    private void Start()
    {
        tranningModelHandler.model.SetTranningType(_index);
        tranningModelHandler.GenerateModelsBasedOnSkill(_tranningTypes);
        _levelGenerator.SetupLevel(this);
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
        if (_tranningTypes.Contains(TranningType.Walking))
        {
            if (_outOfTime)
            {
                // TODO show something that makes it clear the player has to move.
            }
        }
    }

    public List<TranningType> GetTranningTypes()
        => _tranningTypes;

    private void ClearChunkStats()
    {
        _chunkJumpDeaths = 0;
        _chunkEnemiesDeaths = 0;
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
        var currentTranningType = (TranningType)_index;

        if (currentTranningType != TranningType.BasicsTest && isCoolDownChunk == false)
        {
            foreach (var tranningType in chunkTranningTypes)
            {
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
            _failedIndex = _index; ;
            _tranningChunkSucces = playerSucces;
            _playerModel.UpdateChunkInformation(chunkId, playerSucces);
        }
  
        if (_tranningChunkSucces && isCoolDownChunk == false)
        {
            _tranningTypes.Clear();
            _tranningTypes = GenerateTranningType(currentTranningType);

            tranningModelHandler.model.SetTranningType(_index);
        }
        else if(_tranningChunkSucces == false && isCoolDownChunk == false)
        {
            _tranningTypes.Clear();
            tranningModelHandler.model.SetTranningType(_index);
        }
        else
        {
            tranningModelHandler.model.SetTranningType((int)TranningType.Short_Jump);
        }

        tranningModelHandler.GenerateModelsBasedOnSkill(_tranningTypes);
        _levelGenerator.ReachedEndOfChunk(chunkId, _tranningTypes);

        _PCGEventManager.onTranningGoalsGenerated?.Invoke(_tranningTypes);

        SetTimer(30);
        ClearChunkStats();
    }

    /// <summary>
    /// Generate tranning types
    /// </summary>
    /// <param name="currentTypes">Previous chunk tranning types.</param>
    /// <returns></returns>
    private List<TranningType> GenerateTranningType(TranningType previousTranningTypes)
    {
        var types = new List<TranningType>();
        var tranningTypes = tranningModelHandler.Get();

        // Has completed basic list, now work on adaptive part;
        if(_index + 1 > tranningTypes.skillParameters.Count)
        {
            return GenerateAdaptiveTranningType(previousTranningTypes);
        }

        _index = Mathf.Clamp(_index + 1, 0, tranningTypes.skillParameters.Count - 1);

        var returningType = tranningTypes.skillParameters[_index];

        foreach (var type in returningType.skillParameters)
        {
            types.Add(type.tranningType);
        }

        return types;
    }

    private List<TranningType> GenerateAdaptiveTranningType(TranningType previousTranningTypes)
    {
        return new List<TranningType>();
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
                return DidCompletePlatformTranning();
            case TranningType.BasicsTest:
                break;
        }

        return true;
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
        if (_chunkJumpDeaths > 2 || _outOfTime)
        {
            return false;
        }

        return true;
    }

    private bool DidCompletePlatformTranning()
    {
        if (_chunkJumpDeaths > 2 || _outOfTime)
        {
            return false;
        }

        return true;
    }

    private bool DidCompleteEnemies()
    {
        if (_chunkEnemiesDeaths > 2 && _outOfTime == false)
        {
            return false;
        }

        return true;
    }
}