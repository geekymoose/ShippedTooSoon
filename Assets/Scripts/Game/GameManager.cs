using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class GameManager : MonoBehaviour {
	private GameMap gameMap;

	// Use this for initialization
	void Start () {
		Debug.Log("Start Game");
		this.gameMap = Resources.Load<GameMap>("GameMap") as GameMap;
		Assert.IsNotNull(this.gameMap, "Unable to load the GameMap Data");
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
