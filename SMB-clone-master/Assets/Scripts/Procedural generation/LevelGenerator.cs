using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private int _maxHeigth;
    [SerializeField] private int _maxWidth;
    [SerializeField] private int _chunksToGenerate;

    [SerializeField] private GameObject _spawnPoints;
    [SerializeField] private GameObject _endOfChunk;
    [SerializeField] private GameObject _groundBlock;
    [SerializeField] private GameObject _goomba;
    [SerializeField] private GameObject _coin;

    [SerializeField] private GameObject[] _specialBlocks;

    [SerializeField] private Mario _mario;
    [SerializeField] private GameObject _clouds;

    private Dictionary<int, Dictionary<int, EntityModel>> _entities;
    private Dictionary<int, GameObject> _chunks;

    private int _previousChunkWidthEnd = 0;
    private int _lastGeneratedChunk = 1;

    private int minHeigth = 1;

    private TranningHandler tranningHandler;

    public void SetupLevel(TranningHandler handler)
    {
        _entities = new Dictionary<int, Dictionary<int, EntityModel>>();
        _chunks = new Dictionary<int, GameObject>();
        tranningHandler = handler;

        for (int i = 0; i < _chunksToGenerate; i++)
        {
            GenerateChunk();
        }
    }

    public void ReachedEndOfChunk(int id, List<TranningType> tranningTypes)
    {
        if (_chunks.ContainsKey(id - 1))
        {
            var currentId = id - 1;
            var chunk = _chunks[currentId];
            _mario.respawnPositionPCG = new Vector2(id * _maxWidth + 1, 3);

            CleanEntitiesInChunk(currentId);
            Destroy(chunk);
            _chunks.Remove(currentId);
            GenerateChunk();
        }

        if (id % 10 == 0)
        {
            _clouds.transform.position = new Vector2((id -1) * _maxWidth, _clouds.transform.position.y);
        }
    }

    private void Start()
    {
        _mario.EnablePhysics();
    }

    private void GenerateChunk()
    {
        var chunk = new GameObject();
        var maxX = _previousChunkWidthEnd + _maxWidth;

        chunk.name = $"Chunk {_lastGeneratedChunk}";

        _chunks.Add(_lastGeneratedChunk, chunk);

        for (int x = _previousChunkWidthEnd; x < maxX; x++)
            for (int y = minHeigth; y < _maxHeigth; y++)
            {
                if (x == 0 && y == 2)
                {
                    var spawnPos = Instantiate(new GameObject(), _spawnPoints.transform);
                    spawnPos.name = "Spawn position";
                }
                if (y == minHeigth)
                {
                    var go = Instantiate(_groundBlock, chunk.transform);
                    var pos = new Vector2(x, y);
                    var component = go.AddComponent<EntityModel>();

                    go.name = "block";
                    go.transform.position = pos;

                    AddEntity(_lastGeneratedChunk, new Vector2Int(x, y), component, EntityType.Solid);
                }
            }

        HandleElevation(_lastGeneratedChunk);
        HandleChasm(_lastGeneratedChunk);
        HandlePlatforms(_lastGeneratedChunk);
        HandleEnemies(_lastGeneratedChunk);

        SetupEndOfChunk(chunk, maxX + 1, 0);

        _previousChunkWidthEnd += _maxWidth;
    }

    private void HandleElevation(int chunkId)
    {
        var minX = _previousChunkWidthEnd + 1;
        var maxX = _previousChunkWidthEnd + _maxWidth;

        foreach (var model in tranningHandler.GetElevationModels())
        {
            var xPos = Random.Range(minX, maxX - model.width);
            var yPos = FindHighestBlock(xPos, model.width, chunkId) + 1;
            var previousBlockHeigth = FindBlockHighestPosition(chunkId, xPos, yPos, model.heigth);

            if (yPos + model.heigth - previousBlockHeigth > 4)
            {
                if(yPos + 1 - previousBlockHeigth > 4)
                {
                    model.heigth = 0;
                }
                else
                {
                    model.heigth = 1;
                }
            }

            var beginposition = new Vector2Int(xPos, 1);
            var endPosition = new Vector2Int(xPos + model.width, yPos + model.heigth);
            
            var chunk = _chunks[chunkId];

            GenerateBlocks(beginposition, endPosition, chunk);

            if (model.hasEnemies)
            {
                GenerateGoomba(chunk, endPosition);
            }        
        }
    }

    private void HandlePlatforms(int chunkId)
    {
        var minX = _previousChunkWidthEnd;
        var maxX = _previousChunkWidthEnd + _maxWidth;
        var chunk = _chunks[chunkId];

        foreach (var model in tranningHandler.GetPlatformModels())
        {
            var xPos = Random.Range(minX, maxX - model.width);
            var yPos = FindHighestBlock(xPos, model.width - 1, chunkId) + model.heigth;
            var beginposition = new Vector2Int(xPos, yPos);
            var endPosition = new Vector2Int(xPos + model.width, yPos + 1);

            GenerateBlocks(beginposition, endPosition, chunk, model.hasSpecialBlocks);

            if (model.hasEnemies)
            {
                GenerateGoomba(chunk, endPosition);
            }
            if (model.hasCoins)
            {
                for(int x = beginposition.x; x < endPosition.x; x++)
                {
                    GenerateCoins(x, yPos + 1, chunk);
                }
            }
            if (model.chasmModel != null)
            {
                var halfLength = beginposition.x;
                var startX = halfLength;
                var endX = startX + model.chasmModel.width;

                GenerateChasmBlocks(new Vector2Int(startX, minHeigth), new Vector2Int(endX, yPos - 1), chunkId);
            }
        }
    }

    private void HandleEnemies(int chunkId)
    {
        var minX = _previousChunkWidthEnd;
        var maxX = _previousChunkWidthEnd + _maxWidth;
        var chunk = _chunks[chunkId];

        foreach (var model in tranningHandler.GetEnemyModels())
        {
            var xPosition = Random.Range(minX, maxX);
            var position = new Vector2Int(xPosition, _maxHeigth);

            GenerateGoomba(chunk, position);
        }
    }

    private void HandleChasm(int chunkId)
    {
        var minX = _previousChunkWidthEnd;
        var maxX = _previousChunkWidthEnd + _maxWidth;

        foreach (var model in tranningHandler.GetChasmModels())
        {
            var xPos = Random.Range(minX, maxX - model.width);

            var beginposition = new Vector2Int(xPos, minHeigth);
            var endPosition = new Vector2Int(xPos + model.width, _maxHeigth);

            GenerateChasmBlocks(beginposition, endPosition, chunkId);
        }
    }

    private void GenerateCoins(int x, int y, GameObject chunk)
    {
        var go = Instantiate(_coin, chunk.transform);
        var pos = new Vector2(x, y);
        var component = go.AddComponent<EntityModel>();

        go.name = "coin";
        go.transform.position = pos;

        AddEntity(_lastGeneratedChunk, new Vector2Int(x, y), component, EntityType.Coin);
    }

    private void GenerateBlocks(Vector2Int begin, Vector2Int end, GameObject chunk, bool hasSpecial = false)
    {
        for(int x = begin.x; x < end.x; x++)
        {
            for(int y = begin.y; y < end.y; y++)
            {
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
                var component = go.AddComponent<EntityModel>();

                go.name = "block";
                go.transform.position = pos;

                AddEntity(_lastGeneratedChunk, new Vector2Int(x, y), component, EntityType.Solid);
            }
        }
    }

    private void GenerateGoomba(GameObject chunk, Vector2Int end)
    {
        var go = Instantiate(_goomba, chunk.transform);
        var pos = new Vector2Int(end.x, end.y + 1);
        var component = go.AddComponent<EntityModel>();

        go.name = "goomba";
        go.transform.position = new Vector2(pos.x, pos.y);

        AddEntity(_lastGeneratedChunk, pos, component, EntityType.Enemy);
    }

    private void GenerateChasmBlocks(Vector2Int begin, Vector2Int end, int chunkId)
    {
        for (int x = begin.x; x < end.x; x++)
        {
            for (int y = begin.y; y < end.y; y++)
            {
                var index = GetId(x, y, chunkId);
                var block = GetEntity(x, y, chunkId);

                if (block != null)
                {
                    Destroy(block.gameObject);
                    _entities[chunkId].Remove(index);
                }
            }
        }
    }

    private void SetupEndOfChunk(GameObject chunkGo, int x, int y)
    {
        var goEnd = Instantiate(_endOfChunk, chunkGo.transform);
        var component = goEnd.GetComponent<EndOfChunkCollider>();

        goEnd.transform.position = new Vector2(x, y);

        component.Setup(_lastGeneratedChunk, tranningHandler.GetTranningTypes());
        
        _lastGeneratedChunk++;
    }

    private EntityModel GetEntity(int x, int y, int chunk)
    {
        var key = x * y * chunk;
        var contains = _entities[chunk].ContainsKey(key);

        if (contains)
            return _entities[chunk][key];

        return null;
    }

    private int FindBlockHighestPosition(int chunkId, int x, int y, int heigth)
    {
        int highestPositon = 0;

        var chunk = _entities[chunkId];
        var xPos = x - 1;

        foreach (var block in chunk)
        {
            var value = block.Value;

            if(value.xPos == xPos && value.yPos > highestPositon)
            {
                highestPositon = block.Value.yPos;
            }
        }

        return highestPositon;
    }

    private int FindHighestBlock(int x, int width, int chunkId)
    {
        int highestYPos = 0;

        var chunk = _entities[chunkId];

        foreach(var block in chunk)
        {
            var value = block.Value;

            if (value.xPos >= x && value.xPos <= x + width)
            {
                if (block.Value.yPos > highestYPos)
                {
                    highestYPos = block.Value.yPos;
                }
            }
        }

        return highestYPos;
    }

    private void AddEntity(int chunkId, Vector2Int position, EntityModel entityModel, EntityType entityType)
    {
        var id = GetId(position.x, position.y, chunkId);

        if (_entities.ContainsKey(chunkId) == false)
        {
            _entities.Add(chunkId, new Dictionary<int, EntityModel>());
        }

        if (_entities[chunkId].ContainsKey(id) == false)
        {
            entityModel.Setup(id, position.x, position.y, entityType);
            _entities[chunkId].Add(id, entityModel);
        }
    }

    private int GetId(int x, int y, int chunkId)
        => x * y * chunkId;

    private bool ContainsId(int id, int chunkId)
        => _entities[chunkId].ContainsKey(id);

    private void CleanEntitiesInChunk(int chunkId)
        => _entities.Remove(chunkId);
}
