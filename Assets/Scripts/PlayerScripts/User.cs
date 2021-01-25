using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class User : MonoBehaviour
{

    #region Persistant Variables
    
    private string _username="ok desu ka";
    private Client _client; //Parent Client
    private bool _isHost = false; //Used for determining who changes settings in a lobby, player who entered lobby first.
    private int _lives = -1;//-1 indicated Lives are notbeing used in the current gamemode, Intention is that when you run out of lives you stop respawning in Sole Survivor
    [SerializeField]
    [Tooltip("Set the default Car for all Users")]
    private GameObject _Selected; //The prefab used to create our child Car. Might be a dumb way of doing this.
    #endregion

    public CarEngine child; //The Main controller portion of the Car.

    public void Awake()
    {
        DontDestroyOnLoad(this);
    }
    /// <summary>
    /// Sets up basic user information
    /// </summary>
    /// <param name="client">parent client</param>
    /// <param name="uname">player username</param>
    /// <param name="host">are they hosting</param>
    public void InstanciateUser(Client client,string uname,bool host)
    {
        Debug.Log("HERE WE FUCKING GO!");
        _client = client;
        _username = uname;
        _isHost = host;
    }

    /// <summary>
    /// Instanciates User's Selected vehicle at a random spawnpoint
    /// </summary>
    public void SpawnVehicle()
    {
        SpawnPoint Spawn = Server.GetRandomSpawn();
        child = Instantiate(_Selected,Spawn.transform.position,Spawn.transform.rotation).GetComponent<CarEngine>();

        child.Initialize(this);
    }
    /// <summary>
    /// Respawns the Player
    /// </summary>
    public void Respawn()
    {
        if (_lives > 1 || _lives == -1) //As long as you have more than one life or -1 life
        {       
            //Destroy and recreate child vehicle.
            Destroy(child.gameObject);         
            SpawnVehicle();
            _lives -= (_lives == -1) ? 0 : 1; //Dec lives if playing Sole Survivor
            foreach (Client _client in Server.clients.Values)
            {
                //Spawn player in all games this works because the spawn player client handle deletes the old when handed a new one.
                if (_client.isConnected()){ ServerSend.SpawnPlayer(_client.id,child); }
            }
           
        }
        else
        {
            ServerSend.RemovePlayer(GetID()); //TODO: make players without lives not simply get removed from game? Might not matter.
            Destroy(child.gameObject);
        }
    }
    /// <summary>
    /// Loads player into match with all other vehicles, pickups and missiles.
    /// </summary>
    public void SendIntoGame()
    {
        SpawnVehicle();

        //Foreach client spawn your car in their game, and their car in your game
        foreach (Client _client in Server.clients.Values)
        {
            if (_client.isConnected()) 
            {
                if (_client.id != GetID()) //client not me
                {
                    ServerSend.SpawnPlayer(GetID(),_client.getUser().child); //spawn user's car in my client
                }
                ServerSend.SpawnPlayer(_client.id,child);//spawn my car in all clients
            }
        }   
        ServerSend.SetHealth(GetID(),child.GetHealthInt());
        //Send the Client all existing missiles
        foreach (MissileBase _missile in Server.Missiles.Values)
        {
            ServerSend.InitMissiles(GetID(),-1,_missile);//TODO: make it assign the missiles the correct id instead of making them all -1, its not a big deal but it's sloppy
        }
        //Send the Client all existing pickups
        foreach (Pickup _pickup in Server.Pickups.Values)
        {
            ServerSend.InitPickup(GetID(),_pickup);
        }

    }
    /// <summary>
    /// Returns Client ID
    /// </summary>
    public int GetID()
    {
        return _client.id;
    }
    public string GetName()
    {
        return _username;
    }
   


}
