using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OxygenBar : MonoBehaviour
{
    public Image oxygenBar;

    public void UpdateOxygenBar(float value)
    {
        oxygenBar.fillAmount = value;
    }
}
