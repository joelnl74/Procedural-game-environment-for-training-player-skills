using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PCGView : MonoBehaviour
{
    [SerializeField] Text _Goaltext;

    public void SetGoals(List<TranningType> tranningTypes)
    {
        _Goaltext.text = "";

        foreach(var goal in tranningTypes)
        {
            _Goaltext.text += goal.ToString() + "\n";
        }
    }
}
