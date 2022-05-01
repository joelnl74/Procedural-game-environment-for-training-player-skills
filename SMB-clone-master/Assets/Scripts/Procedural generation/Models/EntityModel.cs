using UnityEngine;

public enum EntityType
{
    Unknown = 0,
    Solid = 1,
    Enemy = 2,
    Coin = 3,
    FireBar = 4,
}

public class EntityModel
{
    public EntityType entityType;
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
}
