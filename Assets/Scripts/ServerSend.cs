using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

class ServerSend
{
    private static int Lag = 0;
    private static void SendTCPData(int _toClient,Packet _packet)
    {
        _packet.WriteLength();
        Server.clients[_toClient].tcp.SendData(_packet);
    }
    private static void SendTCPDataToAll(Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {

            Server.clients[i].tcp.SendData(_packet);
        }
    }
    private static void SendTCPDataToAll(int _exceptClient,Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != _exceptClient)
                Server.clients[i].tcp.SendData(_packet);
        }
    }
    private static void SendUDPData(int _toClient,Packet _packet)
    {
        _packet.WriteLength();
        Server.clients[_toClient].udp.SendData(_packet);
    }
    private static void SendUDPDataToAll(Packet _packet)
    {
        /* Task.Run(async () =>
         {
             await Task.Delay(Lag);
             Debug.Log("Sendifying");*/
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {

            Server.clients[i].udp.SendData(_packet);
        }
        //  });
    }
    private static void SendUDPDataToAll(int _exceptClient,Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != _exceptClient)
                Server.clients[i].udp.SendData(_packet);
        }
    }
    public static void Welcome(int _toClient,string _msg)
    {
        using (Packet _packet = new Packet((int) ServerPackets.welcome))
        {
            _packet.Write(_msg);
            _packet.Write(_toClient);

            SendTCPData(_toClient,_packet);
        }
    }
    public static void SpawnPlayer(int _toClient,Player _player)
    {
        using (Packet _packet = new Packet((int) ServerPackets.spawnPlayer))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.username);
            _packet.Write(Mathf.RoundToInt(_player.Health));
            _packet.Write(_player.transform.position);
            _packet.Write(_player.transform.rotation);

            SendTCPData(_toClient,_packet);
        }
    }
    public static void PlayerPosition(Player _player)
    {
        using (Packet _packet = new Packet((int) ServerPackets.playerPosition))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.transform.position);

            SendUDPDataToAll(_packet);
        }
    }
    public static void PlayerRotation(Player _player)
    {
        using (Packet _packet = new Packet((int) ServerPackets.playerRotation))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.transform.rotation);

            SendUDPDataToAll(_packet);
        }
    }
    public static void SpawnPlayer(int _toClient,CarEngine _player)
    {
        using (Packet _packet = new Packet((int) ServerPackets.spawnPlayer))
        {
            _packet.Write(_player.GetId());
            _packet.Write(_player.GetUser().GetName());
            _packet.Write(_player.GetHealthInt());
            _packet.Write(_player.transform.position);
            _packet.Write(_player.transform.rotation);

            SendTCPData(_toClient,_packet);
        }
    }
    public static void PlayerPosition(CarEngine user)
    {
        using (Packet _packet = new Packet((int) ServerPackets.playerPosition))
        {
            _packet.Write(user.GetId());
            _packet.Write(user.transform.position);

            SendUDPDataToAll(_packet);
        }
    }
    public static void PlayerRotation(CarEngine user)
    {
        using (Packet _packet = new Packet((int) ServerPackets.playerRotation))
        {
            _packet.Write(user.GetId());
            _packet.Write(user.transform.rotation);

            SendUDPDataToAll(_packet);
        }
    }
    public static void CarPosition(CarEngine user,uint timeStamp)
    {
        using (Packet _packet = new Packet((int) ServerPackets.CarPosition))
        {
            _packet.Write(user.GetId());
            _packet.Write(user.transform.position);
            _packet.Write(user.transform.rotation);
            _packet.Write(user.GetVelocity());
            _packet.Write(user.GetHealthInt());
            _packet.Write(user.GetTurbo());
            _packet.Write(user.GetEnergyInt());
            _packet.Write(timeStamp);
            SendUDPDataToAll(_packet);

        }
    }
    public static void Pong(int _toClient,float _time)
    {
        using (Packet _packet = new Packet((int) ServerPackets.Pong))
        {

            _packet.Write(_time);

            SendUDPData(_toClient,_packet);
        }

    }
    public static void InitMissiles(int _toClient,int _FromClient,MissileBase _missile)
    {
        using (Packet _packet = new Packet((int) ServerPackets.TestMissile))
        {
            _packet.Write(_FromClient);
            _packet.Write(_missile.GetID());
            _packet.Write(_missile.type);
            _packet.Write(_missile.transform.position);
            _packet.Write(_missile.transform.rotation);

            SendTCPData(_toClient,_packet);
        }
    }
    public static void SpawnMissile(int _FromClient,MissileBase _missile)
    {
        using (Packet _packet = new Packet((int) ServerPackets.TestMissile))
        {
            _packet.Write(_FromClient);
            _packet.Write(_missile.GetID());
            _packet.Write(_missile.type);
            _packet.Write(_missile.transform.position);
            _packet.Write(_missile.transform.rotation);

            SendTCPDataToAll(_packet);
        }
    }
    public static void MissileUpdate(MissileBase _missile)
    {
        using (Packet _packet = new Packet((int) ServerPackets.MissileUpdate))
        {
            //Debug.Log("Updating Missile");
            _packet.Write(_missile.GetID());
            _packet.Write(_missile.transform.position);
            _packet.Write(_missile.transform.rotation);
            SendUDPDataToAll(_packet);
        }
    }
    public static void NetworkObjectUpdate(NetworkObject NetOb)
    {
        using (Packet _packet = new Packet((int) ServerPackets.NetworkObjectUpdate))
        {
            //Debug.Log("Updating Missile");
            _packet.Write(NetOb.GetID());
            _packet.Write(NetOb.transform.position);
            _packet.Write(NetOb.transform.rotation);
            SendUDPDataToAll(_packet);
        }
    }
    public static void MissileDestroyed(int _id)
    {
        using (Packet _packet = new Packet((int) ServerPackets.MissileDestroyed))
        {
            //  Debug.Log("Missile is Kill");
            _packet.Write(_id);
            SendTCPDataToAll(_packet);
        }
    }
    public static void InitPickup(int _toClient,Pickup _pickup)
    {
        using (Packet _packet = new Packet((int) ServerPackets.SpawnPickup))
        {
            _packet.Write(_pickup.GetID());
            _packet.Write(_pickup.Type);
            _packet.Write(_pickup.transform.position);
            SendTCPData(_toClient,_packet);
        }
    }
    public static void SpawnPickup(Pickup _pickup)
    {
        using (Packet _packet = new Packet((int) ServerPackets.SpawnPickup))
        {
            _packet.Write(_pickup.GetID());
            _packet.Write(_pickup.Type);
            _packet.Write(_pickup.transform.position);
            SendTCPDataToAll(_packet);
        }
    }
    public static void KillPickup(int _id)
    {
        using (Packet _packet = new Packet((int) ServerPackets.KillPickup))
        {
            _packet.Write(_id);
            SendTCPDataToAll(_packet);
        }
    }
    public static void KillPlayer(int _id)
    {
        using (Packet _packet = new Packet((int) ServerPackets.KillPlayer))
        {
            _packet.Write(_id);
            SendTCPDataToAll(_packet);
        }
    }
    public static void RemovePlayer(int _id)
    {
        using (Packet _packet = new Packet((int) ServerPackets.RemovePlayer))
        {
            _packet.Write(_id);
            SendTCPDataToAll(_packet);
        }
    }

    public static void SetAmmo(int _toClient,int[] _ammo)
    {
        using (Packet _packet = new Packet((int) ServerPackets.SetAmmo))
        {
            for (int i = 0; i < _ammo.Length; i++)
            {
                _packet.Write(_ammo[i]);
            }
            SendTCPData(_toClient,_packet);
        }
    }
    public static void SetHealth(int _id,int _health)
    {
        using (Packet _packet = new Packet((int) ServerPackets.SetHealth))
        {
            _packet.Write(_id);
            _packet.Write(_health);
            SendTCPDataToAll(_packet);
        }
    }
    public static void SetTurbo(int _id,float _turbo)
    {
        using (Packet _packet = new Packet((int) ServerPackets.SetTurbo))
        {
            _packet.Write(_id);
            _packet.Write(_turbo);
            SendTCPDataToAll(_packet);
        }
    }
    public static void SetEnergy(int _id,int _energy)
    {
        using (Packet _packet = new Packet((int) ServerPackets.SetEnergy))
        {
            _packet.Write(_id);
            _packet.Write(_energy);
            SendTCPDataToAll(_packet);
        }
    }
    public static void ToggleDebuff(int _id,string _debuff)
    {
        using (Packet _packet = new Packet((int) ServerPackets.ToggleDebuff))
        {
            _packet.Write(_id);
            _packet.Write(_debuff);
            SendTCPDataToAll(_packet);
        }
    }
}
