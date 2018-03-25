using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTimeManager {
	private bool isFreezed = false;
	private float timeScale = 1.0f;
	private float minScale = 0.0f;
	private float maxScale = 3.0f;

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
		float newTimeScale = Time.timeScale - value;
		newTimeScale = Mathf.Clamp(newTimeScale, minScale, maxScale);
		Time.timeScale = newTimeScale;
	}

	public void speedUpGame(float value) {
		float newTimeScale = Time.timeScale + value;
		newTimeScale = Mathf.Clamp(newTimeScale, minScale, maxScale);
		Time.timeScale = newTimeScale;
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
