using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
 * Simple gestion for time. (Like stopwatch etc)
 */
public class GameTimeManager {
	private bool 	_isFreezed = false;
	private float 	_timeScale = 1.0f;
	private float 	_minScale = 0.0f;
	private float 	_maxScale = 3.0f;

	private bool 	_isActive = false;
	private float 	_stopwatchStart = 0.0f;
	private float 	_stopwatchLastCheck = 0.0f;


	// -------------------------------------------------------------------------
	// Functions
	// -------------------------------------------------------------------------
	public void freezeGame() {
		Time.timeScale = 0.0f;
	}

	public void unFreezeGame() {
		Time.timeScale = _timeScale;
	}

	public void slowDownGame(float value) {
		float newTimeScale = Time.timeScale - value;
		newTimeScale = Mathf.Clamp(newTimeScale, _minScale, _maxScale);
		Time.timeScale = newTimeScale;
	}

	public void speedUpGame(float value) {
		float newTimeScale = Time.timeScale + value;
		newTimeScale = Mathf.Clamp(newTimeScale, _minScale, _maxScale);
		Time.timeScale = newTimeScale;
	}

	public void startStopwatch() {
		this._isActive = true;
		this._stopwatchStart = Time.time;
	}

	public void stopStopwatch() {
		this._isActive = false;
		this._stopwatchLastCheck = Time.time;
	}


	// -------------------------------------------------------------------------
	// Getters / Setters
	// -------------------------------------------------------------------------
	public bool getIsFreezed() {
		return this._isFreezed;
	}

	public bool isSpeededUp() {
		return Time.timeScale > 1.0f;
	}

	public bool isSpeededDown() {
		return Time.timeScale < 1.0f;
	}

	public float getStopwatchTime() {
		if(this._isActive) {
			this._stopwatchLastCheck = Time.time;
		}
		return this._stopwatchLastCheck - this._stopwatchStart;
	}

	public bool isActive() {
		return this._isActive;
	}
}
