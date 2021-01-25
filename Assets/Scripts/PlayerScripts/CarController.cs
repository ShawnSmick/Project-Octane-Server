using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;




[RequireComponent(typeof(Suspension),typeof(Player))]
public class CarController : MonoBehaviour
{
    /*If this shit doesnt end up working I will commit mine <3 -shawn
    */
    #region Variables 
    

    private Player player;
    private Suspension suspension;
    [SerializeField]
    private float accelForce = 45000;//Force Accel of the car
    [SerializeField]
    private float turningTorque = 8000;//Turn Torque
    [SerializeField]
    [Tooltip("How much local vertical velocity is kept per physics tick, used in reducing bounce on the springs. only works while grounded")]
    private float stiffness = 7f;//decay of vertical momentum while grounded.
    [SerializeField]
    [Tooltip("How much local side velocity is kept per physics tick, higher values = more drift. . only works while grounded")]
    private float sideDrag = 4f;//Decay of sideways momentum while grounded
    [SerializeField]
    private float sideDragReduction = 3f;//Decay of sideways momentum while grounded
    [SerializeField]
    [Tooltip("How much local frontal velocity is kept per physics tick while accelerating, effects max speed. only works while grounded")]
    private float frontDrag = 1.25f;//decay of vertical momentum while grounded.
    [SerializeField]
    [Tooltip("How much local frontal velocity is kept per physics tick while you are not accelerating. only works while grounded")]
    private float frontStopDrag = 2.5f;
    [SerializeField]
    [Tooltip("Caps the fall speed, test throughly if you mess with this, high speed break collision.")]
    private float maxFallSpeed = 50f;
    [SerializeField]
    [Tooltip("How much the force pushing down on the suspention when moving forward or back.")]
    private float wheelOffset = .4f;
    private bool turnRight = false;
    public float turnTime = 0f;
    public float maxTurnTime = 0.8f;
    public float TurnMult = 2f;
    [SerializeField]
    private Transform centerOfMass;
    [SerializeField]
    private Transform Front, Back;
    public Dictionary<String,Debuff> Debuffs;
   
    private Vector3 _groundVector;
    private Rigidbody _rb;//penis joke
    private bool _grounded = false;
    public float collisionAmplitude = 2f;
    bool boosting = false;
    bool fastturn = false;
    #endregion

    void Start()
    {
        Debuffs = new Dictionary<string,Debuff>();
        _rb = GetComponent<Rigidbody>();
        _rb.centerOfMass = centerOfMass.localPosition;
        player = GetComponent<Player>();
        suspension = GetComponent<Suspension>();
    }

    // Update is called once per frame

    void FixedUpdate()
    {
        List<string> keys = new List<string>(Debuffs.Keys);
            foreach (string s in keys)
            {
                if(Debuffs[s].action!=null)
                    Debuffs[s].action(player.id,Time.fixedDeltaTime);
                Debuffs[s].time -= Time.fixedDeltaTime;
                if (Debuffs[s].time < 0)
                {
                    ServerSend.ToggleDebuff(player.id,s);
                    Debuffs.Remove(s);
                }
            }
            
        
        _Thrust();//( ͡° ͜ʖ ͡°)

    }
    public Vector3 getGroundForward()
    {
        return _groundVector;
    }
    public bool isGrounded()
    {
        return _grounded;
    }
    //Look into this
    /*private void OnCollisionEnter(Collision _other)
    {
        CarController _otherCar = _other.gameObject.GetComponent<CarController>();
        if (_otherCar!=null)
        {
            foreach(ContactPoint _contacts in _other.contacts)
            _other.rigidbody.AddForceAtPosition(((_rb.velocity * _rb.mass)/_other.contactCount)* collisionAmplitude,_contacts.point);
        }
    }*/
    private void _Thrust()
    {
        //float inputY = Input.GetAxis("Vertical");//TODO: Find a better way to wrap inputs, I fucking hate unity's default system
        // float inputX = Input.GetAxis("Horizontal");

        float inputY = player.getAccel();
        float inputX = player.getTurn();
        if (Mathf.Abs(inputX) > 0.3f && isGrounded())
        {
            if (inputX > 0)
            {
                if (turnRight == false)
                {
                    turnTime = 0;
                    turnRight = true;

                }
                else
                {
                    turnTime += Time.fixedDeltaTime*((fastturn)?2f:1f);
                }
                
            }
            else
            {
                if (turnRight == true)
                {
                    turnTime = 0;
                    turnRight = false;
                }
                else
                {
                    turnTime += Time.fixedDeltaTime * ((fastturn) ? 2f : 1f);
                }
              

            }
        }
        else
        {
            turnTime = 0;
        }
        bool[] _inputs = player.getInputs();
        bool[] _oldinputs = player.getInputs();
        if (boosting)
        {
            player.Turbo -= 1f * Time.fixedDeltaTime;
            if (player.Turbo < 0)
                player.Turbo = 0;
        }
        if (_inputs != null)
        {
            ServerSend.SetTurbo(player.id,player.Turbo);
            boosting = player.Turbo > 0 ? _inputs[4] : false;
            
            fastturn = _inputs[5];
        }
        if (player.Turbo < Mathf.Epsilon)
        {
            boosting = false;
        }
        //Check to see if the car is more or less touching the ground
        RaycastHit ground, ground2;
        Debug.DrawRay(Front.position,-Front.transform.up);
        Debug.DrawRay(Back.position,-Back.transform.up);

        //if (Physics.Raycast(Front.position,-Front.transform.up,out ground,suspension.groundDistance) && Physics.Raycast(Back.position,-Back.transform.up,out ground2,suspension.groundDistance))
        //{
        //    _grounded = true;

        //    //here we are basically looking at the line made by the ground at the front of the car and the back of it to get the angle of the ground.

        //    _groundVector = ground.point - ground2.point;
        //    _groundVector.Normalize();


        //    //_VelocityDecay((1f-(sideDrag-(fastturn?sideDragReduction:0)))*Time.deltaTime,1f-stiffness * Time.deltaTime,1f-(inputY != 0 ? frontDrag * Time.deltaTime : frontStopDrag * Time.deltaTime));
        //    if ((maxTurnTime > turnTime))
        //    {

        //        _VelocityDecay(1f - (sideDrag - (fastturn ? sideDragReduction : 0)) * Time.deltaTime,1f - stiffness * Time.deltaTime,1f - (inputY != 0 ? frontDrag * Time.deltaTime : frontStopDrag * Time.deltaTime));

        //        _rb.AddForce(_groundVector * accelForce * inputY * (boosting ? 1.5f : 1) / (fastturn ? 1.5f : 1));//Apply forward thrust to the car, increase that by 50% if you are boosting that value subject to change. force is applied parallel to the ground.
        //    }
        //    else if(turnTime > maxTurnTime+1f)
        //    {

        //        _VelocityDecay(1f - (sideDrag) * Time.deltaTime,1f - stiffness * Time.deltaTime,1f - (frontStopDrag) * Time.deltaTime);

        //        _rb.AddForce(_groundVector * accelForce * inputY * (boosting ? 1.5f : 1) / (fastturn ? 1.75f : 1.25f));
        //    }
        //    suspension.AnimateMovement(inputX,inputY,boosting);//Do the silly car leaning thing

        //}
        //else
        //{
        //    _grounded = false;
        //    _FallClamp();
        //}
       
        _rb.AddRelativeTorque(Vector3.up * turningTorque * (fastturn ? 1.5f : 1) * inputX);//Spin the Car

    }

    private void _FallClamp()
    {
        Vector3 vel = transform.InverseTransformDirection(_rb.velocity); //Convert the rigidbody's world velocity into a local space            
        vel = new Vector3(vel.x,Mathf.Clamp(vel.y,-maxFallSpeed,Mathf.Infinity),vel.z); //Clamp the -y speed to cap the fall speed
        _rb.velocity = transform.TransformDirection(vel);//Reapply velocity in the world space 
    }

    private void _VelocityDecay(float decX,float decY,float decZ)
    {
        Vector3 vel = transform.InverseTransformDirection(_rb.velocity); //Convert the rigidbody's world velocity into a local space
        vel = new Vector3(vel.x * decX,vel.y * decY,vel.z * decZ); //Decay the local sideways velocity
        _rb.velocity = transform.TransformDirection(vel); //Convert and update local velocity to the world
    }

    

   
}

