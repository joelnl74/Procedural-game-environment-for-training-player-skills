using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TranningModelHandler : MonoBehaviour
{
    [SerializeField] private SkillsCollectionConfiguration _skillsCollection;

    public List<ElevationModel> elevationModels = new List<ElevationModel>();
    public List<ChasmModel> chasmModels = new List<ChasmModel>();
    public List<PlatformModel> platformModels = new List<PlatformModel>();
    public List<EnemyModel> enemyModels = new List<EnemyModel>();

    public TranningModel model;

    private void Awake()
    {
        model = new TranningModel();
        model.SetPlayerSkillConfiguration(_skillsCollection);

        // TODO Load in from external file.
        model.SetTranningType(new List<TranningType> { TranningType.Walking });
    }

    public List<TranningType> GetTranningType()
        => model.GetCurrentTrannigType();

    public void GenerateModelsBasedOnSkill()
    {
        Clear();

        foreach(var tranningType in model.GetCurrentTrannigType())
        {
            switch (tranningType)
            {
                case TranningType.None:
                    break;
                case TranningType.Walking:
                    break;
                case TranningType.Short_Jump:
                    GenerateShortJumpModels();
                    break;
                case TranningType.Enemies:
                    GenerateEnemies(1, (Enemytype)Random.Range(0, 2));
                    break;
                case TranningType.Medium_Jump:
                    GenerateShortJumpModels();
                    GenerateMediumJumpModels();
                    break;
                case TranningType.Long_Jump:
                    GenerateLongJumpModels();
                    break;
                case TranningType.Platform:
                    GeneratePlatformModels(2, 6, 3, 4, 0, false, true, true, true);
                    break;
                default:
                    GenerateShortJumpModels();
                    GenerateMediumJumpModels();
                    GenerateLongJumpModels();
                    GenerateEnemies(1, (Enemytype)Random.Range(0, 2));
                    GeneratePlatformModels(2, 6, 3, 4, 0, true, true, true, false);
                    break;
            }
        }
    }

    private void Clear()
    {
        elevationModels.Clear();
        chasmModels.Clear();
        platformModels.Clear();
        enemyModels.Clear();
    }

    private void GenerateShortJumpModels(bool hasEnemies = false, int minChance = 100)
    {
        for (int i = 0; i < model.ShortJumpSkill; i++)
        {
            elevationModels.Add(
                new ElevationModel
                {
                    heigth = Random.Range(1, 1),
                    width = Random.Range(1, 5),
                    hasEnemies = hasEnemies && Random.Range(0, 100) > minChance
                });
        }
    }

    private void GenerateMediumJumpModels(bool hasEnemies = false, int minChance = 100)
    {
        for (int i = 0; i < model.MediumJumpSkill; i++)
        {
            elevationModels.Add(
                new ElevationModel
                {
                    heigth = Random.Range(3, 4),
                    width = Random.Range(3, 5),
                    hasEnemies = hasEnemies && Random.Range(0, 100) > minChance
                });
        }
    }

    private void GenerateLongJumpModels(bool hasEnemies = false, int minChance = 100)
    {
        for (int i = 0; i < model.LongJumpSkill; i++)
        {
            chasmModels.Add(
                new ChasmModel
                {
                    width = Random.Range(3, 4),
                });
        }
    }

    private void GenerateEnemies(int amount = 1, Enemytype type = Enemytype.Goomba)
    {
        for(int i = 0; i < model.EnemySkill; i++)
        {
            enemyModels.Add(new EnemyModel
            {
                amount = amount,
                enemytype = type
            });
        }
    }

    private void GeneratePlatformModels(int minWidth, int maxWidth, int minHeigth, int maxHeigth, int minChance = 0, bool hasEnemies = false, bool hasCoins = false, bool HasChasm = false, bool forceChasm = false)
    {
        for (int i = 0; i < model.HighJumpSkill; i++)
        {
            var w = Random.Range(minWidth, maxWidth);
            var h = Random.Range(minHeigth, maxHeigth);
            var containsEnemies = hasEnemies && Random.Range(0, 100) > minChance;
            var containsCoins = hasCoins && Random.Range(0, 100) > 50;
            var containsChasm = HasChasm && Random.Range(0, 100) > 33 || forceChasm;
            var containsSpecialBlocks = GetTranningType().Max() > TranningType.Platform && containsChasm == false;

            ChasmModel chasmModel = null;

            if (containsChasm)
            {
                chasmModel = new ChasmModel
                {
                    width = w
                };
            }

            platformModels.Add(new PlatformModel(w, h, containsCoins, containsEnemies, containsSpecialBlocks, chasmModel));
        }
    }
}
