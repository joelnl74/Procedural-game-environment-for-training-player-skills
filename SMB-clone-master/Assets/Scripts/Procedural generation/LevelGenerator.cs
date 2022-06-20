using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private int _maxHeigth;
    [SerializeField] private int _maxLearningWidth;
    [SerializeField] private int _maxCooldownWidth;
    [SerializeField] private int _chunksToGenerate;

    [SerializeField] private GameObject _spawnPoints;
    [SerializeField] private GameObject _endOfChunk;

    // Over world sprites
    [SerializeField] private GameObject _groundBlockOver;
    [SerializeField] private GameObject _goomba;
    [SerializeField] private GameObject _shell;
    [SerializeField] private GameObject _flyingShell;
    [SerializeField] private GameObject _fireBar;
    [SerializeField] private GameObject _piranhaPlant;

    // Underworld sprites
    [SerializeField] private GameObject _groundBlockUnder;

    [SerializeField] private GameObject _coin;
    [SerializeField] private GameObject[] _specialBlocks;

    [SerializeField] private TranningModelHandler _tranningModelHandler;

    [SerializeField] private Mario _mario;
    [SerializeField] private GameObject _clouds;

    private Dictionary<int, Dictionary<int, EntityModel>> _entities;
    private Dictionary<int, GameObject> _chunks;

    private GameObject _groundBlock;
    private PCGEventManager pcgEventManager;

    private int _previousChunkWidthEnd = 0;
    private int _lastGeneratedChunk = 1;

    private int minHeigth = 3;

    private int generatedCoins = 0;

    private PlayerModelHandler tranningHandler;
    private int _maxWidth;

    public void SetupLevel(PlayerModelHandler handler)
    {
        _entities = new Dictionary<int, Dictionary<int, EntityModel>>();
        _chunks = new Dictionary<int, GameObject>();
        tranningHandler = handler;
        pcgEventManager = PCGEventManager.Instance;

        pcgEventManager.onRegenerateChunk += RegenerateChunk;
        pcgEventManager.onPlayerDeath += HandleOnDeath;

        SetupSprites(SceneManager.GetActiveScene().name == "PCG");

        var spawnPos = Instantiate(new GameObject(), _spawnPoints.transform);
        spawnPos.name = "Spawn position";

        for (int i = 0; i < _chunksToGenerate; i++)
        {
            GenerateChunk();
        }
    }

    private void OnDestroy()
    {
        if (pcgEventManager != null)
        {
            pcgEventManager.onRegenerateChunk -= RegenerateChunk;
            pcgEventManager.onPlayerDeath -= HandleOnDeath;
        }
    }

    public void SetupSprites(bool overworld)
    {
        _groundBlock = overworld == true ? _groundBlockOver : _groundBlockUnder;
        Camera.main.backgroundColor = overworld == true ? Camera.main.backgroundColor : Color.black;
        _clouds.SetActive(overworld);
    }

    public void ReachedEndOfChunk(int id, List<TrainingType> tranningTypes)
    {
        generatedCoins = 0;
        var currentId = id - 1;
        if (currentId == 0)
        {
            GenerateChunk();

            return;
        }

        if (_chunks.ContainsKey(currentId))
        {
            var chunk = _chunks[currentId];

            CleanEntitiesInChunk(currentId);
            Destroy(chunk);
            _chunks.Remove(currentId);

            GenerateChunk();
        }

        if (id % 10 == 0)
        {
            _clouds.transform.position = new Vector2((id - 1) * _maxWidth, _clouds.transform.position.y);
        }

    }

    private void Start()
    {
        _mario.EnablePhysics();
    }

    private void RegenerateChunk(int chunkId)
    {
        var chunkToRegenerate = chunkId + 2;
        var lastWidthEnd = _previousChunkWidthEnd;
        var lastChunk = _lastGeneratedChunk;

        _previousChunkWidthEnd = _previousChunkWidthEnd - (_maxWidth + _maxCooldownWidth + _maxCooldownWidth);

        if (_chunks.ContainsKey(chunkToRegenerate))
        {
            var chunk = _chunks[chunkToRegenerate];
            
            _entities.Remove(chunkToRegenerate);
            _chunks.Remove(chunkToRegenerate);

            Destroy(chunk);

            _lastGeneratedChunk = chunkToRegenerate;

            GenerateChunk();
        }

        _previousChunkWidthEnd = lastWidthEnd;
        _lastGeneratedChunk = lastChunk;
    }

    private void GenerateChunk()
    {
        _entities.Add(_lastGeneratedChunk, new Dictionary<int, EntityModel>());

        if (_lastGeneratedChunk % 2 == 0)
        {
            GenerateCooldownChunk();

            return;
        }

        SetupNewSpawnPosition(_lastGeneratedChunk - 1);

        _maxWidth = _maxLearningWidth;

        var chunk = new GameObject();
        var maxX = _previousChunkWidthEnd + _maxWidth;

        chunk.name = $"Chunk {_lastGeneratedChunk}";

        _chunks.Add(_lastGeneratedChunk, chunk);

        // TODO clean this up can be done easier with generate solid blocks.
        for (int x = _previousChunkWidthEnd; x < maxX; x++)
            for (int y = 1; y < _maxHeigth; y++)
            {
                if (y <= minHeigth)
                {
                    GenerateFloorBlock(chunk, x, y);
                }
            }

        HandleElevation(_lastGeneratedChunk);
        HandleChasm(_lastGeneratedChunk);
        HandlePlatforms(_lastGeneratedChunk);
        HandleEnemies(_lastGeneratedChunk);
        HandleFireBar(_lastGeneratedChunk);
        HandleGenerateCoins(_lastGeneratedChunk);
        FinalCheck(_lastGeneratedChunk);

        SetupEndOfChunk(chunk, maxX + 1, 0);

        _previousChunkWidthEnd += _maxWidth;
    }

    private void GenerateCooldownChunk()
    {
        var chunk = new GameObject();
        var maxX = _previousChunkWidthEnd + _maxCooldownWidth;

        _maxWidth = _maxCooldownWidth;
        chunk.name = $"Cooldown chunk {_lastGeneratedChunk}";

        _chunks.Add(_lastGeneratedChunk, chunk);

        for (int x = _previousChunkWidthEnd; x < maxX; x++)
            for (int y = 1; y < _maxHeigth; y++)
            {
                if (y <= minHeigth)
                {
                    GenerateFloorBlock(chunk, x, y);
                }
            }

        HandleElevation(_lastGeneratedChunk);
        HandleGenerateCoins(_lastGeneratedChunk);
        SetupEndOfChunk(chunk, maxX + 1, 0);

        _previousChunkWidthEnd += _maxCooldownWidth;
    }

    private Vector2Int GetEmptySpotOnMap(int chunkId, TrainingType tranningType)
    {
        var randomXPos = Random.Range(_previousChunkWidthEnd + 1, _previousChunkWidthEnd + _maxWidth - 1);
        var entities = _entities[chunkId];
        var highestYpos = -1;

        foreach(var entity in entities)
        {
            if (entity.Value.xPos == randomXPos)
            {
                var entityValue = entity.Value;
                var ypos = entityValue.yPos;

                if (ypos > highestYpos)
                {
                    if (tranningType == TrainingType.FireBar)
                    {
                        var id = GetId(entityValue.xPos - 1, entityValue.yPos, chunkId);
                        var PreviousEntity = entities.ContainsKey(id);

                        if (PreviousEntity)
                        {
                            highestYpos = ypos;
                        }
                    }
                    else
                    {
                        highestYpos = ypos;
                    }
                }
            }
        }

        return new Vector2Int(randomXPos, highestYpos + 1);
    }

    private void SetupNewSpawnPosition(int chunkId)
    {
        if(chunkId <= 0)
        {
            return;
        }

        var position = FindHighestEmptyPoint(chunkId);
        var spawnPos = Instantiate(new GameObject(), _spawnPoints.transform);

        spawnPos.transform.position = new Vector3(position.x, position.y, 0);
        _mario.respawnPositionPCG = position;

        spawnPos.name = "Spawn position";
    }

    // Generate solid floor block.
    private void GenerateFloorBlock(GameObject chunk, int x, int y)
    {
        var go = Instantiate(_groundBlock, chunk.transform);
        var pos = new Vector2(x, y);
        var entityModel = new EntityModel();

        go.name = "ground_block";
        go.transform.position = pos;

        AddEntity(_lastGeneratedChunk, new Vector2Int(x, y), entityModel, go, EntityType.Solid);
    }

    // Generate elevation in terrain.
    private void HandleElevation(int chunkId)
    {
        var minX = _previousChunkWidthEnd + 1;
        var maxX = _previousChunkWidthEnd + _maxWidth;

        foreach (var model in _tranningModelHandler.elevationModels)
        {
            var xPos = Random.Range(minX, maxX - model.width);
            var previousBlockHeigth = FindBlockHighestPosition(chunkId, xPos - 1);

            var columnHeigth = previousBlockHeigth + model.heigth;
            var increasedHeigth = Mathf.Clamp(columnHeigth, previousBlockHeigth, Mathf.Min(columnHeigth, previousBlockHeigth + 4));

            var beginposition = new Vector2Int(xPos, 1);
            var endPosition = new Vector2Int(xPos + model.width, increasedHeigth);

            var chunk = _chunks[chunkId];

            GenerateBlocks(beginposition, endPosition, chunk, false, Random.Range(1, 3) / 2 == 0);
        }
    }

    private void HandleFireBar(int chunkId)
    {
        var chunk = _chunks[chunkId];

        foreach (var model in _tranningModelHandler.fireBarModels)
        {
            var emptySpot = GetEmptySpotOnMap(chunkId, TrainingType.FireBar);

            GenerateFireBar(emptySpot.x, emptySpot.y, chunk);
        }
    }

    private void HandlePlatforms(int chunkId)
    {
        var minX = _previousChunkWidthEnd;
        var maxX = _previousChunkWidthEnd + _maxWidth;
        var chunk = _chunks[chunkId];

        foreach (var model in _tranningModelHandler.platformModels)
        {
            var xPos = Random.Range(minX, maxX - model.width);
            var previousHighestBlock = FindBlockHighestPosition(chunkId, xPos - 1);

            var yPos = previousHighestBlock <= minHeigth ? 3 : previousHighestBlock;
            yPos += model.heigth;

            var beginposition = new Vector2Int(xPos, yPos);
            var endPosition = new Vector2Int(xPos + model.width, yPos);

            GenerateBlocks(beginposition, endPosition, chunk, model.hasSpecialBlocks, false, true);

            if (model.hasEnemies)
            {
                var type = Random.Range(0, 2);

                if (type == 0)
                {
                    GenerateGoomba(chunk, endPosition);
                }
                else
                {
                    GenerateShell(chunk, endPosition);
                }
            }
            if (model.hasCoins)
            {
                var yPosition = yPos + 1;

                for (int x = beginposition.x; x < endPosition.x; x++)
                {
                    GenerateCoin(x, yPosition, chunk);
                }
            }
            if (model.chasmModel != null)
            {
                var halfLength = beginposition.x;
                var startX = halfLength;
                var endX = startX + model.chasmModel.width;

                GenerateChasmBlocks(new Vector2Int(startX, 1), new Vector2Int(endX, yPos - 1), chunkId, true);
            }
        }
    }

    private void HandleGenerateCoins(int chunkId)
    {
        var chunk = _chunks[chunkId];
        var spawnCoins = Random.Range(0, 8);

        for(int i = 0; i < spawnCoins; i++)
        {
            var emptySpot = GetEmptySpotOnMap(chunkId, TrainingType.Walking);

            GenerateCoin(emptySpot.x, emptySpot.y, chunk);
        }
    }

    private void HandleEnemies(int chunkId)
    {
        var minX = _previousChunkWidthEnd;
        var maxX = _previousChunkWidthEnd + _maxWidth;
        var chunk = _chunks[chunkId];

        foreach (var model in _tranningModelHandler.enemyModels)
        {
            var emptySpot = GetEmptySpotOnMap(chunkId, TrainingType.Enemies);
            var position = new Vector2Int(emptySpot.x, emptySpot.y);

            switch(model.enemytype)
            {
                case Enemytype.Shell:
                    GenerateShell(chunk, position);
                    break;
                case Enemytype.Goomba:
                    GenerateGoomba(chunk, position);
                    break;
                case Enemytype.FlyingShell:
                    GenerateFlyingShell(chunk, position);
                    break;
                default:
                    GenerateGoomba(chunk, position);
                    break;
            }
        }
    }

    private void HandleChasm(int chunkId)
    {
        var minX = _previousChunkWidthEnd;
        var maxX = _previousChunkWidthEnd + _maxWidth;

        foreach (var model in _tranningModelHandler.chasmModels)
        {
            var xPos = Random.Range(minX, maxX - model.width);

            var beginposition = new Vector2Int(xPos, 1);
            var endPosition = new Vector2Int(xPos + model.width, _maxHeigth);

            GenerateChasmBlocks(beginposition, endPosition, chunkId);
        }
    }

    private void GenerateCoin(int x, int y, GameObject chunk)
    {
        if (CheckId(x, y, _lastGeneratedChunk))
        {
            return;
        }

        generatedCoins++;

        var yPos = y >= minHeigth ? y : minHeigth + 1;

        var go = Instantiate(_coin, chunk.transform);
        var pos = new Vector2(x, yPos);
        var entityModel = new EntityModel();

        go.name = "coin";
        go.transform.position = pos;

        AddEntity(_lastGeneratedChunk, new Vector2Int(x, y), entityModel, go, EntityType.Coin);
    }

    private void GenerateFireBar(int x, int y, GameObject chunk)
    {
        if (CheckId(x, y, _lastGeneratedChunk))
        {
            return;
        }

        var go = Instantiate(_fireBar, chunk.transform);
        var pos = new Vector2(x, y);
        var entityModel = new EntityModel();

        go.name = "Fire Bar";
        go.transform.position = pos;

        AddEntity(_lastGeneratedChunk, new Vector2Int(x, y), entityModel, go, EntityType.FireBar);

    }

    private void GenerateBlocks(Vector2Int begin, Vector2Int end, GameObject chunk, bool hasSpecial = false, bool stayAtHeigth = false, bool isPlatform = false)
    {
        var endPointX = stayAtHeigth == false ? end.x : _previousChunkWidthEnd + _maxWidth;

        for(int x = begin.x; x <= endPointX; x++)
        {
            for(int y = begin.y; y <= end.y; y++)
            {
                if (y >= _maxHeigth)
                    break;

                // does this kill the loop?
                if (_entities[_lastGeneratedChunk].ContainsKey(GetId(x, y, _lastGeneratedChunk)) == true)
                    continue;

                GameObject go = null;
                int chance = 0;

                if (hasSpecial)
                {
                    chance = Random.Range(0, 100);
                }

                go = chance < 50
                    ? Instantiate(_groundBlock, chunk.transform)
                    : Instantiate(_specialBlocks[Random.Range(0, _specialBlocks.Length - 1)], chunk.transform);

                var pos = new Vector2(x, y);
                var entityModel = new EntityModel();

                go.name = "block";
                go.transform.position = pos;

                AddEntity(_lastGeneratedChunk, new Vector2Int(x, y), entityModel, go, EntityType.Solid);
            }
        }
    }

    private void GenerateGoomba(GameObject chunk, Vector2Int end)
    {
        var go = Instantiate(_goomba, chunk.transform);
        var pos = new Vector2Int(end.x, end.y + 1);
        var entityModel = new EntityModel();

        entityModel.SetEnemyType(Enemytype.Goomba);
        go.name = "goomba";
        go.transform.position = new Vector2(pos.x, pos.y);

        AddEntity(_lastGeneratedChunk, pos, entityModel, go, EntityType.Enemy);
    }

    private void GenerateFlyingShell(GameObject chunk, Vector2Int end)
    {
        var go = Instantiate(_flyingShell, chunk.transform);
        var pos = new Vector2Int(end.x, end.y + 1);
        var entityModel = new EntityModel();

        entityModel.SetEnemyType(Enemytype.FlyingShell);
        go.name = "Flying shell";
        go.transform.position = new Vector2(pos.x, pos.y);

        AddEntity(_lastGeneratedChunk, pos, entityModel, go, EntityType.Enemy);
    }

    private void GenerateShell(GameObject chunk, Vector2Int end)
    {
        var go = Instantiate(_shell, chunk.transform);
        var pos = new Vector2Int(end.x, end.y + 1);
        var entityModel = new EntityModel();

        entityModel.SetEnemyType(Enemytype.Shell);
        go.name = "shell";
        go.transform.position = new Vector2(pos.x, pos.y);

        AddEntity(_lastGeneratedChunk, pos, entityModel, go, EntityType.Enemy);
    }

    private Vector2Int FindHighestEmptyPoint(int chunkId)
    {
        var chunk = _entities[chunkId];
        var yPos = -1;
        var Xpos = -1;

        foreach(var block in chunk)
        {
            if(block.Value.yPos > yPos)
            {
                var entity = block.Value;

                yPos = entity.yPos;
                Xpos = entity.xPos;
            }
        }

        return new Vector2Int(Xpos, yPos + 1);
    }

    private void GenerateChasmBlocks(Vector2Int begin, Vector2Int end, int chunkId, bool ignoreCheck = false)
    {
        var beginY = FindBlockHighestPosition(chunkId, begin.x - 1);
        var endY = FindBlockHighestPosition(chunkId, end.x);

        for (int x = begin.x; x < end.x; x++)
        {
            if (ignoreCheck == false)
            {
                var CanRemove = CheckBlockInfront(x, chunkId);

                if (CanRemove == false)
                {
                    var blockHighestPosition = FindBlockHighestPosition(chunkId, x - 1);

                    if (blockHighestPosition > beginY)
                    {
                        LowerBlocks(x - 1, beginY, beginY, chunkId);
                    }

                    break;
                }
            }

            for (int y = begin.y; y < end.y; y++)
            {
                var index = GetId(x, y, chunkId);
                var block = GetEntity(x, y, chunkId);

                if (block != null)
                {
                    if(block.gameObject != null)
                    {
                        Destroy(block.gameObject);
                    }

                    _entities[chunkId].Remove(index);
                }
            }
        }

        if (endY > beginY)
        {
            LowerBlocks(end.x, beginY, end.y + 1, chunkId);
        }
    }

    private void LowerBlocks(int x, int maxY, int CurY, int chunkId)
    {
        for (int y = CurY; y > maxY; y--)
        {
            var block = GetEntity(x, y, chunkId);

            if (block != null)
            {
                if (block.gameObject != null)
                {
                    Destroy(block.gameObject);
                }

                _entities[chunkId].Remove(GetId(x, y, chunkId));
            }
        }
    }

    private void FinalCheck(int chunkId)
    {
        var chunk = _entities[chunkId];

        for (int x = _previousChunkWidthEnd + 1; x < _previousChunkWidthEnd + _maxWidth; x++)
        {
            var PreviousY = FindBlockHighestPosition(chunkId, x - 1);
            var posY = FindBlockHighestPosition(chunkId, x);
            var nextPos = FindBlockHighestPosition(chunkId, x + 1);

            if (posY - PreviousY > 4)
            {
                LowerBlocks(x, PreviousY + 4, posY, chunkId);
            }

            if (nextPos == 0 && PreviousY != 0)
            {
                LowerBlocks(x, PreviousY, posY, chunkId);
            }
        }
    }

    private int FindBlockHighestPosition(int chunkId, int x)
    {
        int highestPositon = 0;

        var chunk = _entities[chunkId];
        var xPos = x;

        foreach (var block in chunk)
        {
            var value = block.Value;

            if (value.xPos == xPos && value.yPos > highestPositon)
            {
                highestPositon = block.Value.yPos;
            }
        }

        return highestPositon;
    }

    private bool CheckBlockInfront(int start, int chunkId)
    {
        var min = false;
        var max = false;

        for(int x = start - 3; x < start; x++)
        {
            var beginTest = GetEntity(x, 1, chunkId);

            if(beginTest != null)
            {
                min = true;
                
                break;
            }
        }
        for (int x = start + 3; x > start; x--)
        {
            var endTest = GetEntity(x, 1, chunkId);

            if (endTest != null)
            {
                max = true;

                break;
            }
        }

        return min && max;
    }

    private EntityModel GetEntity(int x, int y, int chunk)
    {
        var key = GetId(x, y, chunk);
        var contains = _entities[chunk].ContainsKey(key);

        if (contains)
            return _entities[chunk][key];

        return null;
    }

    private void AddEntity(int chunkId, Vector2Int position, EntityModel entityModel, GameObject go, EntityType entityType)
    {
        var id = GetId(position.x, position.y, chunkId);

        entityModel.Setup(id, position.x, position.y, go, entityType);
        _entities[chunkId].Add(id, entityModel);
    }

    private int GetId(int x, int y, int chunkId)
        => _maxWidth * chunkId * x + y ;

    private void CleanEntitiesInChunk(int chunkId)
        => _entities.Remove(chunkId);

    private bool CheckId(int x, int y, int chunkId)
    {
        var id = GetId(x, y, chunkId);

        return _entities[chunkId].ContainsKey(id);
    }

    private void HandleOnDeath(bool respawn)
    {
        if (respawn == false)
        {
            return;
        }

        for (var i = 0; i < _entities.Count; i++)
        {
            var entitiesInChunk = _entities.ElementAt(i).Value;
            var chunkGo = _chunks.ElementAt(i).Value;

            foreach (var model in entitiesInChunk.Values)
            {
                if (model.entityType == EntityType.Enemy)
                {
                    if (model.gameObject == null)
                    {
                        GameObject go = null;

                        switch (model.enemytype)
                        {
                            case Enemytype.Goomba:
                                go = Instantiate(_goomba, chunkGo.transform);
                                go.name = "Goomba";
                                break;
                            case Enemytype.Shell:
                                go = Instantiate(_shell, chunkGo.transform);
                                go.name = "Shell";
                                break;
                            case Enemytype.FlyingShell:
                                go = Instantiate(_flyingShell, chunkGo.transform);
                                go.name = "Flying-shell";
                                break;
                        }

                        model.gameObject = go;
                    }

                    model.gameObject.transform.position = new Vector2(model.xPos, model.yPos);
                }
            }
        }
    }

    private void SetupEndOfChunk(GameObject chunkGo, int x, int y)
    {
        var goEnd = Instantiate(_endOfChunk, chunkGo.transform);
        var component = goEnd.GetComponent<EndOfChunkCollider>();

        goEnd.transform.position = new Vector2(x, y);
        goEnd.name = "End of chunk";

        component.Setup(_lastGeneratedChunk, generatedCoins, _lastGeneratedChunk % 2 == 0, tranningHandler.GetTranningTypes());

        _lastGeneratedChunk++;
    }
}