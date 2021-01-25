using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;

public class Client
{
    #region NetworkVars
    private static int dataBufferSize = 4096;
    public TCP tcp;
    public UDP udp;
    #endregion

    public int id;
    private User user;
    public Player player; //THIS IS DEAD


    public Client(int _clientid)
    {
        id = _clientid;
        tcp = new TCP(id);
        udp = new UDP(id);
    }
 

    /// <summary>
    /// Checks if a user is connected to the client slot
    /// </summary>
    /// <returns></returns>
    public bool isConnected()
    {
        return (user!= null);
    }
    /// <summary>
    /// Fills client with an incoming user
    /// </summary>
    public void InitUser(GameObject defaultUser,string username)
    {
        user = GameObject.Instantiate(defaultUser).GetComponent<User>();
        user.InstanciateUser(this,username,false);//Setup User
        user.gameObject.name = username; //Rename the user object mostly for ease of debugging
    }
  
    #region Getters and Setters
    public User getUser()
    {
        return user;
    }
    #endregion
    #region Networking Stuff
    public class TCP
    {

        public TcpClient socket;
        private readonly int id;
        private NetworkStream stream;
        private Packet receivedData;
        private byte[] receiveBuffer;
        public TCP(int _id)
        {
            id = _id;
        }
        public void Connect(TcpClient _socket)
        {
            socket = _socket;
            socket.SendBufferSize = dataBufferSize;
            socket.ReceiveBufferSize = dataBufferSize;

            stream = socket.GetStream();
            receivedData = new Packet();
            receiveBuffer = new byte[dataBufferSize];
            stream.BeginRead(receiveBuffer,0,dataBufferSize,RecieveCallback,null);
            ServerSend.Welcome(id,"Welcome To Octane");
        }
        public void SendData(Packet _packet)
        {
            try
            {
                if (socket != null)
                {
                    stream.BeginWrite(_packet.ToArray(),0,_packet.Length(),null,null);
                }
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error sending data to player {id} via tcp: {_ex}");
            }
        }
        private bool HandleData(byte[] _data)
        {
            int _packetLength = 0;

            receivedData.SetBytes(_data);

            if (receivedData.UnreadLength() >= 4)
            {
                _packetLength = receivedData.ReadInt();
                if (_packetLength <= 0)
                {
                    return true;
                }
            }
            while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
            {
                byte[] _packetByte = receivedData.ReadBytes(_packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _packet = new Packet(_packetByte))

                    {
                        int _packetId = _packet.ReadInt();
                        Server.packetHandlers[_packetId](id,_packet);
                    }
                });
                _packetLength = 0;
                if (receivedData.UnreadLength() >= 4)
                {
                    _packetLength = receivedData.ReadInt();
                    if (_packetLength <= 0)
                    {
                        return true;
                    }
                }
            }
            if (_packetLength <= 1)
            {
                return true;
            }
            return false;
        }
        private void RecieveCallback(IAsyncResult _result)
        {
            try
            {
                int _byteLength = stream.EndRead(_result);
                if (_byteLength <= 0)
                {
                    Server.clients[id].Disconnect();

                    return;
                }
                byte[] _data = new byte[_byteLength];
                Array.Copy(receiveBuffer,_data,_byteLength);
                receivedData.Reset(HandleData(_data));
                stream.BeginRead(receiveBuffer,0,dataBufferSize,RecieveCallback,null);
            }
            catch (Exception _ex)
            {
                Debug.Log("Something went fucky");
                Server.clients[id].Disconnect();
            }
        }
        public void Disconnect()
        {
            socket.Close();
            stream = null;
            receivedData = null;
            receiveBuffer = null;
            socket = null;
        }
    }
    public class UDP
    {
        public IPEndPoint endPoint;
        private int id;
        public UDP(int _id)
        {
            id = _id;
        }

        public void Connect(IPEndPoint _endpoint)
        {
            endPoint = _endpoint;

        }

        public void SendData(Packet _packet)
        {
            Server.SendUDPData(endPoint,_packet);
        }
        public void HandleData(Packet _packetData)
        {
            int _packetLength = _packetData.ReadInt();
            byte[] _packetBytes = _packetData.ReadBytes(_packetLength);

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet _packet = new Packet(_packetBytes))
                {
                    int _packetId = _packet.ReadInt();
                    Server.packetHandlers[_packetId](id,_packet);
                }
            });
        }
        public void Disconnect()
        {
            endPoint = null;

        }
    }
    /// <summary>
    /// Always call when a player disconnects or is forcibly disconnected
    /// </summary>
    public void Disconnect()
    {
        Debug.Log($"{tcp.socket.Client.RemoteEndPoint} has disconnected.");
        ServerSend.RemovePlayer(id); //Tells all clients to remove the user from their instance

        //Safely Destroys the user object 
        ThreadManager.ExecuteOnMainThread(() =>
        {
            if (user.child != null)
                UnityEngine.Object.Destroy(user.child.gameObject);
            if (user != null)
                UnityEngine.Object.Destroy(user.gameObject);

            user = null;
        });
        //Cuts connection
        tcp.Disconnect();
        udp.Disconnect();
    }
    #endregion
}

