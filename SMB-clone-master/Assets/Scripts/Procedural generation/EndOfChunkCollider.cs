using UnityEngine;
using System.Collections.Generic;

public class EndOfChunkCollider : MonoBehaviour
{
    [SerializeField] private Collider2D _collider2D;

    private int _chunkId;
    private List<TranningType> _tranningTypes;

    public void Setup(int chunkId, List<TranningType> tranningTypes)
    {
        _chunkId = chunkId;
        _tranningTypes = tranningTypes;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            PCGEventManager.Instance.onReachedEndOfChunk?.Invoke(_chunkId, _tranningTypes);
            _collider2D.enabled = false;
        }
    }
}
