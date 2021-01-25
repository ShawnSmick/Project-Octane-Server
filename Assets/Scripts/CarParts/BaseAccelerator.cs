using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseAccelerator : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Forward force exerted when accelerating")]
    private float accelForce = 45000;//Force Accel of the car
    [Tooltip("Forward force exerted when accelerating")]
    private float boostMultiplier = 1.5f;//Force Accel of the car
    private CarEngine parent;

    private void Start()
    {
        parent = GetComponent<CarEngine>();
    }
    public void Accelerate()
    {
        int traction = parent.GetTraction();
        //get base forward force in direction of the ground beneath player
        Vector3 force = parent.GetGroundVect() * accelForce * parent.GetInputY();
        //if boosting increase force by boost multiplier
        force *= (parent.IsBoosting() ? boostMultiplier : 1);
        if (traction >= 2)
        {
            //Apply forward thrust to the car, if fastturning reduce the power
            parent.getRigidBody().AddForce(force / (parent.IsFastTurn() ? 1.5f : 1));
        } else if (traction == 1)
        {
            // lower traction reduces the power further, may make the fast turn status factor into traction?
            parent.getRigidBody().AddForce(force / (parent.IsFastTurn() ? 1.75f : 1.25f));
        }
    }
}

