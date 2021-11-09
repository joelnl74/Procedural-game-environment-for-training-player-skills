
using UnityEngine;

public enum TranningType
{
    None = 0,
    Walking = 1,
    Short_Jump = 2,
    Enemies = 3,
    Medium_Jump = 4,
    Long_Jump = 5,
    High_Jump = 6,
}

public class TranningModel
{
    public int WalkingSkill = 0;
    public int ShotJumpSkill = 0;
    public int MediumJumpSkill = 0;
    public int LongJumpSkill = 0;
    public int HighJumpSkill = 0;
    public int EnemySkill = 0;

    private TranningType _currentTranningType;

    public TranningType GetCurrentTrannigType()
        => _currentTranningType;

    public void SetTranningType(TranningType tranningType)
    {
        _currentTranningType = tranningType;

        switch (_currentTranningType)
        {
            case TranningType.None:
                break;
            case TranningType.Walking:
                break;
            case TranningType.Short_Jump:
                ShotJumpSkill = Random.Range(1, 3);
                break;
            case TranningType.Enemies:
                ShotJumpSkill = Random.Range(3, 5);
                break;
            case TranningType.Medium_Jump:
                ShotJumpSkill = Random.Range(1, 3);
                MediumJumpSkill = Random.Range(1, 3);
                break;
            case TranningType.Long_Jump:
                break;
            case TranningType.High_Jump:
                break;

        }
    }
}
