using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructable : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Set the maximum health value")]
    protected float maxHealth = 100f;
    protected float health;
    public void Start()
    {
        health = maxHealth;
    }
    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Destroy();
        }
    }
    //We give the client a rounded off version of the health for a cleaner look.
    public int GetHealthInt()
    {
        return Mathf.RoundToInt(health);
    }
    public void SetHealth(float health)
    {
        this.health = health;
    }
    /// <summary>
    /// Safely Destroy object
    /// </summary>
    public virtual void Destroy()
    {
        ThreadManager.ExecuteOnMainThread(() =>
        {
            UnityEngine.Object.Destroy(gameObject);
        });
    }
}
