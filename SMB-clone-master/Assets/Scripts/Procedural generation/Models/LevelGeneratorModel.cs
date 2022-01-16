public class LevelGeneratorModel
{
    // This is used to calculate the max amount of generated objects within a chunk.
    public int maxAmountOfObjectsPlaced;

    public int amountOfPlatforms;
    public int amountOfEnemies;
    public int shortJumps;
    public int mediumJumps;
    public int longJumps;

    public LevelGeneratorModel(TranningModel tranningModel)
    {
        foreach (var tranningType in tranningModel.GetCurrentTrannigType())
        {
            switch (tranningType)
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
                case TranningType.Platform:
                    amountOfPlatforms = 1;
                    break;
            }
        }
    }
}
