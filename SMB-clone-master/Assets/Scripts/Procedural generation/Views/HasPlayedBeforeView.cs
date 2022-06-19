using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HasPlayedBeforeView : MonoBehaviour
{
    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _noButton;
    [SerializeField] private Button _quitButton;

    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private SkillsCollectionConfiguration skillsCollection;

    private SerializeData _serializeData;
    private FirebaseManager _firebaseManager;

    private bool skippedTutorialSet = false;

    // Start is called before the first frame update
    private IEnumerator Start()
    {
        _serializeData = new SerializeData();
        _firebaseManager = FirebaseManager.Instance;

        Disable();

        skippedTutorialSet = _serializeData.ContainsSkippedTutorial();

        if (skippedTutorialSet)
        {
            Disable();

            yield break;
        }

        yield return new WaitForSeconds(5);
    }

    private void Update()
    {
        if (FirebaseManager.Instance.setup && skippedTutorialSet == false)
        {
            NoButtonPressed();

            skippedTutorialSet = true;
        }
    }

    private void OnDestroy()
    {
        _continueButton?.onClick.RemoveAllListeners();
        _noButton?.onClick.RemoveAllListeners();
        _quitButton.onClick.RemoveAllListeners();
    }

    private void OnQuitButtonPreseed()
    {
        Application.Quit();
    }

    private void NoButtonPressed()
    {
        _serializeData.SetSkippedTutorial(false);
        _firebaseManager.SetSkippedTutorial(false);
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
