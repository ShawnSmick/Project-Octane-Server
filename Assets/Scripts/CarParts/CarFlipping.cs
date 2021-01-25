using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarFlipping : MonoBehaviour
{
    
    Rigidbody carRigidBody ;
    private float timeSinceLastFlipped = 0;

    [SerializeField]
    private float flippingForce = 10000;
    [SerializeField]
    private float flippingTime = 2.5f;

    private void Start()
    {
        carRigidBody = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {

        //get the car Z angle
        float zAngle = transform.rotation.eulerAngles.z;

        //if a car is flipped more than 80 degrees in either direction flip it upright!
        if (zAngle >= 80 && zAngle <= 280)
        {     
            timeSinceLastFlipped += Time.deltaTime;
            if (timeSinceLastFlipped >= flippingTime)
            {
                //apply flip force until flipped
                carRigidBody.AddRelativeTorque(Vector3.forward * (zAngle > 180 ? flippingForce : -flippingForce));
            }
        }else{
            timeSinceLastFlipped = 0;
        }

    }
}
