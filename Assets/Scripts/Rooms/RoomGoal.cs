using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/**
 * RoomGoal is the button unique in each room that player must pickup.
 */
public class RoomGoal : MonoBehaviour {
    // -------------------------------------------------------------------------
    // Attributes
    // -------------------------------------------------------------------------

	private bool 				isDone = false;
	private SpriteRenderer 		spriteRenderer;
	public Sprite 				greedSprite = null;


    // -------------------------------------------------------------------------
    // Unity Methods
    // -------------------------------------------------------------------------

	void Start () {
		this.spriteRenderer = this.GetComponent<SpriteRenderer>();
		Assert.IsNotNull(this.spriteRenderer, "Unable to recover SpriteRenderer from Goal Button");
	}


    // -------------------------------------------------------------------------
    // GamePlay Methods
    // -------------------------------------------------------------------------
	public bool getIsDone() {
		return this.isDone;
	}
	
	public void activate() {
		this.isDone = true;
		this.spriteRenderer = this.GetComponent<SpriteRenderer>(); // Sometime, bugs if recovered only in start
		Assert.IsNotNull(this.spriteRenderer, "Unable to recover SpriteRenderer from Goal Button");
		this.spriteRenderer.sprite = greedSprite;
	}
}
