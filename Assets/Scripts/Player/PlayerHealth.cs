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
    [SerializeField] // To display in editor
    private float           _maxHp = 100f; // Default value

    private float           _currentHP = 0f;

    private Image           _healthHPImgUI = null;
    private Animator        _anim = null;
    private Animator        _animUI = null;

    private PlayerMovement  _playerMovement = null;
    

    // -------------------------------------------------------------------------
    // Unity Methods
    // -------------------------------------------------------------------------

	void Start () {
        this._currentHP = this._maxHp;

        GameObject objImgHPUI = GameObject.Find("PlayerHPImgUI");
        GameObject objGameStateUI = GameObject.Find("CanvasUI_GameStat");

        Assert.IsNotNull(objImgHPUI, "Unable to find player HP UI");
        Assert.IsNotNull(objGameStateUI, "Unable to find UI panel");

        this._healthHPImgUI = objImgHPUI.GetComponent<Image>();
        this._playerMovement = this.GetComponent<PlayerMovement>();
        this._anim = this.GetComponent<Animator>();
        this._animUI = objGameStateUI.GetComponent<Animator>();

        Assert.IsNotNull(this._healthHPImgUI, "Unable to get Image component from PlayerUI");
        Assert.IsNotNull(this._playerMovement, "Unable to get PlayerMovement");
        Assert.IsNotNull(this._anim, "Unable to get Animator");
        Assert.IsNotNull(this._animUI, "Unable to get UI Animator");

        this.updateHealthUI();
	}


    // -------------------------------------------------------------------------
    // UI Methods
    // -------------------------------------------------------------------------

    private void updateHealthUI() {
        float fillAmout = this._currentHP / this._maxHp;
        this._healthHPImgUI.fillAmount = fillAmout;
    }
    

    // -------------------------------------------------------------------------
    // GamePlay Methods
    // -------------------------------------------------------------------------
    public void takeDammage(float dmgValue) {
        this._currentHP -= dmgValue;
        this._currentHP = Mathf.Clamp(this._currentHP, 0, this._maxHp);
        this.updateHealthUI();
        this._anim.SetTrigger("TakeDamage");
        this._animUI.SetTrigger("TakeDamage");
        
        AkSoundEngine.PostEvent("fx_hit", gameObject);

        if(this._currentHP <= 0) {
            this.die();
        }
    }

    public void die() {
        this._currentHP = 0;
        this._playerMovement.FreezeMovement();
        this.updateHealthUI();
    }

    public void healFull() {
        this._currentHP = this._maxHp;
        this.updateHealthUI();
    }
    

    // -------------------------------------------------------------------------
    // Getters / Setters
    // -------------------------------------------------------------------------
    public bool isAlive() {
        return this._currentHP > 0;
    }
}
