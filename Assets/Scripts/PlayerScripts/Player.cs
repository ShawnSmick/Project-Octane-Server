using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int id;
    public string username;


    public int Lives = 5;
    public float Health = 100;
    public float maxHealth = 100;
    public float Energy = 100;
    public float maxEnergy = 100;
    public float Turbo = 5f;
    public float maxTurbo = 5f;
    public float mainCooldown = 0;
    public float secondaryCooldown = 0;
    
    private bool[] oldInputs;
    private bool[] inputs;
    private float Accel = 0;
    private float Turn = 0;
    public CarController Car;
    private AcquireTargets targeting;
    public float jumpVel = 40f;
    public int[] ammo;
    public GameObject[] WeaponAnchors;

    public int currentAmmo = 0;
    bool vollying = false;
    int vollyLeft = 0;
    int volleyAmmo = 0;
    private void Start()
    {
        ammo = new int[NetworkManager.instance.Missiles.Length];
       
        ammo[1] = 2;
        ammo[2] = 2;
        ammo[5] = 2;
        Car = GetComponent<CarController>();
        targeting = GetComponent<AcquireTargets>();
        ServerSend.SetAmmo(id,ammo);
    }
    public void Initialize(int _id,string _username)
    {
        id = _id;
        username = _username;
        //inputs = new bool[5];
    }
    public void FixedUpdate()
    {

        mainCooldown -= Time.fixedDeltaTime;
        secondaryCooldown -= Time.fixedDeltaTime;
        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this);
        EnergyRegen();
    }
    public void EnergyRegen()
    {


        if (Energy < maxEnergy)
        {
            Energy += 1f * Time.fixedDeltaTime;
            if (Energy > maxEnergy)
            {
                Energy = maxEnergy;
            }
            ServerSend.SetEnergy(id,Mathf.RoundToInt(Energy));
        }
    }
    public void SetInput(bool[] _inputs,float _accel,float _turn)
    {
        if (Health <= 0)
            return;//TODO MAKE THIS NOT SHITE

        Accel = _accel;
        Turn = _turn;
        if (inputs != null)
        {
            oldInputs =new bool[inputs.Length];
            for (int i = 0; i < oldInputs.Length; i++)
            {
                oldInputs[i] = inputs[i];
            }
            if (Car.Debuffs.ContainsKey("Frozen"))
            {
                for (int i = 0; i < oldInputs.Length; i++)
                {
                    if (oldInputs[i] != _inputs[i] && Car.isGrounded())
                    {
                        Car.Debuffs["Frozen"].time -= .25f;
                        Rigidbody _rb = GetComponent<Rigidbody>();

                        _rb.velocity = new Vector3(_rb.velocity.x,5,_rb.velocity.z);
                        break;
                    }

                }
            }
        }
        inputs = _inputs;

        if (Car.Debuffs.ContainsKey("Frozen"))
            return;
        if (getInputs()[6] && mainCooldown<=0 && ammo[currentAmmo]>0&& !vollying)
        {
            MissileBase _missile = NetworkManager.instance.Missiles[currentAmmo].GetComponent<MissileBase>();
            mainCooldown = _missile.Rate;
            
            MissileBase Miss = NetworkManager.instance.InstantiateMissile(id,currentAmmo,WeaponAnchors[_missile.Anchor].transform.position,Car.isGrounded()?Quaternion.LookRotation(GetComponent<CarController>().getGroundForward(),Vector3.up):transform.rotation);
     
            if(Miss.volley > 0){
                volleyAmmo = currentAmmo;
                vollying = true;
                mainCooldown = Miss.volleyRate;
                vollyLeft = Miss.volley;
            }else
            {
                ammo[currentAmmo]--;
                ServerSend.SetAmmo(id,ammo);
            }
            Miss.SetParent(gameObject);
            Miss.target = targeting.GetCurrentTarget();
            
            
        }
        if (getInputs()[7] && secondaryCooldown <= 0)
        {
            secondaryCooldown = .1f;
            MissileBase Miss = NetworkManager.instance.InstantiateMissile(id,0,WeaponAnchors[0].transform.position,Car.isGrounded() ? Quaternion.LookRotation(GetComponent<CarController>().getGroundForward(),Vector3.up) : transform.rotation);
            Miss.SetParent(gameObject);
        }
        if (vollying && mainCooldown <= 0)
        {
            MissileBase _missile = NetworkManager.instance.Missiles[volleyAmmo].GetComponent<MissileBase>();          
            MissileBase Miss = NetworkManager.instance.InstantiateMissile(id,volleyAmmo,WeaponAnchors[_missile.Anchor].transform.position,Car.isGrounded() ? Quaternion.LookRotation(GetComponent<CarController>().getGroundForward(),Vector3.up) : transform.rotation);
            //ServerSend.SetAmmo(id,ammo);
            Miss.SetParent(gameObject);
            Miss.target = targeting.GetCurrentTarget();
            mainCooldown = Miss.volleyRate;
            vollyLeft--;
            if (vollyLeft <= 0)
            {
                mainCooldown = _missile.Rate;
                vollying = false;
                ammo[currentAmmo]--;
                ServerSend.SetAmmo(id,ammo);
            }
        }
        //transform.rotation = _rotation;
    }
    public void InvokeAA(int _AA)
    {
        Debug.Log("Invoking AA");
        switch (_AA)
        {
            case 1:
                if (mainCooldown <= 0 && ammo[currentAmmo] > 0)
                {
                    MissileBase _missile = NetworkManager.instance.Missiles[currentAmmo].GetComponent<MissileBase>();
                    mainCooldown = _missile.Rate;
                    ammo[currentAmmo]--;
                    MissileBase rearMiss = NetworkManager.instance.InstantiateMissile(id,currentAmmo,WeaponAnchors[_missile.Anchor].transform.position,Car.isGrounded() ? Quaternion.LookRotation(-GetComponent<CarController>().getGroundForward(),Vector3.up) : Quaternion.LookRotation(-transform.forward,Vector3.up));
                    Homing missH = rearMiss.GetComponent<Homing>();
                    //GroundFollow missGF = Miss.GetComponent<GroundFollow>();
                    Follow missF = rearMiss.GetComponent<Follow>();
                    if (missF)
                        missF.enabled = false;
                    if (missH)
                        missH.correctionSpeed /= 2;
                    ServerSend.SetAmmo(id,ammo);
                    rearMiss.SetParent(gameObject);
                    rearMiss.target = targeting.GetCurrentRearTarget();


                }
                break;
            case 2:
                if (Car.isGrounded()&&SpendEnergy(20))
                {

                    Rigidbody _rb = GetComponent<Rigidbody>();
                  
                    _rb.velocity = new Vector3(_rb.velocity.x,jumpVel,_rb.velocity.z);
                }
                break;
            case 3:

                if (SpendEnergy(50))
                {

                    MissileBase freezeMiss = NetworkManager.instance.InstantiateMissile(id,6,WeaponAnchors[1].transform.position,Car.isGrounded() ? Quaternion.LookRotation(GetComponent<CarController>().getGroundForward(),Vector3.up) : transform.rotation);
                    
                    freezeMiss.SetParent(gameObject);
                    freezeMiss.target = targeting.GetCurrentTarget();
                }


                
                break;
        }
        ServerSend.SetEnergy(id,Mathf.RoundToInt(Energy));
    }
    public bool SpendEnergy(int _energy)
    {
        if (Energy > _energy)
        {
            Energy -= _energy;
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool[] getInputs()
    {
        return Car.Debuffs.ContainsKey("Frozen")?new bool[inputs.Length]:inputs;
    }
    public float getTurn()
    {
        return Car.Debuffs.ContainsKey("Frozen") ? 0:Turn;
    }
    public float getAccel()
    {
        return Car.Debuffs.ContainsKey("Frozen") ? 0 : Accel;
    }
    public bool[] getOldInputs()
    {
        return oldInputs;
    }
    public void Damage(float _damage)
    {
        Health -= _damage;
        if (Health <= 0)
        {
            IsKill();
        }
        ServerSend.SetHealth(id,Mathf.RoundToInt(Health));
    }
    public void IsKill()
    {
        Rigidbody _rb = gameObject.GetComponent<Rigidbody>();
        _rb.velocity = new Vector3(_rb.velocity.x,10,_rb.velocity.y);
        _rb.angularVelocity = new Vector3(_rb.angularVelocity.x + 20,_rb.angularVelocity.y + 20,_rb.velocity.z + 20);
        gameObject.GetComponent<CarController>().enabled = false;
        gameObject.GetComponent<Suspension>().enabled = false;
        ServerSend.KillPlayer(id);
        Invoke("Respawn",5f);
    }
    public void Respawn()
    {
        if (Lives > 1)
        {
            if (username == "GLORIOUSBIGBOY")
            {
                Server.clients[id].player = NetworkManager.instance.InstantiateBigBoy();
            }
            else
            {
                Server.clients[id].player = NetworkManager.instance.InstantiatePlayer();
            }
            Server.clients[id].player.Lives = Lives - 1;
            Server.clients[id].player.Initialize(id,username);
            foreach (Client _client in Server.clients.Values)
            {
                if (_client.player != null)
                {
                    ServerSend.SpawnPlayer(_client.id,Server.clients[id].player);
                }
            }
            ThreadManager.ExecuteOnMainThread(() =>
            {
                UnityEngine.Object.Destroy(gameObject);
            });
        }

        else
        {
            ServerSend.RemovePlayer(id);
            ThreadManager.ExecuteOnMainThread(() =>
            {
                UnityEngine.Object.Destroy(gameObject);


            });

        }
    }
    
}

