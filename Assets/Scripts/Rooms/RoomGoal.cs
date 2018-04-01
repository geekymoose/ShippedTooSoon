using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class RoomGoal : MonoBehaviour {
	private bool isDone = false;
	private SpriteRenderer spriteRenderer;

	public Sprite redSprite = null;
	public Sprite greedSprite = null;

	// Use this for initialization
	void Start () {
		this.spriteRenderer = this.GetComponent<SpriteRenderer>();
		Assert.IsNotNull(this.spriteRenderer, "Unable to recover SpriteRenderer from Goal Button");
	}

	public bool getIsDone() {
		return this.isDone;
	}
	
	public void activate() {
		this.isDone = true;
		// Sometime, bugs if recovered only in start :/
		this.spriteRenderer = this.GetComponent<SpriteRenderer>();
		Assert.IsNotNull(this.spriteRenderer, "Unable to recover SpriteRenderer from Goal Button");
		this.spriteRenderer.sprite = greedSprite;
	}
}
