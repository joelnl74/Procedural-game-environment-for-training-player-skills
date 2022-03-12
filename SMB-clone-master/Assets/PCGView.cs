using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using DG;

public class PCGView : MonoBehaviour
{
    [SerializeField] Text _goaltext;
    [SerializeField] Text _feedbackText;

    [SerializeField] private CanvasGroup _feedbackCanvas;

    private void Awake()
    {
        _feedbackCanvas.alpha = 0;
    }

    public void SetGoals(List<TranningType> tranningTypes)
    {
        var addedTypes = new List<TranningType>();

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
}
