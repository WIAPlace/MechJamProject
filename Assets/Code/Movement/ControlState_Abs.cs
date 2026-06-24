using UnityEngine;

// base class of all controll states
public class ControlState_Abs : MonoBehaviour
{
    // Put fixed variables
    protected Controls_Brain brain; // refrence to the controler script.
    protected InputReader input; // refrence to the input script.
    protected GameObject playerBody;

    void OnEnable() // get this at the start because all of them will need a refrence to it.
    {
        StartupStuff();
    }

    
    private void OnDestroy()
    {   // this is used for unsubscribing to events
        DoExit();
    }

    /////////////////////////////////// DO ENTER
    public virtual void DoEnter()
    {
        // When the state begins.
    }
    /////////////////////////////////// DO EXIT
    public virtual void DoExit()
    {
        // When the state is over.
    }
    /////////////////////////////////// DO STATE
    public virtual ControlState_Abs DoState()
    {
        return this;
    }

    protected void StartupStuff()
    {
        brain = gameObject.GetComponent<Controls_Brain>();
    }
}
