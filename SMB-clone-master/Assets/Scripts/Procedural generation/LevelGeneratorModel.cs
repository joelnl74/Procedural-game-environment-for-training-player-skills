public class LevelGeneratorModel
{
    public int amountOfPlatforms;
    public int amountOfEnemies;
    public int shortJumps;
    public int mediumJumps;
    public int longJumps;

    public LevelGeneratorModel(TranningModel tranningModel)
    {
        switch (tranningModel.GetCurrentTrannigType())
        {
            case TranningType.None:
                break;
            case TranningType.Walking:
                break;
            case TranningType.Short_Jump:
                shortJumps = 1;
                break;
            case TranningType.Medium_Jump:
                mediumJumps = 1;
                break;
            case TranningType.Long_Jump:
                longJumps = 1;
                break;
            case TranningType.High_Jump:
                amountOfPlatforms = 1;
                break;
        }
    }
}
