using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private int _maxHeigth;
    [SerializeField] private int _maxWidth;
    [SerializeField] private int _chunksToGenerate;

    [SerializeField] private GameObject _endOfChunk;
    [SerializeField] private GameObject _groundBlock;

    [SerializeField] private Mario _mario;

    private Dictionary<int, List<Vector2>> _entities;
    private Dictionary<int, GameObject> _chunks;

    private int _previousChunkWidthEnd = 0;
    private int _previousChunkHeigthEnd = 0;

    private int _lastGeneratedChunk = 1;
    private int _currentChunk = 1;

    // Start is called before the first frame update
    private void Start()
    {
        _entities = new Dictionary<int, List<Vector2>>();
        _chunks = new Dictionary<int, GameObject>();

        for(int i = 0; i < _chunksToGenerate; i++)
        {
            GenerateChunk();
        }

        _mario.EnablePhysics();
    }

    private void GenerateChunk()
    {
        var chunk = new GameObject();

        var maxX = _previousChunkWidthEnd + _maxWidth;

        for (int x = _previousChunkWidthEnd; x < maxX; x++)
            for (int y = 0; y < _maxHeigth; y++)
            {
                if (y == 0)
                {
                    var go = Instantiate(_groundBlock, chunk.transform);
                    var pos = new Vector2(x, y);

                    go.transform.position = pos;

                    AddEntity(1, pos);
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

        Destroy(chunk);
        GenerateChunk();
    }

    private void AddEntity(int type, Vector2 position)
    {
        if(_entities.ContainsKey(type) == false)
        {
            _entities.Add(type, new List<Vector2>());
        }

        _entities[type].Add(position);
    }
}
