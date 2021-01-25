using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseSteering : MonoBehaviour
{
    [SerializeField]
    private float turningTorque = 8000;//Turn Torque
    //cant think of a smarter thing to name this
    private bool turningRight = false;
    [SerializeField]
    private float skidThreshold = .5f;
    [SerializeField]
    private float skidTime = 0f;
    [SerializeField]
    private float startSkidTime = 0.8f;
    [SerializeField]
    private float maxSkidTime = 0.8f;
    [SerializeField]
    private float TurnMult = 2f;
    private CarEngine parent;

    void Start()
    {
        parent = GetComponent<CarEngine>();
       
    }
    public void Turn()
    {
        calculateTraction();
        parent.getRigidBody().AddRelativeTorque(Vector3.up * turningTorque * (parent.IsFastTurn() ? 1.5f : 1) * parent.GetInputX());//Spin the Car
    }
    public void calculateTraction()
    {
        //if turning more than the skid threshold add to skidding time
        if (Mathf.Abs(parent.GetInputX()) > skidThreshold && parent.IsGrounded())
        {

            if (parent.GetInputX() > 0)
            {
                applySkid(turningRight == false);
                turningRight = true;
            }
            else
            {
                applySkid(turningRight == true);
                turningRight = false;
            }
        }
        else
        {
            skidTime = 0;
        }

        //as long as you have not skid longer then the start skid time you still have traction
        if ((startSkidTime > skidTime))
        {
            parent.SetTraction(2);
        }
        //once you have skidded longer than the startskid + maxskid time you regain traction
        else if (skidTime > startSkidTime + maxSkidTime)
        {
            parent.SetTraction(1);
        }
        //between startskid and startskid+maxskid you have no traction!
        else
        {
            parent.SetTraction(0);

        }
            
    }
    public void applySkid(bool sameDir)
    {
        //if you are not turning in the same direction as you started skidding reset skid
        if (sameDir)
        {
            skidTime = 0;
        }
        else
        {
            skidTime += Time.fixedDeltaTime * ((parent.IsFastTurn()) ? 2f : 1f);
        }
    }
}
