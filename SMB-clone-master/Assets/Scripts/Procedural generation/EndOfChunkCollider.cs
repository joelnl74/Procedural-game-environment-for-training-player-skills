using UnityEngine;
using System.Collections.Generic;

public class EndOfChunkCollider : MonoBehaviour
{
    [SerializeField] private Collider2D _collider2D;

    private int _chunkId;
    private bool _isCooldownChunk;
    private List<TrainingType> _tranningTypes;

    private bool _triggerd = false;

    public void Setup(int chunkId, bool isCooldownChunk, List<TrainingType> tranningTypes)
    {
        _chunkId = chunkId;
        _tranningTypes = tranningTypes;
        _isCooldownChunk = isCooldownChunk;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(_triggerd == true)
        {
            return;
        }

        if (collision.tag == "Player")
        {
            PCGEventManager.Instance.onReachedEndOfChunk?.Invoke(_chunkId, _isCooldownChunk, _tranningTypes);
            _collider2D.enabled = false;
            _triggerd = true;
        }
    }
}
