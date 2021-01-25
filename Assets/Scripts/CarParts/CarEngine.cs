using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class CarEngine : Destructable
{
    /*
     * Private Inspector Fields 
     */   
    [SerializeField]
    [Tooltip("Set the players maximum energy value, probably shouldn't change this?")]
    private float maxEnergy = 100f;
    [SerializeField]
    [Tooltip("Energy Recharge Rate per second, probably shouldn't change this?")]
    private float energyRegenRate = 1f;
    [SerializeField]
    [Tooltip("Set the players maximum energy value, probably shouldn't change this?")]
    private float maxTurbo = 100f;
    [SerializeField]
    [Tooltip("Upwards Velocity set but AA Jump")]
    public float jumpVelocity = 10f;
    [Tooltip("How far from the ground you can be and still be considered grounded")]
    private float maxGroundDistance = .6f;//How from the ground the car still does car things
    [SerializeField]
    [Tooltip("Front of Vehicle for determining ground angle")]
    private Transform frontOfVehicle;
    [SerializeField]
    [Tooltip("Back of Vehicle for determining ground angle")]
    private Transform backOfVehicle; //These are where the front and back of the vehicle are used in getting ground vector TODO: possible add a pair in front and back for the ground vector of each of those points.


    //Vehicle Statuses
    private MissileRack missileRack;
    private MachineGun machineGun;
    private Rigidbody carRigidBody; 
    private User parent; //What user owns me
    private int traction = 2;
    private float turbo;
    private float energy;
    private bool grounded = false;
    private float maxFallSpeed = 300f;
    //CONTROLS
    private bool[] oldInputs = new bool[32],inputs = new bool[32];
    private float inputX = 0; //Turning Direction and Amount
    private float inputY = 0; //Acceleration Direction and Amount
    private Vector3 groundVector; //Angle of the Ground under the vehicle
    //Unity Control Events TODO: do research into the overhead caused by events
    public UnityEvent Accelerating, Turning, Drag;

    public new void Start()
    {
        health = maxHealth;
        missileRack = GetComponent<MissileRack>();
        machineGun = GetComponent<MachineGun>();
        carRigidBody = GetComponent<Rigidbody>();
        turbo = maxTurbo;
        energy = maxEnergy;
    }
    public void Initialize(User user)
    {
        parent = user;
    }

    //Brains of the whole Car Engine
    public void FixedUpdate()
    {
        if (health > 0)
        {

            EnergyRegen();
            TurboBurn();
            CheckGrounding();
            if (grounded)
            {
                Drag.Invoke(); //Use attached drag module(s)
                if (inputY != 0)
                {
                    Accelerating.Invoke();//Use attached acceleration module(s)
                }
            }

            Turning.Invoke();//Use attached turning module(s)
            if (inputs[6])
            {
                missileRack.FireMissile();
            }
            if (inputs[7])
            {
                machineGun.Fire();
            }
        }
        ServerSend.CarPosition(this,1);
    }

    /// <summary>
    /// burns through turbo while boosting at a rate of 1 per second
    /// </summary>
    /// <param name="force">forces the turbo burn</param>
    public void TurboBurn(bool force = false)
    {
        if (IsBoosting()||force)
        {
            turbo -= 1f * Time.fixedDeltaTime;
            if (turbo < 0)
                turbo = 0;
        }
    }
    /// <summary>
    /// Refills the energy back to max at a rate of _energyRegenRate per second
    /// </summary>
    public void EnergyRegen()
    {
        if (energy < maxHealth)
        {
            energy += energyRegenRate * Time.fixedDeltaTime;
            if (energy > maxHealth){ energy = maxHealth;}           
            ServerSend.SetEnergy(parent.GetID(),Mathf.RoundToInt(energy));
        }
    }
    /// <summary>
    /// check if there is enough energy to cover energy cost
    /// then spends that energy
    /// </summary>
    /// <param name="_energy">energy cost of action</param>
    /// <returns>whether or not energy could be spent</returns>
    public bool SpendEnergy(int energy)
    {
        if (this.energy >= energy)
        {
            this.energy -= energy;
            return true;
        }
        return false;
    }
    /// <summary>
    /// Checks whether or not the vehicle is considered grounded
    /// then stores the angle of the ground for later use.
    /// </summary>
    public void CheckGrounding()
    {
        RaycastHit ground, ground2; //Stores positional data of where the ground rays hit
        //Checks if both the front and back of the vehicles are within distance to be considered grounded
        if (Physics.Raycast(frontOfVehicle.position,-frontOfVehicle.transform.up,out ground,maxGroundDistance) 
            && Physics.Raycast(backOfVehicle.position,-backOfVehicle.transform.up,out ground2,maxGroundDistance))
        {
            grounded = true;
            //Debugging rays
            Debug.DrawRay(frontOfVehicle.position,-frontOfVehicle.transform.up);
            Debug.DrawRay(backOfVehicle.position,-backOfVehicle.transform.up);
            //Calculate the angle of the ground beneath the car
            //This is used both for getting missile angles as angle of acceleration
            groundVector = ground.point - ground2.point;
            groundVector.Normalize();
        }
        else{
            grounded = false;
            FallClamp();
        }
    }
    private void FallClamp()
    {
        Vector3 vel = transform.InverseTransformDirection(carRigidBody.velocity); //Convert the rigidbody's world velocity into a local space            
        vel = new Vector3(vel.x,Mathf.Clamp(vel.y,-maxFallSpeed,Mathf.Infinity),vel.z); //Clamp the -y speed to cap the fall speed
        carRigidBody.velocity = transform.TransformDirection(vel);//Reapply velocity in the world space 
    }
    public bool IsGrounded()
    {
        return grounded;
    }
    /// <summary>
    /// Handles all incoming advanced moves and either preforms them
    /// or invokes the modules that do preform them
    /// </summary>
    /// <param name="_AA"></param>
    public void InvokeAA(int _AA)
    {
        Debug.Log("Invoking AA");
        switch (_AA)
        {
            case 1:
                missileRack.FireMissile(true);

                break;
            case 2:
                if (grounded && SpendEnergy(20))
                {
                    /*replaces all global y velocity with the jump its a little janky but when
                     * I switched it over to the local space it made jumping feel very broken
                     * if you don't completely reset the Y velocity the jump is very inconsistant
                     * I want to look into this more but it plays well for now
                     */
                    carRigidBody.velocity = new Vector3(carRigidBody.velocity.x,jumpVelocity,carRigidBody.velocity.z);
                }
                break;
            case 3:

                if (SpendEnergy(50))
                {
                    missileRack.InstanciateMissile(NetworkManager.instance.Missiles[6].GetComponent<MissileBase>());
                }
                break;
        }

    }
    public override void Destroy()
    {

        carRigidBody.velocity = new Vector3(carRigidBody.velocity.x,10,carRigidBody.velocity.y);
        carRigidBody.angularVelocity = new Vector3(carRigidBody.angularVelocity.x + 20,carRigidBody.angularVelocity.y + 20,carRigidBody.velocity.z + 20);
        //gameObject.GetComponent<CarEngine>().enabled = false;
        gameObject.GetComponent<Suspension>().enabled = false;
        ServerSend.KillPlayer(GetId());
        parent.Invoke("Respawn",5f);
    }
    /// <summary>
    /// Determines what level of traction the car currently has
    /// will be based on how long you are turning as well as ground type possibly
    /// </summary>
    /// <returns>Current Traction</returns>
    public int GetTraction()
    {
        return traction;
    }
    public void SetTraction(int traction)
    {
        this.traction = traction;
    }
    public Vector3 GetGroundVect()
    {
        return groundVector;
    }
    public void SetInputs(bool[] inputs,float inputY,float inputX)
    {
        this.oldInputs = this.inputs;
        this.inputs = inputs;
       
            this.inputX = inputX;
        this.inputY = inputY;
    }
    public bool[] GetInputs()
    {
        return inputs;
    }
    public Vector3 GetVelocity()
    {
        return carRigidBody.velocity;
    }
    public bool IsFastTurn()
    {
        return inputs[5];
    }
    
    public bool IsBoosting()
    {
        return turbo > 0 ? inputs[4] : false;
        
    }
    public float GetInputX()
    {
        return inputX;
    }
    public float GetInputY()
    {
        return inputY;
    }
    public User GetUser()
    {
        return parent;
    }
    public int GetId()
    {
        return parent.GetID();
    }
    public float GetHealth()
    {
        return health;
    }
    public MissileRack GetMissileRack()
    {
        return missileRack;
    }
    public float GetTurbo()
    {
        return turbo;
    }
    public float GetMaxTurbo()
    {
        return maxTurbo;
    }
    public void FillTurbo()
    {
        turbo = maxTurbo;
    }
    public void SetTurbo(float turbo)
    {
        this.turbo = turbo;
        
    }
    public float GetEnergy()
    {
        return energy;
    }
    //We give the client a rounded off version of the energy for a cleaner look.
    public int GetEnergyInt()
    {
        return Mathf.RoundToInt(energy);
    }public Rigidbody getRigidBody()
    {
        return carRigidBody;
    }
    
}
