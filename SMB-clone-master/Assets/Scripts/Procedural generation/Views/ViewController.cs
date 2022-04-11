using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewController : MonoBehaviour
{
    [SerializeField] private PCGView _view;

    private PCGEventManager _PCGEventManager;
    private int chunkId;

    private void Awake()
    {
        _PCGEventManager = PCGEventManager.Instance;
        _PCGEventManager.onTranningGoalsGenerated += OnReceivedTranningTypes;
        _PCGEventManager.onShowMessage += OnShowMessageReceived;
        _PCGEventManager.onShowRegenerateLevelMessage += OnShowRegenerateLevelMessageReceived;

        _view.RegenerateButton.onClick.AddListener(OnRegenerateButtonInvoked);
    }

    private void OnDestroy()
    {
        if (_PCGEventManager != null)
        {
            _PCGEventManager.onTranningGoalsGenerated -= OnReceivedTranningTypes;
            _PCGEventManager.onShowMessage -= OnShowMessageReceived;
            _PCGEventManager.onShowRegenerateLevelMessage -= OnShowRegenerateLevelMessageReceived;
        }
    }

    private void OnReceivedTranningTypes(List<TranningType> tranningTypes)
    {
        _view.SetGoals(tranningTypes);
    }

    private void OnShowMessageReceived(string text)
    {
        _view.ShowTip(text);
    }

    private void OnShowRegenerateLevelMessageReceived(int id)
    {
        chunkId = id;
        _view.ShowRegenerateDialog(true);
    }

    private void OnRegenerateButtonInvoked()
    {
        _PCGEventManager.onRegenerateLevelSelected(chunkId);
        _view.ShowRegenerateDialog(false);
    }
}
