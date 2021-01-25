using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileBase : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    public float speed = 5f;
    [SerializeField]
    private float impact = 10000f; //force imparted on collision
    [SerializeField]
    private float popup = 0f; //force imparted on collision
    public float damage = 8;
    private GameObject _parent;
    public Target target;
    public float Rate = 0.5f;
    public int Anchor = 0;
    public bool Targeting = false;
    public float killTime = 5;
    public int type = 0;
    public float radius = 0;
    public bool synced = true;
    public string[] Debuffs;
  //  private ParticleSystem emit;
  //  public GameObject explosion;
    public float aliveTime = 0;
    public int id;
    public int volley = 0;
    public float volleyRate = .1f;
    void Start()
    {
       // emit = GetComponentInChildren<ParticleSystem>();
        // GetComponent<Rigidbody>().detectCollisions = false;
    }
 

    void FixedUpdate()
    {
        aliveTime += Time.deltaTime;
        transform.position += transform.forward * speed * Time.fixedDeltaTime;
        if(synced)
            ServerSend.MissileUpdate(this);
        if (aliveTime > killTime)
            IsKill();
        
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(transform.position,radius);
    }
        //https://www.youtube.com/watch?v=lLl0DVzRksk This is cursed please never again.
        public void SetParent(GameObject parentalfigure) // Please Die Immediately
    {
        _parent = parentalfigure;
    }
    public GameObject GetParent()
    {
        return _parent;
    }
    private void OnTriggerEnter(Collider other)
    {
        //Make sure you are not hitting daddy, he doesnt like that. Also make sure you are not colliding with fellow projectiles
        Debug.Log("HIT");
        if (radius > 0&& other.gameObject != _parent && other.gameObject.tag != "Projectiles" && other.gameObject.tag != "IgnoreMissiles")
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position,radius);
            List<int> idActedOn = new List<int>();
            foreach (Collider _other in colliders)
            {
                Player _player = _other.gameObject.GetComponent<Player>();
                if (_player != null)
                {
                    Debug.Log(_player.id + " Tagged");
                    if (!idActedOn.Contains(_player.id))
                    {
                        idActedOn.Add(_player.id);
                        applyForces(_other);

                    }
                }
                else
                {
                    applyForces(_other);
                }
                
                

            }
        }
        else if (other.gameObject != _parent && other.gameObject.tag != "Projectiles" && other.gameObject.tag != "IgnoreMissiles")
        {

            //Ensure we are actually dealing with something 'rigid'
            
            
                applyForces(other);
            damage -= damage;

                
                    
            
          //  IsKill();
            /*  if (emit != null)
              {
                  DetachParticle();
              }
              if (explosion != null)
              {
                  GameObject exp = Instantiate(explosion,transform.position,Quaternion.identity);
                  Destroy(exp,5);
              }*/
           
        }
    }
    public void applyForces(Collider other)
    {
        Rigidbody CollidedRB = other.gameObject.GetComponent<Rigidbody>();
        if (CollidedRB != null)
        {
            
            CollidedRB.AddForceAtPosition(transform.forward * impact,other.ClosestPointOnBounds(transform.position)); //if so apply force at point of contact (more or less)
            if (popup > 0)
                CollidedRB.velocity = new Vector3(CollidedRB.velocity.x,popup,CollidedRB.velocity.z);
            CarEngine CollidedCar = other.gameObject.GetComponent<CarEngine>();
            if (CollidedCar != null)
            {
                if(CollidedCar.GetHealth()>0)
                    CollidedCar.TakeDamage(damage);
                foreach (string s in Debuffs)
                {
                    switch (s)
                    {
                        case "Burn":
                        case "burn":
                            //Burning(CollidedCar);
                            break;
                        case "Freeze":
                        case "freeze":
                           
                            
                            //Freeze(CollidedCar);
                            
                            
                            break;
                    }
                }
               
                
            }
        }
        IsKill();
    }
    public void Freeze(Player _player)
    {

        if (_player.Car.Debuffs.ContainsKey("Frozen"))
        {
            MissileBase freezeMiss = NetworkManager.instance.InstantiateMissile(id,6,transform.position,Quaternion.LookRotation(-transform.forward,Vector3.up));

            freezeMiss.SetParent(_player.gameObject);
            freezeMiss.target = _parent.GetComponent<Target>();
        }
        else
        {

            Debuff burn = new Debuff();
            burn.action = null;
            burn.time = 5f;
            _player.Car.Debuffs.Add("Frozen",burn);
            ServerSend.ToggleDebuff(_player.id,"Frozen");
        }
        

    }
    public void Burning(Player _player)
    {
        if (_player.Car.Debuffs.ContainsKey("Burning"))
        {
            _player.Car.Debuffs["Burning"].time = 5f;
        }
        else
        {
            Debuff burn = new Debuff();
            burn.action = Debuff.Burning;
            burn.time = 5f;
            _player.Car.Debuffs.Add("Burning",burn);
            ServerSend.ToggleDebuff(_player.id,"Burning");
        }

    }
    public void IsKill()
    {
        Server.Missiles.Remove(id);
        ServerSend.MissileDestroyed(id);
        Destroy(gameObject);//Dead... not big surprise.
    }
    private void DetachParticle()
    {
       // emit.transform.parent = null;
        //emit.Stop();
        //Destroy(emit.gameObject,5);
    }
}
