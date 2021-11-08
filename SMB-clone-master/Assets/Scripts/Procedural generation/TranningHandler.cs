using UnityEngine;

public class TranningHandler : MonoBehaviour
{
    [SerializeField] private LevelGenerator _levelGenerator;
    
    private TranningModelHandler tranningModelHandler;

    // Start is called before the first frame update
    void Awake()
    {
        tranningModelHandler = new TranningModelHandler();
        tranningModelHandler.GenerateModelsBasedOnSkill();

        _levelGenerator.SetupLevel(tranningModelHandler);
    }
}
