using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTimeManager {
	private bool isFreezed = false;
	private float timeScale = 1.0f;
	private float minScale = 0.0f;
	private float maxScale = 2.0f;

	private float stopwatchStart = 0.0f;


	// -------------------------------------------------------------------------
	// Functions
	// -------------------------------------------------------------------------
	public void freezeGame() {
		Time.timeScale = 0.0f;
	}

	public void unFreezeGame() {
		Time.timeScale = timeScale;
	}

	public void slowDownGame(float value) {
		value = Mathf.Clamp(value, minScale, maxScale);
		Time.timeScale = value;
	}

	public void speedUpGame(float value) {
		value = Mathf.Clamp(value, minScale, maxScale);
		Time.timeScale = 1.5f;
	}

	public void startStopwatch() {
		this.stopwatchStart = Time.time;
	}


	// -------------------------------------------------------------------------
	// Getters / Setters
	// -------------------------------------------------------------------------
	public bool getIsFreezed() {
		return this.isFreezed;
	}

	public bool isSpeededUp() {
		return Time.timeScale > 1.0f;
	}

	public bool isSpeededDown() {
		return Time.timeScale < 1.0f;
	}

	public float getStopwatchTime() {
		return Time.time - this.stopwatchStart;
	}
}
