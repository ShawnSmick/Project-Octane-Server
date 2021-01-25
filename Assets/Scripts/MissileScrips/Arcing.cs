using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arcing : MonoBehaviour
{
    private MissileBase _missileBase;

    public float UpVelocity = 100;
    public float minVelocity = 10;
    public float MaxDown = -10;
    public float Gravity = 9.81f;
    public float DistanceScaling = 5f;
   
    void Start()
    {
        transform.rotation = Quaternion.Euler(0,transform.rotation.eulerAngles.y,0);
        _missileBase = GetComponent<MissileBase>();
        if (_missileBase.Targeting)
        {
            float distance = Vector3.Distance(new Vector3(_missileBase.target.transform.position.x,0,_missileBase.target.transform.position.z),new Vector3(transform.position.x,0,transform.position.z));
            Debug.Log(distance);
            if (distance < DistanceScaling)
            {
                UpVelocity *= (distance / (DistanceScaling*2f)+0.30f);
                if (UpVelocity < minVelocity)
                    UpVelocity = minVelocity;
            }

        }
    }
    private void FixedUpdate()
    {
        
        transform.position =new Vector3(transform.position.x,transform.position.y+( UpVelocity *Time.fixedDeltaTime),transform.position.z);
        UpVelocity -= Gravity;
        if (UpVelocity < MaxDown)
        {
            UpVelocity = MaxDown;
        }
    }
}
