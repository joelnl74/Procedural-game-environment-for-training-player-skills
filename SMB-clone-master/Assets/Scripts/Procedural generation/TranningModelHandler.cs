using System.Collections.Generic;
using UnityEngine;

public class TranningModelHandler
{
    public List<ElevationModel> elevationModels = new List<ElevationModel>();
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
        switch (model.GetCurrentTrannigType())
        {
            case TranningType.None:
                break;
            case TranningType.Walking:
                break;
            case TranningType.Short_Jump:
                GenerateShortJumpModels();
                break;
            case TranningType.Medium_Jump:
                break;
            case TranningType.Long_Jump:
                break;
            case TranningType.High_Jump:
                break;
        }
    }

    private void GenerateShortJumpModels()
    {
        for (int i = 0; i < model.ShotJumpSkill; i++)
        {
            elevationModels.Add(
                new ElevationModel
                {
                    heigth = Random.Range(1, 1),
                    width = Random.Range(1, 5)
                });
        }
    }
}
