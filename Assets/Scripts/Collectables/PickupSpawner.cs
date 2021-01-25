using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupSpawner : MonoBehaviour
{
    // Start is called before the first frame update
    public Pickup pickup;
    private Pickup child;
    public float spawnTime = 5;
    private float timeElapsedSincePickup = 0;
    bool spawned = false;
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (child == null)
        {
            timeElapsedSincePickup += Time.fixedDeltaTime;
            if (timeElapsedSincePickup > spawnTime || spawned == false)
            {
                //Debug.Log("POP!");
                spawned = true;
                child = NetworkManager.instance.InstantiatePickup(pickup,transform.position);
                timeElapsedSincePickup = 0;
            }
        }
    }
}
