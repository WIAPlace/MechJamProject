using Unity.Cinemachine.Samples;
using UnityEngine;
using UnityEngine.Animations;

public class EatTriggerZone : MonoBehaviour
{
    private SimplePlayerController player;
    private LayerMask playerMask;
    private bool playerIn = false;
    public GameObject particlePrefab;
    public GameObject item;
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
            SpawnAlignedParticles();
            Destroy(item);
            Destroy(transform.parent.gameObject,1.2f);
        }
    }

    void SpawnAlignedParticles()
    {
        // Aligns the particle system's Z-axis (forward) with the object's forward direction
        Quaternion spawnRotation = Quaternion.LookRotation(Vector3.up, transform.forward);

        // Instantiate the prefab at your current position with the new forward alignment
        GameObject fxInstance = Instantiate(particlePrefab, transform.position, spawnRotation);
        Destroy(fxInstance,1.1f);
    }
}
