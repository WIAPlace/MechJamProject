using UnityEngine;

public class ControlState_Crawl : ControlState_Abs
{
    [SerializeField] public LayerMask surfaceMask;
    [SerializeField] float moveSpeed;
    [SerializeField] float rayLength;
    public Transform cameraTransform;
    public float gravityStrength = 30f;
    public float rotationSpeed = 12f; // Increased for sharp turns
    
    private bool crawling = false;

    
    /////////////////////////////////// DO ENTER
    public override void DoEnter()
    {
        crawling = true;
    }
    /////////////////////////////////// DO EXIT
    public override void DoExit()
    {
        crawling = false;
    }
    /////////////////////////////////// DO STATE
    public override ControlState_Abs DoState()
    {
        //RaycastDown(); // stick to surface 
        return this;
    }

    void FixedUpdate()
    {
        if(crawling){
            brain.GetSurfaceNormal();
            brain.ApplyCustomGravity();
            brain.SmoothLookRotation();
            Move();
            //RotateToVelocity();
        }
    }



    private void RaycastDown()
    {
        // Cast a ray downwards relative to the object's local rotation
        if (Physics.Raycast(playerBody.transform.position, -playerBody.transform.up, out RaycastHit hit, rayLength, surfaceMask))
        {
            // Move to surface point (plus a tiny offset to prevent clipping)
            playerBody.transform.position = hit.point + (hit.normal * 0.1f);

            // Align rotation to surface normal
            Quaternion targetRotation = Quaternion.FromToRotation(playerBody.transform.up, hit.normal) * playerBody.transform.rotation;
            playerBody.transform.rotation = Quaternion.Slerp(playerBody.transform.rotation, targetRotation, Time.deltaTime * 15f);
        }
    }

    void Move()
    {
        // Calculate surface-relative "Right" vector
        Vector3 surfaceRight = Vector3.ProjectOnPlane(cameraTransform.right, brain.surfaceNormal).normalized;

        // Calculate surface-relative "Forward" vector 
        // This ensures 'W' always points up on walls, or forward on flat ground
        Vector3 surfaceForward = Vector3.ProjectOnPlane(cameraTransform.up, brain.surfaceNormal).normalized;
        
        // Alternative variant: if camera up feels weird on flat ground, use camera forward projected
        /*
        if (Vector3.Dot(brain.surfaceNormal, Vector3.up) > 0.7f) // If mostly flat ground
        {
            surfaceForward = Vector3.ProjectOnPlane(cameraTransform.forward, brain.surfaceNormal).normalized;
        }
        */
        //Vector3 camForward = cameraTransform.forward;
        //Vector3 camRight = cameraTransform.right;

        Vector3 moveDirection = (surfaceRight* brain.moveInput.x) + (surfaceForward *  brain.moveInput.y);
        if (moveDirection.magnitude > 0.1f)
        {
            // FORCE VELOCITY ALONG THE SURFACE
            // Project the movement direction completely flat onto the surface normal
            Vector3 surfaceMoveDir = Vector3.ProjectOnPlane(moveDirection, brain.surfaceNormal).normalized;
            
            // Re-apply velocity entirely along the slope, killing outward momentum
            brain.rb.linearVelocity = surfaceMoveDir * moveSpeed;
            
            // Optional: Rotate bug's visuals to look in the direction of movement
            Quaternion lookRot = Quaternion.LookRotation(moveDirection, brain.surfaceNormal);
            playerBody.transform.rotation = Quaternion.Slerp(playerBody.transform.rotation, lookRot, rotationSpeed * Time.fixedDeltaTime);
        }
        else
        {
            // Stop moving if no input, but keep gravity pull
            Vector3 normalVelocity = Vector3.Project(brain.rb.linearVelocity, brain.surfaceNormal);
            brain.rb.linearVelocity = Vector3.MoveTowards(brain.rb.linearVelocity, normalVelocity, moveSpeed * Time.fixedDeltaTime);
        }
    }

    void RotateToVelocity()
    {
        if (brain.rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            // Calculate the target rotation based on velocity
            Quaternion targetRotation = Quaternion.LookRotation(brain.rb.linearVelocity, Vector3.up);
            
            // OPTION 1: Smooth rotation (Recommended)
            brain.rb.MoveRotation(Quaternion.Slerp(brain.rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
            
            // OPTION 2: Instant rotation (Uncomment below and comment out Option 1 if preferred)
            // rb.MoveRotation(targetRotation);
        }
    }
}
