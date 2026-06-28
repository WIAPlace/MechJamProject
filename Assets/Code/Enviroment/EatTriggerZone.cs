using Unity.Cinemachine.Samples;
using UnityEngine;

public class EatTriggerZone : MonoBehaviour
{
    private SimplePlayerController player;
    private LayerMask playerMask;
    private bool playerIn = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameManager.Instance.player;
        playerMask = GameManager.Instance.playerMask;

        player.Eat += HandleEat;
    }
    void OnDestroy()
    {
        player.Eat -= HandleEat;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if ((playerMask.value & (1 << other.gameObject.layer)) != 0)
        {
           playerIn = true;
        }
    }
    void OnTriggerExit(Collider other)
    {
        if ((playerMask.value & (1 << other.gameObject.layer)) != 0)
        {
           playerIn = false;
        }
    }

    private void HandleEat()
    {
        if (playerIn)
        {
            Destroy(gameObject);
        }
    }
}
