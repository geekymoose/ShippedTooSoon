using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class GameManager : MonoBehaviour {
	private GameMap gameMap = null;


	// Use this for initialization
	void Start () {
		Debug.Log("GameManager::Start()");
		//this.gameMap = (GameMap)GameObject.Find("GameMap") as GameObject;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
