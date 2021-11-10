using System.Collections.Generic;
using UnityEngine;

public class TranningModelHandler
{
    public List<ElevationModel> elevationModels = new List<ElevationModel>();
    public List<ChasmModel> chasmModels = new List<ChasmModel>();
    public List<PlatformModel> platformModels = new List<PlatformModel>();

    public TranningModel model;

    public TranningModelHandler()
    {
        model = new TranningModel();

        model.SetTranningType(TranningType.Walking);

        // TODO Load in from external file.
    }

    public TranningType GetTranningType()
        => model.GetCurrentTrannigType();

    public void GenerateModelsBasedOnSkill()
    {
        Clear();

        switch (model.GetCurrentTrannigType())
        {
            case TranningType.None:
                break;
            case TranningType.Walking:
                break;
            case TranningType.Short_Jump:
                GenerateShortJumpModels();
                break;
            case TranningType.Enemies:
                GenerateShortJumpModels(true, 25);
                break;
            case TranningType.Medium_Jump:
                GenerateShortJumpModels();
                GenerateMediumJumpModels();
                break;
            case TranningType.Long_Jump:
                GenerateLongJumpModels();
                break;
            case TranningType.High_Jump:
                GeneratePlatformModels(4, 4, 0, false, true, true);
                break;

        }
    }

    private void Clear()
    {
        elevationModels.Clear();
        chasmModels.Clear();
        platformModels.Clear();
    }

    private void GenerateShortJumpModels(bool hasEnemies = false, int minChance = 100)
    {
        for (int i = 0; i < model.ShotJumpSkill; i++)
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
                    heigth = Random.Range(2, 3),
                    width = Random.Range(3, 5),
                    hasEnemies = hasEnemies && Random.Range(0, 100) > minChance
                });
        }
    }

    private void GenerateLongJumpModels(bool hasEnemies = false, int minChance = 100)
    {
        for (int i = 0; i < model.MediumJumpSkill; i++)
        {
            chasmModels.Add(
                new ChasmModel
                {
                    width = Random.Range(3, 5),
                });
        }
    }

    private void GeneratePlatformModels(int width, int heigth, int minChance = 0, bool hasEnemies = false, bool hasCoins = false, bool HasChasm = false)
    {
        for (int i = 0; i < model.HighJumpSkill; i++)
        {
            var w = Random.Range(4, 6);
            var h = Random.Range(4, 6);
            var containsEnemies = hasEnemies && Random.Range(0, 100) > minChance;
            var containsCoins = hasCoins && Random.Range(0, 100) > 50;
            var containsChasm = HasChasm && Random.Range(0, 100) > 50;

            platformModels.Add(new PlatformModel(w, h, containsCoins, containsEnemies, containsChasm));
        }
    }
}
