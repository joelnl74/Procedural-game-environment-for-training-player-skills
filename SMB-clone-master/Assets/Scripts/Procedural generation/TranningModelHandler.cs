using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TranningModelHandler
{
    private TranningModel tranningModel;

    public TranningModelHandler()
    {
        tranningModel = new TranningModel();

        // TODO Load in from external file.
    }

    public TranningType GetTranningType()
        => tranningModel.GetCurrentTrannigType();
}
