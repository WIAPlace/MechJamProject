using UnityEngine;

public class Controls_Brain : MonoBehaviour
{
    [field: SerializeField]
    public InputReader input {get;private set;} // putting this as public so i dont have to plug it in to every control state.
    [field:SerializeField, Tooltip("Player Body")]
    public GameObject playerBody{get;private set;} 


    private ControlState_Abs currentState;
    private ControlState_Abs previousState;

    // used for seeing what state we are in in the  inspector
    public string debugCurrentStateName;
    public string debugPreviousStateName;

    // States
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (currentState != null)
        {
            ControlState_Abs tempCheck = currentState.DoState();
            if(currentState != tempCheck) 
            { // using this as a of being able to utilize change state instead of just changing current state dirrectly
                ChangeState(tempCheck);
            }
            debugCurrentStateName = currentState.GetType().Name; //used for debuging to see name
            debugPreviousStateName = previousState?.GetType().Name; //used for debuging to see name
        }
    }

    /////////////////////////////////////////////////////////////// Chanage State
    public void ChangeState(ControlState_Abs newState)
    {
        previousState = currentState;
        currentState?.DoExit(); // leave the prevvious state
        currentState = newState;
        currentState?.DoEnter(); // enter the new state   
    }


}
