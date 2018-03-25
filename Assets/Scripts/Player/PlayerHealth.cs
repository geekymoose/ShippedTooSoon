using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour {
    public Stats healthStats;
    public PanelHpPlayerBar panelHp;
    [Range(0, 100)]
    public float hp;

    [Range(0,100)]
    public float maxHp;



	// Use this for initialization
	void Start () {
        //set all the starting value for the player
        healthStats.SetValue(hp, maxHp, 1);
        panelHp.SetHealthBar(healthStats.GetValuePercent());
	}

    private void Update()
    {
        
    }

    //player take damage
    public void TakeDamage(float dmgValue)
    {
        healthStats.ReduceValue(dmgValue);

        if (healthStats.GetValue() <= 0)
        {
            Die();
        }
    }

    //if hp <=0 player die
    public void Die()
    {
        Debug.Log("player si dead");
    }

    //get the hp Percent
    public float GetHPPercent()
    {
        return healthStats.GetValuePercent();
    }
}
