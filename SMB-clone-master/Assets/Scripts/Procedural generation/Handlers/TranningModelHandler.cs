using System.Collections.Generic;
using UnityEngine;

public class TranningModelHandler : MonoBehaviour
{
    [SerializeField] private SkillsCollectionConfiguration _skillsCollection;

    public List<ElevationModel> elevationModels = new List<ElevationModel>();
    public List<ChasmModel> chasmModels = new List<ChasmModel>();
    public List<PlatformModel> platformModels = new List<PlatformModel>();
    public List<FireBarModel> fireBarModels = new List<FireBarModel>();
    public List<EnemyModel> enemyModels = new List<EnemyModel>();

    public TranningModel model;

    private void Awake()
    {
        model = new TranningModel();
        model.SetPlayerSkillConfiguration(_skillsCollection);

        // TODO Load in from external file.
        model.SetTranningType((int)TranningType.Walking);
    }

    public SkillsCollectionConfiguration Get()
        => _skillsCollection;

    public void GenerateModelsBasedOnSkill()
    {
        Clear();

        GenerateShortJumpModels();
        GenerateMediumJumpModels();
        GenerateEnemies(1);
        GenerateLongJumpModels();
        GeneratePlatformModels(2, 5, 2, 4, 0, false, true, true, false);
        GenerateFireBarModel();
    }

    private void Clear()
    {
        elevationModels.Clear();
        chasmModels.Clear();
        platformModels.Clear();
        enemyModels.Clear();
        fireBarModels.Clear();
    }

    private void GenerateShortJumpModels(bool hasEnemies = false, int minChance = 100)
    {
        for (int i = 0; i < model.ShortJumpSkill; i++)
        {
            elevationModels.Add(
                new ElevationModel
                {
                    heigth = Random.Range(1, 1),
                    width = Random.Range(1, 4),
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
                    width = Random.Range(2, 4),
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

    private void GenerateFireBarModel()
    {
        for (int i = 0; i < model.FireBarSkill; i++)
        {
            fireBarModels.Add(new FireBarModel());
        }
    }

    private void GenerateEnemies(int amount = 1)
    {
        for(int i = 0; i < model.EnemySkill; i++)
        {
            enemyModels.Add(new EnemyModel
            {
                amount = amount,
                enemytype = (Enemytype)Random.Range(0, model.Difficulty > 40 ? 3 : 2)
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
            var containsSpecialBlocks = containsChasm == false;

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
