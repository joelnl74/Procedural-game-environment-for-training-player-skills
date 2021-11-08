using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LevelGenerator : MonoBehaviour
{
    private int _halfWidth;

    [SerializeField] private int _maxHeigth;
    [SerializeField] private int _maxWidth;
    [SerializeField] private int _chunksToGenerate;

    [SerializeField] private GameObject _spawnPoints;
    [SerializeField] private GameObject _endOfChunk;
    [SerializeField] private GameObject _groundBlock;

    [SerializeField] private Mario _mario;

    private Dictionary<int, Dictionary<int, List<Vector2>>> _entities;
    private Dictionary<int, GameObject> _chunks;

    private int _previousChunkWidthEnd = 0;
    private int _previousChunkHeigthEnd = 0;

    private int _lastGeneratedChunk = 1;

    private TranningModelHandler _tranningModelHandler;

    public void SetupLevel(TranningModelHandler handler)
    {
        _entities = new Dictionary<int, Dictionary<int, List<Vector2>>>();
        _chunks = new Dictionary<int, GameObject>();
        _tranningModelHandler = handler;

        for (int i = 0; i < _chunksToGenerate; i++)
        {
            GenerateChunk(handler);
        }
    }

    private void Start()
    {
        _halfWidth = _maxWidth / 2;

        _mario.EnablePhysics();
    }

    private void GenerateChunk(TranningModelHandler handler)
    {
        var chunk = new GameObject();
        var maxX = _previousChunkWidthEnd + _maxWidth;

        chunk.name = $"Chunk {_lastGeneratedChunk}";

        _chunks.Add(_lastGeneratedChunk, chunk);

        for (int x = _previousChunkWidthEnd; x < maxX; x++)
            for (int y = 0; y < _maxHeigth; y++)
            {
                if (x == 0 && y == 2)
                {
                    var spawnPos = Instantiate(new GameObject(), _spawnPoints.transform);
                    spawnPos.name = "Spawn position";
                }
                if (y == 0)
                {
                    var go = Instantiate(_groundBlock, chunk.transform);
                    var pos = new Vector2(x, y);

                    go.name = "block";
                    go.transform.position = pos;

                    AddEntity(_lastGeneratedChunk, 1, pos);
                }
            }

        HandleElevation(handler, _lastGeneratedChunk);

        SetupEndOfChunk(chunk, maxX + 1, 0);

        _previousChunkWidthEnd = maxX;
    }

    private void HandleElevation(TranningModelHandler handler, int chunkId)
    {
        foreach(var model in handler.elevationModels)
        {
            var startPos = chunkId * _maxWidth - _halfWidth;
            var xPos = Random.Range(startPos - model.width, startPos + model.width);
            var yPos = FindHighestBlock(xPos, model.width, chunkId);

            var beginposition = new Vector2Int(xPos, yPos);
            var endPosition = new Vector2Int(xPos + model.width, yPos + model.heigth);

            GenerateSolidBlocks(beginposition, endPosition, _chunks[chunkId]);
        }
    }

    private void GenerateSolidBlocks(Vector2Int begin, Vector2Int end, GameObject chunk)
    {
        for(int x = begin.x; x < end.x; x++)
        {
            for(int y = begin.y; y < end.y; y++)
            {
                var go = Instantiate(_groundBlock, chunk.transform);
                var pos = new Vector2(x, y);

                go.name = "block";
                go.transform.position = pos;

                AddEntity(_lastGeneratedChunk, 1, pos);
            }
        }
    }

    private int FindHighestBlock(int xPos, int width, int chunkId)
    {
        // TODO create dictonary of coordinates and check this.
        var chunk = _entities[chunkId];
        var highestYPos = 1;

        return highestYPos;
    }

    private void SetupEndOfChunk(GameObject chunkGo, int x, int y)
    {
        var goEnd = Instantiate(_endOfChunk, chunkGo.transform);
        var component = goEnd.GetComponent<EndOfChunkCollider>();

        goEnd.transform.position = new Vector2(x, y);

        component.Setup(_lastGeneratedChunk, new List<TranningType>());
        component.onReachedEndOfChunk += CheckEndOfChunk;

        _lastGeneratedChunk++;
    }

    private void CheckEndOfChunk(int id, List<TranningType> tranningTypes)
    {
        var chunk = _chunks[id];

        _chunks.Remove(id);

        _tranningModelHandler.model.SetTranningType(TranningType.Short_Jump);
        _tranningModelHandler.GenerateModelsBasedOnSkill();
        CleanEntitiesInChunk(id);
        Destroy(chunk);
        GenerateChunk(_tranningModelHandler);
    }

    private void AddEntity(int chunk, int type, Vector2 position)
    {
        if(_entities.ContainsKey(chunk) == false)
        {
            _entities.Add(chunk, new Dictionary<int, List<Vector2>>());

            if(_entities[chunk].ContainsKey(type) == false)
            {
                _entities[chunk].Add(type, new List<Vector2>());
            }
        }

        _entities[chunk][type].Add(position);
    }

    private void CleanEntitiesInChunk(int chunkId)
    {
        _entities.Remove(chunkId);
    }
}
