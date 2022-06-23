using UnityEngine;
using UnityEngine.UI;

public class TrainingCompletedView : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Text _dfficultyVersionOne;
    [SerializeField] private Text _difficultyVersionTwo;

    private void Start()
    {
        SetView(false, null);

        PCGEventManager.Instance.onTrainingEnd += (() => 
        {
            SetView(true, FirebaseManager.Instance.GetGlobalResults());
        });
    }

    private void OnDestroy()
    {
        
    }

    public void SetView(bool isActive, GlobalPlayerResults data)
    {
        if (data != null)
        {
            _dfficultyVersionOne.text = $"Version one: { data.HighestScoreVersionOne}";
            _difficultyVersionTwo.text = $"Version two: { data.HighestScoreVersionTwo}";
        }

        canvasGroup.alpha = isActive ? 1 : 0;
        canvasGroup.interactable = isActive ? true : false;
        canvasGroup.blocksRaycasts = isActive ? true : false;
        gameObject.SetActive(isActive);
    }
}
