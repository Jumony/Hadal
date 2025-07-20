using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public PlayerStatsSO playerStats;

    private int maxHealth;
    private int health;

    private void Awake()
    {
        maxHealth = playerStats.health;
        health = maxHealth;
    }
    
    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log("Took " + damage + " damage.\nPlayer now has " + health + " health");
    }
}
