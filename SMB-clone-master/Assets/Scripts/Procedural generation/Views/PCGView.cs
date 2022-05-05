using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PCGView : MonoBehaviour
{
    public Button RegenerateButton;

    [SerializeField] Text _goaltext;
    [SerializeField] Text _feedbackText;

    [SerializeField] private CanvasGroup _feedbackCanvas;

    [SerializeField] private Button _closeButton;
    [SerializeField] private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _feedbackCanvas.alpha = 0;
        _closeButton.onClick.AddListener(() => { ShowRegenerateDialog(false);});

        ShowRegenerateDialog(false);
    }

    public void SetGoals(List<TrainingType> tranningTypes)
    {
        var addedTypes = new List<TrainingType>();

        _goaltext.text = "";

        foreach(var goal in tranningTypes)
        {
            if(addedTypes.Contains(goal))
            {
                continue;
            }

            _goaltext.text += goal.ToString() + "\n";

            addedTypes.Add(goal);
        }
    }

    public void ShowTip(string text)
    {
        var sequence = DOTween.Sequence();

        sequence.SetLink(_feedbackCanvas.gameObject);
        sequence.Append(_feedbackCanvas.DOFade(1, 0.1f));
        sequence.AppendCallback(() =>
        {
            _feedbackText.text = text;
        });
        sequence.AppendInterval(2);
        sequence.Append(_feedbackCanvas.DOFade(0, 0.1f));
        sequence.Play();
    }

    public void ShowRegenerateDialog(bool show)
    {
        _canvasGroup.alpha = show == true ? 1 : 0;
        _canvasGroup.blocksRaycasts = show;
        _canvasGroup.interactable = show;
    }
}
