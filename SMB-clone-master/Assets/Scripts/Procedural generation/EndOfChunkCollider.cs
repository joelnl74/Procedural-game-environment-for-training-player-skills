using UnityEngine;
using System;

public class EndOfChunkCollider : MonoBehaviour
{
    public Action<int> onReachedEndOfChunk;

    private int _chunkId;

    public void Setup(int chunkId)
    {
        _chunkId = chunkId;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            onReachedEndOfChunk?.Invoke(_chunkId);
        }
    }
}
