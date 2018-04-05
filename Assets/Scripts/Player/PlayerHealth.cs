using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class PlayerHealth : MonoBehaviour {
    // -------------------------------------------------------------------------
    // Attributes
    // -------------------------------------------------------------------------
   
    [Range(0,100)]
    [Tooltip("Health at the start (And maximum possible if health reset)")]
    public float            maxHp = 100f; // Default value

    private float           currentHP = 0f;
    private Image           healthHPImgUI = null;

    private PlayerMovement  playerMovement;
    

    // -------------------------------------------------------------------------
    // Unity Methods
    // -------------------------------------------------------------------------

	void Start () {
        this.currentHP = this.maxHp;

        GameObject objUI = GameObject.Find("PlayerHPImgUI");
        Assert.IsNotNull(objUI, "Unable to find player HP UI");
        this.healthHPImgUI = objUI.GetComponent<Image>();
        this.playerMovement = this.GetComponent<PlayerMovement>();
        Assert.IsNotNull(this.healthHPImgUI, "Unable to get Image component from PlayerUI");
        Assert.IsNotNull(this.playerMovement, "Unable to get PlayerMovement");

        this.updateHealthUI();
	}


    // -------------------------------------------------------------------------
    // UI Methods
    // -------------------------------------------------------------------------

    private void updateHealthUI() {
        float fillAmout = this.currentHP / this.maxHp;
        this.healthHPImgUI.fillAmount = fillAmout;
    }
    

    // -------------------------------------------------------------------------
    // GamePlay Methods
    // -------------------------------------------------------------------------
    public void takeDammage(float dmgValue) {
        this.currentHP -= dmgValue;
        this.currentHP = Mathf.Clamp(this.currentHP, 0, this.maxHp);
        this.updateHealthUI();

        // TODO SOUND: Play sound damage

        if(this.currentHP <= 0) {
            this.die();
        }
    }

    public void die() {
        this.currentHP = 0;
        this.updateHealthUI();
        this.playerMovement.FreezeMovement();

        // TODO SOUND: Play die sound
    }
}
