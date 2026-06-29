using System.Collections;
using UnityEngine;

public class TurnOnHazzardTrigger : MonoBehaviour
{
    [SerializeField] GameObject HazardTrigger;
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
            yield return new WaitForSeconds(Intervals);
            HazardTrigger.SetActive(true);
            yield return new WaitForSeconds(lastingTime);   
        }
    }
}
