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

    private List<TrainingType> _tranningTypes = new List<TrainingType>();
    private List<TrainingType> _failedTrainingTypes = new List<TrainingType>();

    private float _timer;
    private float _globalTimer;
    private float _chunkTimer;
    private bool _outOfTime = false;
    private bool _buttonPressed = false;
    private bool _completeTutorial;

    private void Awake()
    {
        _index = (int)TrainingType.BasicsTest;
        _PCGEventManager = PCGEventManager.Instance;
        _playerModel = new PlayerModel();
        _chunkTimer = 30;

        if (_playerModel.HasSafe())
        {
            var lastCunk = _playerModel._previousChunkStats.Last();
            var tranningTypes = tranningModelHandler.Get();

            _index = Mathf.Clamp(lastCunk.Value.index, 1, tranningTypes.skillParameters.Count - 1);
        }

        _tranningTypes = new List<TrainingType> { TrainingType.Walking };

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
        _globalTimer += Time.deltaTime;

        if (_timer >= _chunkTimer && _outOfTime == false)
        {
            EndOfTimerReached();
        }

        if(_timer >= 30)
        {
            GetHelpText((TrainingType)_index);
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
            GetHelpText((TrainingType)_index);
        }
    }

    public int GetDifficulty()
        => _playerModel.currentDifficultyScore;

    public List<TrainingType> GetTranningTypes()
        => _tranningTypes;

    private void GetHelpText(TrainingType tranningType)
    {
        return;

        switch (tranningType)
        {
            case TrainingType.None:
                break;
            case TrainingType.Walking:
                _PCGEventManager.onShowMessage?.Invoke("Use the arrow keys to move");
                break;
            case TrainingType.Short_Jump:
                _PCGEventManager.onShowMessage?.Invoke("Use the  arrows keys + Z key To jump");
                break;
            case TrainingType.Medium_Jump:
                _PCGEventManager.onShowMessage?.Invoke("Holding the Z key a bit longer to jump higher");
                break;
            case TrainingType.Enemies:
                _PCGEventManager.onShowMessage?.Invoke("Jumping on top or over the enemies by using the Z key");
                break;
            case TrainingType.Long_Jump:
                _PCGEventManager.onShowMessage?.Invoke("Holding the Z key a bit longer to jump higher");
                break;
            case TrainingType.Platform:
                _PCGEventManager.onShowMessage?.Invoke("Holding the Z key a bit longer to jump higher");
                break;
            case TrainingType.FireBar:
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
    private void CheckEndOfChunk(int chunkId, int coins, bool isCoolDownChunk, List<TrainingType> chunkTranningTypes)
    {
        var playerSucces = false;
        var currentTranningType = (TrainingType)_index;

        if (isCoolDownChunk == false && _outOfTime == false)
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
        else if (isCoolDownChunk == true)
        {
            playerSucces = true;
        }

        if (playerSucces == false && isCoolDownChunk == false)
        {
            _failedIndex = _index;
            _failedTrainingTypes = _tranningTypes;
        }
        
        if (isCoolDownChunk == false)
        {
            _playerModel.UpdateChunkInformation(chunkId, _index, coins, playerSucces, (int)_timer, _outOfTime, _tranningTypes);

            _tranningTypes.Clear();
            _tranningTypes = GenerateTranningType(_failedTrainingTypes, playerSucces);
        }
        else
        {
            tranningModelHandler.model.SetTranningType((int)TrainingType.Medium_Jump);
        }

        tranningModelHandler.GenerateModelsBasedOnSkill();
        _levelGenerator.ReachedEndOfChunk(chunkId, _tranningTypes);

        if (isCoolDownChunk == false)
        {
            _PCGEventManager.onShowMessage?.Invoke(playerSucces ? "Well done increasing difficulty!" : GetTipText(_playerModel.lastTranningTypeFailure));
        }

        _PCGEventManager.onTranningGoalsGenerated?.Invoke(_tranningTypes, _playerModel.currentDifficultyScore);

        SetTimer(30);
    }

    private string GetTipText(TrainingType tranningType)
    {
        switch (tranningType)
        {
            case TrainingType.Enemies:

                return "Try jumping on top of the enemies!";
            case TrainingType.Long_Jump:
                return "Try jumping timing your jumps a bit better!";
            case TrainingType.Platform:
                return "Try jumping timing your jumps a bit better!";
            case TrainingType.FireBar:
                return "Try jumping timing your jumps based on the position of the fire bar!";
        }

        return "Please try again!";
    }

    /// <summary>
    /// Generate tranning types
    /// </summary>
    /// <param name="currentTypes">Previous chunk tranning types.</param>
    /// <returns></returns>
    private List<TrainingType> GenerateTranningType(List<TrainingType> previousFailedTraningTypes, bool succes)
    {
        // var testTypes = new List<TrainingType> { TrainingType.Platform, TrainingType.Platform, TrainingType.Medium_Jump, TrainingType.Medium_Jump, TrainingType.Medium_Jump };
        // tranningModelHandler.model.SetAdaptiveTranningType(testTypes, _playerModel.currentDifficultyScore);
        // _tranningTypes = testTypes;

        // return testTypes;

        var tranningTypes = tranningModelHandler.Get();

        // Has completed basic list, now work on adaptive part;
        if(_completeTutorial)
        {
            var adaptiveTypes = _playerModel.GetTranningTypes(_tranningTypes, previousFailedTraningTypes);
            _tranningTypes = adaptiveTypes;

            tranningModelHandler.model.SetAdaptiveTranningType(adaptiveTypes, _playerModel.currentDifficultyScore);

            return adaptiveTypes;
        }

        _playerModel.ResetChunkInformation();

        var types = new List<TrainingType>();

        _index = Mathf.Clamp(succes ? _index + 1 : _failedIndex, 0, tranningTypes.skillParameters.Count - 1);

        var returningType = tranningTypes.skillParameters[_index];

        foreach (var type in returningType.skillParameters)
        {
            types.Add(type.tranningType);
        }
        
        tranningModelHandler.model.SetTranningType(_index);

        _completeTutorial = _index + 1 >= tranningTypes.skillParameters.Count;

        if (_completeTutorial)
        {
            _playerModel.SetTutorialCompletion((int)_globalTimer);
        }

        return types;
    }

    private bool DidCompleteTranningType(TrainingType type)
    {
        switch (type)
        {
            case TrainingType.None:
                return true;
            case TrainingType.Walking:
                return DidCompleteWalkingTranning();
            case TrainingType.Short_Jump:
                return DidCompleteJumpTranning();
            case TrainingType.Medium_Jump:
                return DidCompleteJumpTranning();
            case TrainingType.Enemies:
                return DidCompleteEnemies();
            case TrainingType.Long_Jump:
                return DidCompleteJumpTranning();
            case TrainingType.Platform:
                return DidCompletePlatformTranning();
            case TrainingType.BasicsTest:
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
        var currentTranningType = (TrainingType)_index;

        tranningModelHandler.GenerateModelsBasedOnSkill();
        _PCGEventManager.onRegenerateChunk?.Invoke(chunkId);
        _playerModel.ResetChunkInformation();
        _PCGEventManager.onTranningGoalsGenerated?.Invoke(_tranningTypes, _playerModel.currentDifficultyScore);
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