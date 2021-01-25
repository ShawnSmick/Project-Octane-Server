using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private float MaxSpeed = 30f;
    [SerializeField]
    private float MaxCorrection = 20f;
    [SerializeField]
    private float Acceleration = 1f;
    private float CurrentSpeed;
    private float CurrentCorrection;
    [SerializeField]
    private float CorrectionSpeed = 1f;
    [SerializeField]
    private float heightMulti = 1.5f;
    [SerializeField]
    private float trackingDelay =0f;
    public Vector3 Velocity;
    private MissileBase _missileBase;

    void Start()
    {
        Velocity = new Vector3();
        _missileBase = GetComponent<MissileBase>();
        _missileBase.speed = 0;
        if (_missileBase.target != null)
        {
            _missileBase.Targeting = true;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //transform.LookAt(dummythicc.target.transform.position);
        float distance = _missileBase.target != null?Vector3.Distance(new Vector3(_missileBase.target.transform.position.x,0,_missileBase.target.transform.position.z),new Vector3(transform.position.x,0,transform.position.z)):2;

            CurrentSpeed += Acceleration;
        
        
        if (CurrentSpeed > MaxSpeed)
        {
            CurrentSpeed = MaxSpeed;
        }
        if (_missileBase.aliveTime > trackingDelay && _missileBase.target != null)
        {
            CurrentCorrection += CorrectionSpeed;
            if (CurrentCorrection > MaxCorrection)
            {
                CurrentCorrection = MaxCorrection;
            }
            _missileBase.speed = 0;
            Vector3 direction = (new Vector3(_missileBase.target.transform.position.x,0,_missileBase.target.transform.position.z) - new Vector3(transform.position.x,0,transform.position.z)).normalized;
            Velocity += direction * CurrentCorrection * Time.fixedDeltaTime;
            if (Velocity.magnitude > MaxSpeed * Time.fixedDeltaTime)
            {
                Velocity = Velocity.normalized * MaxSpeed * Time.fixedDeltaTime;
            }






            transform.position += Velocity;
            
        }
        else
        {
            Velocity += transform.forward * Acceleration * Time.fixedDeltaTime;
            if (Velocity.magnitude > MaxSpeed * Time.fixedDeltaTime)
            {
                Velocity = Velocity.normalized * MaxSpeed * Time.fixedDeltaTime;
            }
            _missileBase.speed = CurrentSpeed;
        }
    }
}
