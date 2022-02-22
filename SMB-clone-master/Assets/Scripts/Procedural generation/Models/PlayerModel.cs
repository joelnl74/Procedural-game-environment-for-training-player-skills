using System.Collections.Generic;

public class ChunkInformation
{
    public int deathCount = 0;
    public int jumpDeaths = 0;
    public int enemiesDeaths = 0;
}

public class PlayerModel
{
    public int deathTotalCount = 0;
    public int jumpTotalDeaths = 0;
    public int enemiesTotalDeaths = 0;

    private Dictionary<int, ChunkInformation> _chunkStats = new Dictionary<int, ChunkInformation>();

    public void UpdateChunk(int chunkId, int jumpDeaths, int enemyDeaths)
    {
        _chunkStats.Add(chunkId, new ChunkInformation
        {
            deathCount = jumpDeaths + enemyDeaths,
            jumpDeaths = jumpDeaths,
            enemiesDeaths = enemyDeaths
        });
    }

    public int ReturnDifficulty()
    {
        return 50;
    }
}
