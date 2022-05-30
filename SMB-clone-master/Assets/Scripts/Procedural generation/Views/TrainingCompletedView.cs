using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingCompletedView : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;

    private void Start()
    {
        SetView(false);

        PCGEventManager.Instance.onTrainingEnd += (() => 
        {
            SetView(true);
        });
    }

    private void OnDestroy()
    {
        
    }

    public void SetView(bool isActive)
    {
        canvasGroup.alpha = isActive ? 1 : 0;
        canvasGroup.interactable = isActive ? true : false;
        canvasGroup.blocksRaycasts = isActive ? true : false;
        gameObject.SetActive(isActive);
    }
}
