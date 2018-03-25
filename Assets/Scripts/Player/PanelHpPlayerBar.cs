using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelHpPlayerBar : MonoBehaviour {

    public RawImage healthBar;
    
    public RawImage background;
    
    //set the hp bar
    public void SetHealthBar(float healthPercent)
    {
        float xValue = healthPercent / 10f;
        healthBar.uvRect = new Rect(xValue, 0f, 0.5f, 1f);
    }


}
