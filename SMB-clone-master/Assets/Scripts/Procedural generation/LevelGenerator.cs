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

    [SerializeField] private Mario _mario;

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
        var chunk = _chunks[id];

        _chunks.Remove(id);

        CleanEntitiesInChunk(id);
        Destroy(chunk);
        GenerateChunk();
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

        SetupEndOfChunk(chunk, maxX + 1, 0);

        _previousChunkWidthEnd += _maxWidth;
    }

    private void HandleElevation(int chunkId)
    {
        var minX = _previousChunkWidthEnd;
        var maxX = _previousChunkWidthEnd + _maxWidth;

        foreach (var model in tranningHandler.GetElevationModels())
        {
            var xPos = Random.Range(minX, maxX - model.width);
            var yPos = FindHighestBlock(xPos, model.width, chunkId);

            var beginposition = new Vector2Int(xPos, yPos);
            var endPosition = new Vector2Int(xPos + model.width, yPos + model.heigth);
            
            var chunk = _chunks[chunkId];

            GenerateSolidBlocks(beginposition, endPosition, chunk);

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

        foreach (var model in tranningHandler.GetPlatformModels())
        {
            var xPos = Random.Range(minX, maxX - model.width);

            var beginposition = new Vector2Int(xPos, model.heigth);
            var endPosition = new Vector2Int(xPos + model.width, model.heigth + 1);

            var chunk = _chunks[chunkId];

            GenerateSolidBlocks(beginposition, endPosition, chunk);

            if (model.hasEnemies)
            {
                GenerateGoomba(chunk, endPosition);
            }
            if(model.hasCoins)
            {
                for(int x = beginposition.x; x < endPosition.x; x++)
                {
                    GenerateCoins(x, model.heigth + 1, chunk);
                }
            }
            if(model.hasChasm)
            {
                var startX = Random.Range(beginposition.x + model.width - 1, beginposition.x + model.width + 2);
                var endX = startX + Random.Range(4, 5);

                GenerateChasmBlocks(new Vector2Int(startX, minHeigth), new Vector2Int(endX, _maxHeigth), chunkId);
            }
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

    private void GenerateSolidBlocks(Vector2Int begin, Vector2Int end, GameObject chunk)
    {
        for(int x = begin.x; x < end.x; x++)
        {
            for(int y = begin.y; y < end.y; y++)
            {
                var go = Instantiate(_groundBlock, chunk.transform);
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

                if (block == null && x == begin.x && y == 1)
                {
                    // TODO generate platform or block.
                }

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

        component.Setup(_lastGeneratedChunk, new List<TranningType>());
        
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

    private int FindHighestBlock(int xPos, int width, int chunkId)
    {
        // TODO create dictonary of coordinates and check this.
        var chunk = _entities[chunkId];
        var highestYPos = minHeigth + 1;

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

    private void CleanEntitiesInChunk(int chunkId)
    {
        _entities.Remove(chunkId);
    }
}
