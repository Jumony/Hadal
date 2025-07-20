using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Player Stats SO")]
public class PlayerStatsSO : ScriptableObject
{
    public int health;
    public int oxygen;

    public float moveForce;
    public float maxSpeed;
    public float waterDrag;
    public float buoyancyForce;
}
