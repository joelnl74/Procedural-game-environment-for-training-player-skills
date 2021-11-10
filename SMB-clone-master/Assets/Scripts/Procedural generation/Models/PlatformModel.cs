public class PlatformModel
{
    public int width;
    public int heigth;
    public bool hasCoins;
    public bool hasEnemies;
    public bool hasChasm;

    public PlatformModel(int w, int h, bool containsCoins = false, bool containsEnemies = false, bool containsChasm = false)
    {
        width = w;
        heigth = h;
        hasCoins = containsCoins;
        hasEnemies = containsEnemies;
        hasChasm = containsChasm;
    }
}
