public class PlatformModel
{
    public int width;
    public int heigth;
    public bool hasCoins;
    public bool hasEnemies;
    public bool hasChasm;
    public bool hasSpecialBlocks;

    public ChasmModel chasmModel;

    public PlatformModel(int w, int h, bool containsCoins = false, bool containsEnemies = false, bool containsSpecialBlocks = false, ChasmModel chasmModel = null)
    {
        width = w;
        heigth = h;
        hasCoins = containsCoins;
        hasEnemies = containsEnemies;
        hasSpecialBlocks = containsSpecialBlocks;
        this.chasmModel = chasmModel;
    }
}
