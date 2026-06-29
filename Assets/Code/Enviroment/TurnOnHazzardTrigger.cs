using System.Collections;
using UnityEngine;

public class TurnOnHazzardTrigger : MonoBehaviour
{
    [SerializeField] GameObject HazardTrigger;
    [SerializeField] ParticleSystem ParticleEffect;
    [SerializeField] float Intervals;
    [SerializeField] float lastingTime;
    private bool activeState = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(TurnOnAndOff());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator TurnOnAndOff()
    {
        while (activeState)
        {
            HazardTrigger.SetActive(false);
            ParticleEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            yield return new WaitForSeconds(Intervals);
            HazardTrigger.SetActive(true);
            ParticleEffect.Play();
            yield return new WaitForSeconds(lastingTime);   
        }
    }
}
