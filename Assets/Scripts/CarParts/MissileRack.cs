using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileRack : MonoBehaviour
{
    [SerializeField]
    private GameObject[] missileAnchor;
    private int[] ammo;
    private CarEngine parent;
    private AcquireTargets targeting;
    private int currentAmmo = 0;
    private bool volleying = false;
    private bool rearFire = false;
    private int volleyShotsLeft = 0;
    private float weaponCooldown = 0;
    private void Start()
    {
        ammo = new int[NetworkManager.instance.Missiles.Length];

        ammo[1] = 2;
        ammo[2] = 2;
        ammo[7] = 2;
        parent = GetComponent<CarEngine>();
        targeting = GetComponent<AcquireTargets>();
        ServerSend.SetAmmo(parent.GetId(),ammo);
    }
    public void FixedUpdate()
    {
        if(weaponCooldown > 0)
            weaponCooldown -= Time.fixedDeltaTime;
        if (volleying && weaponCooldown <= 0)
        {
            MissileBase missile = NetworkManager.instance.Missiles[currentAmmo].GetComponent<MissileBase>();
            weaponCooldown = missile.volleyRate;
            InstanciateMissile(missile,rearFire);
           
            volleyShotsLeft--;
            if (volleyShotsLeft <= 0)
            {
                weaponCooldown = missile.Rate;
                volleying = false;
                ammo[currentAmmo]--;
                ServerSend.SetAmmo(parent.GetId(),ammo);
            }
        }
    }
    public void FireMissile(bool rearFire=false)
    {
        if (weaponCooldown <= 0 && ammo[currentAmmo] > 0 && !volleying)
        {

            this.rearFire = rearFire;
            MissileBase missile = NetworkManager.instance.Missiles[currentAmmo].GetComponent<MissileBase>();
            weaponCooldown = missile.Rate;
            InstanciateMissile(missile,rearFire);
            if (missile.volley > 0)
            {
                volleying = true;
                weaponCooldown = missile.volleyRate;
                volleyShotsLeft = missile.volley;
            }
            else
            {
                ammo[currentAmmo]--;
                ServerSend.SetAmmo(parent.GetId(),ammo);
            }
        }
    }
    public void InstanciateMissile(MissileBase missile,bool rearFire = false)
    {
        Quaternion rotation;
        if (parent.IsGrounded())
        {
            rotation = Quaternion.LookRotation(parent.GetGroundVect() * ((rearFire) ? -1 : 1),Vector3.up);
        }
        else
        {
            rotation = Quaternion.LookRotation(transform.forward * ((rearFire) ? -1 : 1),Vector3.up);
        }
        missile = NetworkManager.instance.InstantiateMissile(parent.GetId(),missile.type,missileAnchor[missile.Anchor].transform.position,rotation);
        if (rearFire)
        {

            Homing missH = missile.GetComponent<Homing>();
            GroundFollow missGF = missile.GetComponent<GroundFollow>();
            Follow missF = missile.GetComponent<Follow>();
            if (missF)
                missF.enabled = false;
            if (missH)
                missH.correctionSpeed /= 2;
        }
        
        
        missile.SetParent(gameObject);
        missile.target = targeting.GetCurrentTarget();
    }
    public void SetCurrentAmmo(int Ammo)
    {
        currentAmmo = Ammo;
    }
    public int[] getAmmo()
    {
        return ammo;
    }
    public void LoadAmmo(int slot,int amount)
    {
        ammo[slot] += amount;
    }
}
