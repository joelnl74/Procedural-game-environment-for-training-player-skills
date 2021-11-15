
using UnityEngine;

public enum TranningType
{
    None = 0,
    Walking = 1,
    Short_Jump = 2,
    Medium_Jump = 3,
    Long_Jump = 5,
    Enemies = 6,
    High_Jump = 7,
    BasicsTest = 8,
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
                LongJumpSkill = Random.Range(1, 2);
                break;
            case TranningType.High_Jump:
                HighJumpSkill = Random.Range(1, 2);
                break;
            case TranningType.BasicsTest:
                //TODO;
                break;
        }
    }
}
