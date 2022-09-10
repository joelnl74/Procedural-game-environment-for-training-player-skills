using System.Collections;
using UnityEngine;

public class FirebaseSignInView : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private bool skipSetup;

    private bool calledSetup;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(2);

        if (skipSetup)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    private void Update()
    {
        if (FirebaseManager.Instance.setup && calledSetup == false)
        {
            FirebaseManager.Instance.GetPlayerDataFromDataBase();

            calledSetup = true;
        }

        if (calledSetup)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
