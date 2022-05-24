using System.Collections;
using UnityEngine;

public class FirebaseSignInView : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;

    private IEnumerator Start()
    {
        if (FirebaseManager.Instance.setup)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        yield return new WaitForSeconds(2);

        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        FirebaseManager.Instance.GetLeaderBoardAysncFromDataBase();
    }
}
