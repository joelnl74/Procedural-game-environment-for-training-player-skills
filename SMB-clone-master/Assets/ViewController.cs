using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewController : MonoBehaviour
{
    [SerializeField] private PCGView _view;

    private PCGEventManager _PCGEventManager;

    private void Awake()
    {
        _PCGEventManager = PCGEventManager.Instance;
        _PCGEventManager.onTranningGoalsGenerated += OnReceivedTranningTypes;
        _PCGEventManager.onCompleted += OnCompletedChunk;
        _PCGEventManager.onFailed += OnFailedChunk;

    }

    private void OnDestroy()
    {
        if (_PCGEventManager != null)
        {
            _PCGEventManager.onTranningGoalsGenerated -= OnReceivedTranningTypes;
        }
    }

    private void OnReceivedTranningTypes(List<TranningType> tranningTypes)
    {
        _view.SetGoals(tranningTypes);
    }
    private void OnCompletedChunk(string text)
    {
        _view.ShowTip(text);
    }

    private void OnFailedChunk(string text)
    {
        _view.ShowTip(text);
    }

}
