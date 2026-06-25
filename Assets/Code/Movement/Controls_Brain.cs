using System;
using UnityEngine;

public class Controls_Brain : MonoBehaviour
{
    [field: SerializeField]
    public InputReader input {get;private set;} // putting this as public so i dont have to plug it in to every control state.
    [field:SerializeField, Tooltip("Player Body")]
    public GameObject playerBody{get;private set;} 
    public Rigidbody rb;

    private ControlState_Abs currentState;
    private ControlState_Abs previousState;

    // used for seeing what state we are in in the  inspector
    public string debugCurrentStateName;
    public string debugPreviousStateName;

    // States
    [SerializeField] private ControlState_Crawl crawlState; 


    [HideInInspector] public Vector2 moveInput;

    [Header("Raycast Distances")]
    public float downDistance = 1.2f;
    public float forwardDistance = 1.0f;

    private Vector3 surfaceNormal;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        input.MoveEvent += HandleMove;

        rb.useGravity = false;
        rb.freezeRotation = true;
        surfaceNormal = transform.up;

        ChangeState(crawlState);
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

    public void GetSurfaceNormal()
    {
        RaycastHit hit;

        // 1. Check Forward for Inner 90-Degree Corners
        Vector3 forwardDir = playerBody.transform.forward;
        if (Physics.Raycast(playerBody.transform.position, forwardDir, out hit, forwardDistance, crawlState.surfaceMask))
        {
            surfaceNormal = hit.normal;
            return; // Prioritize the wall in front
        }

        // 2. Check Down for Floor/Ceiling
        Vector3 downDir = -playerBody.transform.up;
        if (Physics.Raycast(playerBody.transform.position, downDir, out hit, downDistance, crawlState.surfaceMask))
        {
            surfaceNormal = hit.normal;
            return;
        }

        // 3. Check Angled Down-Forward for Outer Ledges
        Vector3 angledDir = (-playerBody.transform.up + playerBody.transform.forward).normalized;
        if (Physics.Raycast(playerBody.transform.position, angledDir, out hit, downDistance * 1.5f, crawlState.surfaceMask))
        {
            surfaceNormal = hit.normal;
        }
    }

    public void ApplyCustomGravity()
    {
        rb.AddForce(-surfaceNormal * crawlState.gravityStrength, ForceMode.Acceleration);
    }

    public void SmoothLookRotation()
    {
        // Calculate target rotation matching the surface normal
        Quaternion targetRot = Quaternion.FromToRotation(playerBody.transform.up, surfaceNormal) * playerBody.transform.rotation;
        
        // Prevent unwanted yaw spinning during alignment
        Vector3 forward = playerBody.transform.forward;
        Vector3.OrthoNormalize(ref surfaceNormal, ref forward);
        targetRot = Quaternion.LookRotation(forward, surfaceNormal);

        rb.MoveRotation(Quaternion.Slerp(playerBody.transform.rotation, targetRot, Time.fixedDeltaTime * crawlState.rotationSpeed));
    }

    void HandleMove(Vector2 moveAxis)
    {
        moveInput=moveAxis;
    }


}
