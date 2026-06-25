using UnityEngine;

public class ControlState_Crawl : ControlState_Abs
{
    [SerializeField] public LayerMask surfaceMask;
    [SerializeField] float moveSpeed;
    [SerializeField] float rayLength;
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
        Vector3 moveDirection = (playerBody.transform.right * brain.moveInput.x) + (playerBody.transform.forward *  brain.moveInput.y);
        moveDirection = moveDirection.normalized * moveSpeed;

        brain.rb.MovePosition(brain.rb.position + moveDirection * Time.fixedDeltaTime);
    }
}
