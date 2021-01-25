using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
class ServerHandle
{
    
    public static void WelcomeRecieved(int _fromClient,Packet _packet)
    {
        int _clientIdCheck = _packet.ReadInt();
        string _username = _packet.ReadString();

        Console.WriteLine($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_fromClient}.");
        if (_fromClient != _clientIdCheck)
        {
            Console.WriteLine($"Player \"{_username}\" (IDL {_fromClient}) has assumed the wrong client ID ({_clientIdCheck}.");
        }
        Server.clients[_fromClient].InitUser(NetworkManager.instance.defaultUser,_username);
        if (NetworkManager.instance.GameState == 1)
        {
            Server.clients[_fromClient].getUser().SendIntoGame();
        }
    }
    public static void PlayerMovement(int _fromClient,Packet _packet)
    {
        //Console.WriteLine("Moving out");

        bool[] _inputs = Utils.ConvertUIntToBoolArray(_packet.ReadUInt());
        Server.clients[_fromClient].getUser().child.GetMissileRack().SetCurrentAmmo(_packet.ReadInt());
        float Turn = _packet.ReadFloat();
        float Accel = _packet.ReadFloat();
      
        Server.clients[_fromClient].getUser().child.SetInputs(_inputs,Accel,Turn);
    }
    public static void Ping(int _fromClient,Packet _packet)
    {
        ServerSend.Pong(_fromClient,_packet.ReadFloat());
    }
    public static void AA(int _fromClient,Packet _packet)
    {
        Debug.Log("Recieve AA Packet");
        int _AAtype = _packet.ReadInt();
        //Server.clients[_fromClient].player.currentAmmo =
            _packet.ReadInt();
        Server.clients[_fromClient].getUser().child.InvokeAA(_AAtype);
    }
}
