
public enum TranningType
{
    None = 0,
    Walking = 1,
    Short_Jump = 2,
    Medium_Jump = 3,
    Long_Jump = 4,
    High_Jump = 5,
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
}
