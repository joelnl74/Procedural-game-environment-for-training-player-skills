using System.Collections;
using UnityEngine;

public class FirebaseSignInView : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;

    private bool calledSetup;

    private IEnumerator Start()
    {
        if (FirebaseManager.Instance.setup)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        yield return new WaitForSeconds(2);
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
