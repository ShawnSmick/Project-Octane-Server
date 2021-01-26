using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkObject : MonoBehaviour
{
    private int id;
    public int GetID()
    {
        return this.id;
    }
    public void SetID(int id)
    {
        this.id = id;
    }
    public virtual void NetworkFixedUpdate()
    {
        ServerSend.NetworkObjectUpdate(this);
    }
}
