using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineGun : MonoBehaviour
{
    [SerializeField]
    private GameObject[] machinegunAnchors;
    private int machineGunIndex = 0;
    private float weaponCooldown = 0;
    private CarEngine parent;
    private AcquireTargets targeting;
    private void Start()
    {
        parent = GetComponent<CarEngine>();
        targeting = GetComponent<AcquireTargets>();
    }
    public void FixedUpdate()
    {
        if (weaponCooldown > 0)
            weaponCooldown -= Time.fixedDeltaTime;
    }
        public void Fire()
    {
 
            if (weaponCooldown <= 0)
            {
                MissileBase missile = NetworkManager.instance.Missiles[0].GetComponent<MissileBase>();
                weaponCooldown = missile.Rate;
                InstanciateMissile(missile);
                cycleAnchors();
           }       
    }
    public void InstanciateMissile(MissileBase missile)
    {
        Quaternion rotation;
        if (parent.IsGrounded())
        {
            rotation = Quaternion.LookRotation(parent.GetGroundVect(),Vector3.up);
        }
        else
        {
            rotation = transform.rotation;
        }
        missile = NetworkManager.instance.InstantiateMissile(parent.GetId(),missile.type,machinegunAnchors[machineGunIndex].transform.position,rotation);
        missile.SetParent(gameObject);
        missile.target = targeting.GetCurrentTarget();
    }
    public void cycleAnchors()
    {
        machineGunIndex++;
        if (machineGunIndex > machinegunAnchors.Length-1)
        {
            machineGunIndex=0;
            
        }
    }
}
