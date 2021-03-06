﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupTurbo : Pickup
{
    private void OnTriggerEnter(Collider other)
    {


        if (other.gameObject.tag == "Player")
        {
            CarEngine _player = other.gameObject.GetComponent<CarEngine>();
            Debug.Log("Pickup!");
            if (_player.GetTurbo() < _player.GetMaxTurbo())
            {

                _player.FillTurbo();
                Server.NetworkedObjects.Remove(GetID());
                ServerSend.SetTurbo(_player.GetId(),_player.GetTurbo());
                ServerSend.KillPickup(GetID());
                Destroy(gameObject);
            }
        }

    }
}
