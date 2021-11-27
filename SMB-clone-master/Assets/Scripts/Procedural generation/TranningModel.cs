using UnityEngine;

public enum TranningType
{
    None = 0,
    Walking = 1,
    Short_Jump = 2,
    Medium_Jump = 3,
    Enemies = 4,
    Long_Jump = 5,
    Platform = 6,
    BasicsTest = 7,
}

public class TranningModel
{
    public int WalkingSkill = 0;
    public int ShortJumpSkill = 0;
    public int MediumJumpSkill = 0;
    public int LongJumpSkill = 0;
    public int HighJumpSkill = 0;
    public int EnemySkill = 0;

    private TranningType _currentTranningType;

    public TranningType GetCurrentTrannigType()
        => _currentTranningType;

    public void SetTranningType(TranningType tranningType)
    {
        ResetSkills();

        _currentTranningType = tranningType;

        switch (_currentTranningType)
        {
            case TranningType.None:
                break;
            case TranningType.Walking:
                break;
            case TranningType.Short_Jump:
                ShortJumpSkill = Random.Range(6, 10);
                break;
            case TranningType.Medium_Jump:
                ShortJumpSkill = Random.Range(3, 5);
                MediumJumpSkill = Random.Range(3, 5);
                break;
            case TranningType.Enemies:
                EnemySkill = Random.Range(1, 2);
                break;
            case TranningType.Long_Jump:
                LongJumpSkill = Random.Range(1, 2);
                break;
            case TranningType.Platform:
                HighJumpSkill = Random.Range(1, 2);
                break;
            default:
                ShortJumpSkill = Random.Range(0, 4);
                MediumJumpSkill = Random.Range(0, 4);
                LongJumpSkill = Random.Range(0, 3);
                HighJumpSkill = Random.Range(0, 3);
                EnemySkill = Random.Range(0, 2);
                break;
        }
    }

    private void ResetSkills()
    {
        WalkingSkill = 0;
        ShortJumpSkill = 0;
        MediumJumpSkill = 0;
        LongJumpSkill = 0;
        HighJumpSkill = 0;
        EnemySkill = 0;
    }
}
