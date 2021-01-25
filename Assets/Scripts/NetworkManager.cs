using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;

    public GameObject playerPrefab,Bigboy,defaultUser;
    public GameObject[] Missiles;

    public int GameState = 0;
    private void FixedUpdate()
    {
        switch (GameState)
        {
            case 0:
                Lobby();
                break;
            case 1:
            
                 //   Debug.Log(Server.FUCK);
                
                    break;
        }
    }
    private void Lobby()
    {
        //Debug.Log("Lobby");
        if (Server.GetConnected() > 0)
        {
            StartCoroutine(LoadScene("Prison"));
           
            GameState = 1;
            
           
        }
    }
    private IEnumerator LoadScene(string levelName)
    {
        // Start loading the scene
        AsyncOperation asyncLoadLevel = SceneManager.LoadSceneAsync(levelName,LoadSceneMode.Single);
        // Wait until the level finish loading
        while (!asyncLoadLevel.isDone)
            yield return null;
        // Wait a frame so every Awake and Start method is called
        yield return new WaitForEndOfFrame();
        Server.InitializeMap();
        foreach (Client client in Server.clients.Values)
        {
            Debug.Log(client.isConnected());
            if (client.isConnected())
            {
                Debug.Log("Spawning!");
                client.getUser().SendIntoGame();
            }
           
        }
    }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        Debug.Log("WHAEWHRAWEFHIWEA G");
        Server.Start(8,26950);

    }

    private void OnApplicationQuit()
    {
        Server.Stop();
    }

    public Player InstantiatePlayer()
    {
        //Debug.Log("Init Player");
        SpawnPoint Spawn = Server.GetRandomSpawn();
        return Instantiate(playerPrefab,Spawn.transform.position,Spawn.transform.rotation).GetComponent<Player>();
    }
    public Player InstantiateBigBoy()
    {
        //Debug.Log("Init Player");
        SpawnPoint Spawn = Server.GetRandomSpawn();
        return Instantiate(Bigboy,Spawn.transform.position,Spawn.transform.rotation).GetComponent<Player>();
    }
    public MissileBase InstantiateMissile(int _fromClient,int _ammo,Vector3 _position, Quaternion _rotation)
    {
       // Debug.Log("Init Player");
        MissileBase _missile = Instantiate(Missiles[_ammo],_position,_rotation).GetComponent<MissileBase>();
        _missile.type = _ammo;
        Server.AddMissile(1,_missile);
        ServerSend.SpawnMissile(_fromClient,_missile);
        return _missile;
       
    }
    public Pickup InstantiatePickup(Pickup _pickup,Vector3 _position)
    {
        // Debug.Log("Init Player");
        Pickup _pickupchild = Instantiate(_pickup,_position,Quaternion.identity).GetComponent<Pickup>();
        Server.AddPickup(1,_pickupchild);
        ServerSend.SpawnPickup(_pickupchild);
        return _pickupchild;
    }
}
