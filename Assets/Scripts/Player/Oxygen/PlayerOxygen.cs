using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerOxygen : MonoBehaviour
{
    [Header("References")]
    public PlayerStatsSO playerStats;
    public OxygenBar oxygenBar;

    [Header("Parameters")]
    [Tooltip("How many seconds until oxygen decreases")]
    public int oxygenDecreaseInterval;
    [Tooltip("By how much does the oxygen decrease")]
    public int oxygenDecreaseStep;

    private int maxOxygen;
    private int oxygen;

    private float time = 0f;

    // Start is called before the first frame update
    void Start()
    {
        maxOxygen = playerStats.oxygen;
        oxygen = maxOxygen;
    }

    private void FixedUpdate()
    {
        time += Time.deltaTime;

        if (time >= oxygenDecreaseInterval && oxygen != 0)
        {
            oxygen -= oxygenDecreaseStep;
            float normalizedOxygen = (float)oxygen / maxOxygen;
            Debug.Log("Normalized Oxygen: " + normalizedOxygen);
            oxygenBar.UpdateOxygenBar(normalizedOxygen);
            time = 0f;
        }
    }
}
