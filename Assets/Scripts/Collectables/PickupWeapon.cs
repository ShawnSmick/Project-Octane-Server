using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupWeapon : Pickup
{
    public int AmmoType = 0;
    public int Amount = 2;
    private void OnTriggerEnter(Collider other)
    {


        if (other.gameObject.tag == "Player")
        {
            Debug.Log("Pickup!");
            CarEngine _player = other.gameObject.GetComponent<CarEngine>();
            _player.GetMissileRack().LoadAmmo(AmmoType,Amount);
            Amount -= Amount;
            Server.Pickups.Remove(id);
            ServerSend.SetAmmo(_player.GetId(),_player.GetMissileRack().getAmmo());
            ServerSend.KillPickup(id);
            Destroy(gameObject);
        }

    }
}
