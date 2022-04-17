using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HasPlayedBeforeView : MonoBehaviour
{
    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _noButton;
    [SerializeField] private Button _quitButton;

    [SerializeField] private bool deleteSave;

    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private SkillsCollectionConfiguration skillsCollection;

    private SerializeData _serializeData;

    // Start is called before the first frame update
    private void Start()
    {
        _serializeData = new SerializeData();

        if (deleteSave)
        {
            _serializeData.DeleteSave();
        }
        if (_serializeData.ContainsSkippedTutorial())
        {
            Disable();
        }
        else
        {
            Enable();
        }

        _continueButton.onClick.AddListener(OnContinuePressed);
        _noButton.onClick.AddListener(NoButtonPressed);
        _quitButton.onClick.AddListener(OnQuitButtonPreseed);
    }

    private void OnDestroy()
    {
        _continueButton?.onClick.RemoveAllListeners();
        _noButton?.onClick.RemoveAllListeners();
        _quitButton.onClick.RemoveAllListeners();
    }

    private void OnContinuePressed()
    {
        _serializeData.SetSkippedTutorial(true);
        _serializeData.SaveData(new Dictionary<int, ChunkInformation>
        {
            { 1, new ChunkInformation
            {
                index = skillsCollection.skillParameters.Count,
                completedChunk = true,
            }
            }
        });

        Disable();
    }

    private void OnQuitButtonPreseed()
    {
        Application.Quit();
    }

    private void NoButtonPressed()
    {
        _serializeData.SetSkippedTutorial(false);
        Disable();
    }

    private void Enable()
    {
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }

    private void Disable()
    {
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }
}
