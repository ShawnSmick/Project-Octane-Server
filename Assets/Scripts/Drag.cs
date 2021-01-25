using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drag : MonoBehaviour
{
    [SerializeField]
    [Tooltip("How much local vertical velocity is kept per physics tick, used in reducing bounce on the springs. only works while grounded")]
    private float stiffness = 7f;//decay of vertical momentum while grounded.
    [SerializeField]
    [Tooltip("How much local side velocity is kept per physics tick, higher values = more drift. . only works while grounded")]
    private float sideDrag = 4f;//Decay of sideways momentum while grounded
    [SerializeField]
    private float sideDragReduction = 3f;//Decay of sideways momentum while grounded
    [SerializeField]
    [Tooltip("How much local frontal velocity is kept per physics tick while accelerating, effects max speed. only works while grounded")]
    private float frontDrag = 1.25f;//decay of vertical momentum while grounded.
    [SerializeField]
    [Tooltip("How much local frontal velocity is kept per physics tick while you are not accelerating. only works while grounded")]
    private float frontStopDrag = 2.5f;

    private CarEngine parent;
    private void Start()
    {
        parent = GetComponent<CarEngine>();

    }
    public void InflictDrag()
    {
        int traction = parent.GetTraction();
        if (traction >= 2)
        {
            VelocityDecay(1f - (sideDrag - (parent.IsFastTurn() ? sideDragReduction : 0)) * Time.fixedDeltaTime,1f - stiffness * Time.fixedDeltaTime,1f - (parent.GetInputY() != 0 ? frontDrag * Time.fixedDeltaTime : frontStopDrag * Time.fixedDeltaTime));
          
            
        }
        else if (traction ==1)
        {
            VelocityDecay(1f - (sideDrag) * Time.fixedDeltaTime,1f - stiffness * Time.fixedDeltaTime,1f - (frontStopDrag) * Time.fixedDeltaTime);

        }
    }
    private void VelocityDecay(float decX,float decY,float decZ)
    {
        Vector3 vel = transform.InverseTransformDirection(parent.getRigidBody().velocity); //Convert the rigidbody's world velocity into a local space
        vel = new Vector3(vel.x * decX,vel.y * decY,vel.z * decZ); //Decay the local sideways velocity
        parent.getRigidBody().velocity = transform.TransformDirection(vel); //Convert and update local velocity to the world
    }
}
