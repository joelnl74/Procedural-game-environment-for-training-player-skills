using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
    private float _chunkTimer;
    private bool _outOfTime = false;
    private bool _buttonPressed = false;

    private void Awake()
    {
        _index = (int)TranningType.Walking;
        _PCGEventManager = PCGEventManager.Instance;
        _playerModel = new PlayerModel(_PCGEventManager);
        _chunkTimer = 30;

        if (_playerModel.HasSafe())
        {
            var lastCunk = _playerModel._previousChunkStats.Last();
            var tranningTypes = tranningModelHandler.Get();

            _index = Mathf.Clamp(lastCunk.Value.index, 1, tranningTypes.skillParameters.Count - 1);
        }

        _tranningTypes = new List<TranningType> { TranningType.Walking };

        _PCGEventManager.onReachedEndOfChunk += CheckEndOfChunk;
        _PCGEventManager.onPlayerModelUpdated += HandlePlayerModelUpdated;
        _PCGEventManager.onRegenerateLevelSelected += HandleRegenerateLevel;
    }

    private void OnDestroy()
    {
        if (_PCGEventManager != null)
        {
            _PCGEventManager.onReachedEndOfChunk -= CheckEndOfChunk;
            _PCGEventManager.onPlayerModelUpdated -= HandlePlayerModelUpdated;
            _PCGEventManager.onRegenerateLevelSelected -= HandleRegenerateLevel;
        }
    }

    private void Start()
    {
        tranningModelHandler.model.SetTranningType(1);
        tranningModelHandler.GenerateModelsBasedOnSkill();
        _levelGenerator.SetupLevel(this);
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= _chunkTimer && _outOfTime == false)
        {
            EndOfTimerReached();
        }

        if (_buttonPressed)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow)
            || Input.GetKeyDown(KeyCode.LeftArrow)
            || Input.GetKeyDown(KeyCode.Z))
        {
            _buttonPressed = true;
        }

        if (_timer > 8 && _buttonPressed == false)
        {
            GetHelpText((TranningType)_index);
        }
    }

    public int GetDifficulty()
        => _playerModel.currentDifficultyScore;

    public List<TranningType> GetTranningTypes()
        => _tranningTypes;

    private void GetHelpText(TranningType tranningType)
    {
        switch (tranningType)
        {
            case TranningType.None:
                break;
            case TranningType.Walking:
                _PCGEventManager.onShowMessage?.Invoke("Use the arrow keys to move");
                break;
            case TranningType.Short_Jump:
                _PCGEventManager.onShowMessage?.Invoke("Use the  arrows keys + Z key To jump");
                break;
            case TranningType.Medium_Jump:
                _PCGEventManager.onShowMessage?.Invoke("Holding the Z key a bit longer to jump higher");
                break;
            case TranningType.Enemies:
                _PCGEventManager.onShowMessage?.Invoke("Jumping on top or over the enemies by using the Z key");
                break;
            case TranningType.Long_Jump:
                _PCGEventManager.onShowMessage?.Invoke("Holding the Z key a bit longer to jump higher");
                break;
            case TranningType.Platform:
                _PCGEventManager.onShowMessage?.Invoke("Holding the Z key a bit longer to jump higher");
                break;
            case TranningType.FireBar:
                _PCGEventManager.onShowMessage?.Invoke("Holding the Z key a bit longer to jump higher");
                break;
            default:
                _PCGEventManager.onShowMessage?.Invoke("Use the arrows keys to move and Z to jump");
                break;
        }
    }

    private void EndOfTimerReached()
    {
        _outOfTime = true;
    }

    private void SetTimer(float time)
    {
        _buttonPressed = false;
        _timer = 0;
        _chunkTimer = time;
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

        if (isCoolDownChunk == false)
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
            _playerModel.UpdateChunkInformation(chunkId, _index, playerSucces, (int)_timer, _outOfTime);

            _tranningTypes.Clear();
            _tranningTypes = GenerateTranningType(currentTranningType, playerSucces);
        }
        else
        {
            tranningModelHandler.model.SetTranningType((int)TranningType.Medium_Jump);
        }

        tranningModelHandler.GenerateModelsBasedOnSkill();
        _levelGenerator.ReachedEndOfChunk(chunkId, _tranningTypes);

        _PCGEventManager.onShowMessage?.Invoke(playerSucces ? "Well done increasing difficulty!" : GetTipText(_playerModel.lastTranningTypeFailure));
        _PCGEventManager.onTranningGoalsGenerated?.Invoke(_tranningTypes);

        SetTimer(30);
    }

    private string GetTipText(TranningType tranningType)
    {
        switch (tranningType)
        {
            case TranningType.Enemies:

                return "Try jumping on top of the enemies!";
            case TranningType.Long_Jump:
                return "Try jumping timing your jumps a bit better!";
            case TranningType.Platform:
                return "Try jumping timing your jumps a bit better!";
            case TranningType.FireBar:
                return "Try jumping timing your jumps based on the position of the fire bar!";
        }

        return "Please try again!";
    }

    /// <summary>
    /// Generate tranning types
    /// </summary>
    /// <param name="currentTypes">Previous chunk tranning types.</param>
    /// <returns></returns>
    private List<TranningType> GenerateTranningType(TranningType previousTranningTypes, bool succes)
    {
        var tranningTypes = tranningModelHandler.Get();

        // Has completed basic list, now work on adaptive part;
        if(_index + 1 >= tranningTypes.skillParameters.Count)
        {
            var adaptiveTypes = _playerModel.GetTranningTypes(_tranningTypes);

            tranningModelHandler.model.SetAdaptiveTranningType(adaptiveTypes, _playerModel.currentDifficultyScore);

            return adaptiveTypes;
        }

        var types = new List<TranningType>();

        _index = Mathf.Clamp(succes ? _index + 1 : _index - 1, 0, tranningTypes.skillParameters.Count - 1);

        var returningType = tranningTypes.skillParameters[_index];

        foreach (var type in returningType.skillParameters)
        {
            types.Add(type.tranningType);
        }
        
        tranningModelHandler.model.SetTranningType(_index);

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
                return DidCompletePlatformTranning();
            case TranningType.BasicsTest:
                break;
        }

        return true;
    }

    private void HandlePlayerModelUpdated(int chunkId)
    {
        var total = _playerModel.chunkInformation.GetTotalDeaths();

        if (total > 5)
        {
            _PCGEventManager.onShowRegenerateLevelMessage?.Invoke(chunkId);
        }
    }

    private void HandleRegenerateLevel(int chunkId)
    {
        _tranningTypes.Clear();

        var currentTranningType = (TranningType)_index;

        _tranningTypes = GenerateTranningType(currentTranningType, false);

        tranningModelHandler.GenerateModelsBasedOnSkill();
        _PCGEventManager.onRegenerateChunk?.Invoke(chunkId);
        _playerModel.ResetChunkInformation();
        _PCGEventManager.onTranningGoalsGenerated?.Invoke(_tranningTypes);
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
        if (_playerModel.chunkInformation.jumpDeaths > 2 || _outOfTime)
        {
            return false;
        }

        return true;
    }

    private bool DidCompletePlatformTranning()
    {
        if (_playerModel.chunkInformation.jumpDeaths > 2 || _outOfTime)
        {
            return false;
        }

        return true;
    }

    private bool DidCompleteEnemies()
    {
        if (_playerModel.chunkInformation.enemiesDeaths > 2 || _outOfTime)
        {
            return false;
        }

        return true;
    }
}