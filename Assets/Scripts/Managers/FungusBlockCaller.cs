using UnityEngine;
using Fungus;

public class FungusBlockCaller : MonoBehaviour
{
    public Flowchart flowchart;

    public void CallEnding1()
    {
        if (flowchart == null)
        {
            Debug.LogError("FungusBlockCaller: Flowchart reference is not assigned.");
            return;
        }

        if (!flowchart.HasBlock("Ending1"))
        {
            Debug.LogError("FungusBlockCaller: Ending1 block not found on the assigned Flowchart.");
            return;
        }

        flowchart.ExecuteBlock("Ending1");
    }
    public void CallEnding2()
    {
        if (flowchart == null)
        {
            Debug.LogError("FungusBlockCaller: Flowchart reference is not assigned.");
            return;
        }

        if (!flowchart.HasBlock("Ending2"))
        {
            Debug.LogError("FungusBlockCaller: Ending2 block not found on the assigned Flowchart.");
            return;
        }

        flowchart.ExecuteBlock("Ending2");
    }
}
