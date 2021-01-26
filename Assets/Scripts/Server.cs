using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using System.Threading.Tasks;

class Server
{
    public static int MaxPlayers
    {
        get; private set;
    }
    public static int Port
    {
        get; private set;
    }
    public delegate void PacketHandler(int _fromClient,Packet _packet);
    public static Dictionary<int,PacketHandler> packetHandlers;
    private static TcpListener tcpListener;
    private static UdpClient udpListener;
    public static int FUCK = -1;
    public static Dictionary<int,Client> clients = new Dictionary<int,Client>();
    //public static Dictionary<int,MissileBase> Missiles = new Dictionary<int,MissileBase>();
    //public static Dictionary<int,Pickup> Pickups = new Dictionary<int,Pickup>();
    public static Dictionary<int,NetworkObject> NetworkedObjects = new Dictionary<int,NetworkObject>(); 
    public static SpawnPoint[] Spawns;


    public static void Start(int _maxPlayers,int _port)
    {
        FUCK = 1;
        
        MaxPlayers = _maxPlayers;
        Port = _port;
        InitializeServerData();
        tcpListener = new TcpListener(IPAddress.Any,Port);
        tcpListener.Start();
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback),null);

        udpListener = new UdpClient(Port);
        udpListener.BeginReceive(UDPReceiveCallback,null);
        Console.WriteLine($"Server started on {Port}.");
    }
    public static int GetConnected()
    {
        int connected = 0;
        foreach (Client c in clients.Values)
        {
           // Debug.Log(c.isConnected());
            connected += (c.isConnected()) ? 1 : 0;
        }
        return connected;
    }
    public static void InitializeMap()
    {
      
      //  Pickup[] pickups = GameObject.FindObjectsOfType<Pickup>();

       // InitPickups(pickups);
        Spawns = GameObject.FindObjectsOfType<SpawnPoint>();
        for (int s = 0; s < Spawns.Length; s++)
        {
           
            Spawns[s].id = s;
        }
    }
    public static void InitPickups(Pickup[] _pickups)
    {
        int _id = 0;
        foreach (Pickup _pickup in _pickups)
        {
            AddPickup(_id,_pickup);
            _id++;
        }
    }
    public static void AddPickup(int id,Pickup pickup)
    {
        if (NetworkedObjects.ContainsKey(id))
        {
            AddPickup(id + 1,pickup);
        }
        else
        {
            pickup.SetID(id);
            NetworkedObjects.Add(id,pickup);
        }
    }
    public static void UnInitializeMap()
    {
        //Missiles = new Dictionary<int,MissileBase>();
        //Pickups = new Dictionary<int,Pickup>();
        NetworkedObjects = new Dictionary<int,NetworkObject>();
        Spawns = null;
    }
    private static void TCPConnectCallback(IAsyncResult _result)
    {
        TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback),null);
        Console.WriteLine($"incoming connection from {_client.Client.RemoteEndPoint}...");
        for (int i = 1; i <= MaxPlayers; i++)
        {
            if (clients[i].tcp.socket == null)
            {
                clients[i].tcp.Connect(_client);
                Console.WriteLine($"{_client.Client.RemoteEndPoint} connected!");
                return;
            }
        }
        Console.WriteLine($"{_client.Client.RemoteEndPoint} failed to connect: Sever Full!");
    }
    private static void UDPReceiveCallback(IAsyncResult _result)
    {
        try
        {
            IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any,0);
            byte[] _data = udpListener.EndReceive(_result,ref _clientEndPoint);
            udpListener.BeginReceive(UDPReceiveCallback,null);

            if (_data.Length < 4)
            {
                return;
            }
            using (Packet _packet = new Packet(_data))
            {
                int _clientId = _packet.ReadInt();

                if (_clientId == 0)
                {
                    return;
                }
                if (clients[_clientId].udp.endPoint == null)
                {
                    clients[_clientId].udp.Connect(_clientEndPoint);
                    return;
                }
                if (clients[_clientId].udp.endPoint.ToString() == _clientEndPoint.ToString())
                {
                    clients[_clientId].udp.HandleData(_packet);
                }
            }
        }
        catch (Exception _ex)
        {
            Console.WriteLine($"Error recieving UDP data: {_ex}.");
        }
    }
    public static void SendUDPData(IPEndPoint _clientEndPoint,Packet _packet)
    {
        try
        {
            if (_clientEndPoint != null)
            {
                // Task.Run(async() =>{

                //   await Task.Delay(200);
                //Debug.Log("LETS GO");
                udpListener.BeginSend(_packet.ToArray(),_packet.Length(),_clientEndPoint,null,null);
                //  });
            }
        }
        catch (Exception _ex)
        {
            Console.WriteLine($"Error sending data to {_clientEndPoint} vis UDP: {_ex}");
        }
    }

    private static void InitializeServerData()
    {
        Debug.Log("init ServerData");
        for (int i = 1; i <= MaxPlayers; i++)
        {
            clients.Add(i,new Client(i));
        }
        packetHandlers = new Dictionary<int,PacketHandler>() {
                { (int) ClientPackets.welcomeReceived,ServerHandle.WelcomeRecieved },
                { (int) ClientPackets.playerMovement,ServerHandle.PlayerMovement },
                { (int) ClientPackets.Ping,ServerHandle.Ping },
                 { (int) ClientPackets.AA,ServerHandle.AA }
            };
    }
    public static void Stop()
    {
        tcpListener.Stop();
        udpListener.Close();
    }

    public static SpawnPoint GetRandomSpawn()
    {
        int Spawn = UnityEngine.Random.Range(0,Spawns.Length - 1);
        Debug.Log("Spawning!" + Spawn);
        if (Physics.CheckSphere(Spawns[Spawn].transform.position,10f,8))
        {
            return GetRandomSpawn();
        }
        return Spawns[Spawn];
    }
    public static void AddMissile(int _id,MissileBase _missile)
    {
        if (NetworkedObjects.ContainsKey(_id))
        {
            AddMissile(_id + 1,_missile);
        }
        else
        {
            _missile.SetID(_id);
            NetworkedObjects.Add(_id,_missile);
        }
    }
    
}
