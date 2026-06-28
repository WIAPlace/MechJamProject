using UnityEngine;
using Unity.Cinemachine.Samples;
//using System.Runtime.InteropServices;

public class NewAnimationController : MonoBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] SimplePlayerControllerBase m_Controller;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_Controller.PostUpdate += HandleMove;
        m_Controller.Eat += HandleEat;
    }
    void OnDestroy()
    {
        m_Controller.PostUpdate -= HandleMove;
        m_Controller.Eat -= HandleEat;
    }
    // Update is called once per frame
    void Update()
    {
        
    }


    private void HandleMove(Vector3 vel, float jump)
    {
        //wanim.ResetTrigger("EatTrigger");
        //Debug.Log("Pre Y=0 : "+vel.magnitude);
        vel.y = 0; // we don need vertical velocity

        float velocity = vel.magnitude;

        if (Mathf.Abs(velocity) < 0.0001f)
        {
            velocity = 0;
        }

        //Debug.Log("Post Y= : "+vel.magnitude);
        anim.SetFloat("Velocity",velocity);
    }
    private void HandleEat()
    {
        anim.SetTrigger("EatTrigger");
    }
}
