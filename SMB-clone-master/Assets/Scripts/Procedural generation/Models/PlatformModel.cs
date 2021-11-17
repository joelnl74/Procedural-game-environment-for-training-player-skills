public class PlatformModel
{
    public int width;
    public int heigth;
    public bool hasCoins;
    public bool hasEnemies;
    public bool hasChasm;

    public ChasmModel chasmModel;

    public PlatformModel(int w, int h, bool containsCoins = false, bool containsEnemies = false, ChasmModel chasmModel = null)
    {
        width = w;
        heigth = h;
        hasCoins = containsCoins;
        hasEnemies = containsEnemies;
        this.chasmModel = chasmModel;
    }
}
