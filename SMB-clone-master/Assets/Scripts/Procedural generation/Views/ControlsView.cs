using UnityEngine;

public class ControlsView : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Input.anyKey)
        {
            DestoryView();
        }
    }

    private void DestoryView()
    {
        Destroy(gameObject);
    }
}
