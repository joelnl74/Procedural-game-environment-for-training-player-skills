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

    [SerializeField] private Mario _mario;

    private Dictionary<int, Dictionary<int, List<Vector2>>> _entities;
    private Dictionary<int, GameObject> _chunks;

    private int _previousChunkWidthEnd = 0;
    private int _previousChunkHeigthEnd = 0;

    private int _lastGeneratedChunk = 1;
    private int _currentChunk = 1;

    // Start is called before the first frame update
    private void Awake()
    {
        _entities = new Dictionary<int, Dictionary<int, List<Vector2>>>();
        _chunks = new Dictionary<int, GameObject>();

        for(int i = 0; i < _chunksToGenerate; i++)
        {
            GenerateChunk(i == 0);
        }
    }

    private void Start()
    {
        _mario.EnablePhysics();
    }

    private void GenerateChunk(bool generateSpawnPoint = false)
    {
        var chunk = new GameObject();
        var maxX = _previousChunkWidthEnd + _maxWidth;

        chunk.name = $"Chunk {_lastGeneratedChunk}";

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

        SetupEndOfChunk(chunk, maxX + 1, 0);

        _previousChunkWidthEnd = maxX;
    }

    private void SetupEndOfChunk(GameObject chunkGo, int x, int y)
    {
        var goEnd = Instantiate(_endOfChunk, chunkGo.transform);
        var component = goEnd.GetComponent<EndOfChunkCollider>();

        goEnd.transform.position = new Vector2(x, y);

        component.Setup(_lastGeneratedChunk);
        component.onReachedEndOfChunk += CheckEndOfChunk;

        _chunks.Add(_lastGeneratedChunk, chunkGo);

        _lastGeneratedChunk++;
    }

    private void CheckEndOfChunk(int id)
    {
        var chunk = _chunks[id];

        _currentChunk = id;
        _chunks.Remove(id);

        CleanEntitiesInChunk(id);
        Destroy(chunk);
        GenerateChunk(false);
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
