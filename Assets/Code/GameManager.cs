using System;
using Unity.Cinemachine.Samples;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Declare the static instance variable
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        // Check if an instance already exists
        if (Instance != null && Instance != this)
        {
            // Destroy this duplicate object immediately
            Destroy(gameObject);
            return;
        }

        // Assign this object as the unique instance
        Instance = this;
    }

    [SerializeField] public SimplePlayerController player;
    [SerializeField] public LayerMask playerMask;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
