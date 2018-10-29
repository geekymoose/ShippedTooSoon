using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/**
 * Manage Player Action such as Attacks and activate Goals or pickup glitches.
 */
public class PlayerAction : MonoBehaviour {
	// -------------------------------------------------------------------------
	// Attributes
	// -------------------------------------------------------------------------
	private PlayerMovement 			_playerMovement 	= null;
	private SpeechBubble			_speechBubble		= null;
	private bool 					_canAttack 			= false;
	private Transform 				_attackCenter 		= null;
	private CircleCollider2D 		_attackCollider 	= null;

    private Animator 				_anim 				= null;
    private Animator        		_animUI 			= null;


	// -------------------------------------------------------------------------
	// Attributes (Unity Editor)
	// -------------------------------------------------------------------------
	[SerializeField]
	[Tooltip("Speech that player will say when pickup this pickable")]
	private SpeechBubbleData _noWeaponMessage;

	[SerializeField]
	[Tooltip("Speech that player will say when pickup this pickable")]
	private SpeechBubbleData _enemyMessage;


	// -------------------------------------------------------------------------
	// Unity Methods
	// -------------------------------------------------------------------------
	public void Start() {
		_playerMovement 	= this.GetComponent<PlayerMovement>();
		_speechBubble 		= this.GetComponent<SpeechBubble>();
        _anim 				= this.GetComponent<Animator>();
		_attackCollider		= this.GetComponentInChildren<CircleCollider2D>();
		_attackCenter 		= GameObject.Find("PlayerAttackCenter").transform;
		_animUI 			= GameObject.Find("CanvasUI_GameStat").GetComponent<Animator>();

		Assert.IsNotNull(_playerMovement, "Unable to get playerMovement");
		Assert.IsNotNull(_attackCollider, "No range collider in player? :/");
		Assert.IsNotNull(_attackCenter, "Player's hand must be dragged on player script");
		Assert.IsNotNull(_anim, "Unable to get the player animator");
		Assert.IsNotNull(_animUI, "Unable to get UI Animator");

		_attackCollider.enabled = false; // Important. Used only for range value.
	}

	void Update () {
        if (Input.GetButtonDown("Fire1")) {
			if(_canAttack) {
				this.attack();
			}
			else {
				AkSoundEngine.PostEvent("fx_none", gameObject);
				_speechBubble.showBubble(_noWeaponMessage.message, _noWeaponMessage.duration);
			}
        }
	}

	public void OnCollisionEnter2D(Collision2D other) {
		if(other.gameObject.CompareTag("Enemy")) {
			_speechBubble.showBubble(_enemyMessage.message, _enemyMessage.duration);
		}
	}
	
    public void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Goal")) {
            RoomGoal roger = other.gameObject.GetComponent<RoomGoal>();
            Assert.IsNotNull(roger, "Goal object doesn't have a RoomGoal script");
			if(roger.getIsDone() == false) {
            	roger.activate();
				_animUI.SetTrigger("PickupGoal");
				AkSoundEngine.PostEvent("fx_button", gameObject);
				AkSoundEngine.PostEvent("fx_next", gameObject);
			}
        }
		else if(other.CompareTag("Pickable")) {
			Pickable pickable = other.GetComponent<Pickable>();
			Assert.IsNotNull(pickable, "Missing script on a Glitch Pickage");
			if(!pickable.isPickedup()) {
				pickable.pickup();
				_speechBubble.showBubble(pickable.data.message, pickable.data.duration);
				AkSoundEngine.PostEvent("fx_bulle", gameObject);
			}
		}
		else if(other.gameObject.name == "sword") {
			GameObject.Destroy(other.gameObject);
			this.pickupWeapon();
		}
    }


	// -------------------------------------------------------------------------
	// GamePlay Methods
	// -------------------------------------------------------------------------

	private void attack() {
		AkSoundEngine.PostEvent("fx_att", gameObject);
		_anim.SetTrigger("Attack");

		// Update position of attack center position
		float distance = Vector3.Distance(_attackCenter.transform.position, this.transform.position);
		Vector3 dir = _playerMovement.getDirection();
		Vector3 center = this.transform.position + (dir * distance);
		Vector2 attackCenter = new Vector2(center.x, center.y);

		Collider2D[] colliders = Physics2D.OverlapCircleAll(attackCenter, _attackCollider.radius/2);

		foreach(Collider2D coco in colliders) {
			if(coco.gameObject.CompareTag("Destructable")) {
				Destructable dd = coco.gameObject.GetComponent<Destructable>();
				Assert.IsNotNull(dd, "Destructable object without Destructable script :/");
				dd.destroy();
			}
		}
	}

	/**
	 * Pickup a weapon. Player can now attack.
	 * Plays pickup animation.
	 */
	public void pickupWeapon() {
		AkSoundEngine.PostEvent("fx_grab", gameObject);
		_playerMovement.FreezeMovement();
		_anim.SetTrigger("PickupSword");
		_canAttack = true;
		Invoke("internalUnfreeze", 0.6f);
	}

	// This is ugly, just to unfreeze movement after pickup animation
	private void internalUnfreeze() {
		_playerMovement.AllowMovement();
	}
}
