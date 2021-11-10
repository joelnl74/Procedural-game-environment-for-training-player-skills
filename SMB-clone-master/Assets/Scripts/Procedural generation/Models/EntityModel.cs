using UnityEngine;

public enum EntityType
{
    Unknown = 3,
    Solid = 1,
    Enemy = 2,
    Coin = 3
}

public class EntityModel : MonoBehaviour
{
    public EntityType entityType;

    public int id;
    public int xPos;
    public int yPos;
    
    public void Setup(int entityId, int x, int y, EntityType type)
    {
        id = entityId;
        xPos = x;
        yPos = y;
        entityType = type;
    }
}
