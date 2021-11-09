using System;
using System.Collections.Generic;
using UnityEngine;

public class TranningHandler : MonoBehaviour
{
    [SerializeField] private LevelGenerator _levelGenerator;
    
    private TranningModelHandler tranningModelHandler;
    private List<TranningType> tranningTypes = new List<TranningType>();

    public List<TranningType> GetTranningTypes()
        => tranningTypes;

    public TranningModel GetTranningModel()
        => tranningModelHandler.model;

    public List<ElevationModel> GetElevationModels()
        => tranningModelHandler.elevationModels;

    public List<ChasmModel> GetChasmModels()
    => tranningModelHandler.chasmModels;

    private void Awake()
    {
        tranningModelHandler = new TranningModelHandler();
        tranningModelHandler.GenerateModelsBasedOnSkill();

        _levelGenerator.SetupLevel(this);

        PCGEventManager.Instance.onReachedEndOfChunk += CheckEndOfChunk;
    }

    private void CheckEndOfChunk(int chunkId, List<TranningType> types)
    {
        if (chunkId > 0)
        {
            tranningModelHandler.model.SetTranningType(TranningType.Short_Jump);
        }
        if(chunkId > 2)
        {
            tranningModelHandler.model.SetTranningType(TranningType.Enemies);
        }
        if(chunkId > 3)
        {
            tranningModelHandler.model.SetTranningType(TranningType.Medium_Jump);
        }
        if (chunkId > 4)
        {
            tranningModelHandler.model.SetTranningType(TranningType.Long_Jump);
        }

        tranningModelHandler.GenerateModelsBasedOnSkill();

        tranningTypes = new List<TranningType> { tranningModelHandler.GetTranningType() };

        _levelGenerator.ReachedEndOfChunk(chunkId, tranningTypes);
    }
}
