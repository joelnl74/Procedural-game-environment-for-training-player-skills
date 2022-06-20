using UnityEngine;

public enum EntityType
{
    Unknown = 0,
    Solid = 1,
    Platform = 2,
    Enemy = 3,
    Coin = 4,
    FireBar = 5,
}

public class EntityModel
{
    public EntityType entityType;
    public Enemytype enemytype;
    public GameObject gameObject;
    public int id;
    public int xPos;
    public int yPos;
    
    public void Setup(int entityId, int x, int y, GameObject go, EntityType type)
    {
        id = entityId;
        xPos = x;
        yPos = y;
        entityType = type;
        gameObject = go;
    }

    public void SetEnemyType(Enemytype type)
    {
        enemytype = type;
    }
}
