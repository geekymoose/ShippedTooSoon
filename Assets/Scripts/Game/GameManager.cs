using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;


/**
 * General GameManager.
 *
 * \author Constantin
 */
public class GameManager : MonoBehaviour {
	// Game
	private GameMap 			gameMap = null;
	private PlayerMovement 		playerMovement = null;
	private PlayerHealth		playerHealth = null;
	private CameraController 	roomCamera = null;
	private GameTimeManager 	timeManager = new GameTimeManager();
	private ScoreData 			scoreData = null;

	// Gameplay
	private Transform 			spawnPoint = null;
	private float 				stopwatchTime = 0.0f;
	private bool 				isRunning = false;

	// Room management
	private Room 				currentRoom = null;
	private Room 				previousRoom = null;
	private bool 				hasSwitchedRoom = false;

	// UI (Player State)
	private Text 				goalCounterDoneTextUI = null;
	private Text 				goalCounterUndoneTextUI = null;
	private Text 				timeCounterTextUI = null;

	private MenuManager 		menuManager = null;

	// Debug / Editor
	private GameObject 			gameMapCreator; // Used in editor to create GameMap.

	private string 				scoreDataPath = "ScoreDataSave";


	// -------------------------------------------------------------------------
	// Unity Methods
	// -------------------------------------------------------------------------

	public void Awake() {
        if(Debug.isDebugBuild) {
			this.scoreDataPath = "ScoreDataSave_Debug";
		}
		this.scoreData = Resources.Load(scoreDataPath) as ScoreData;
		Assert.IsNotNull(this.scoreData, "Unable to find the ScoreData Resources! Oooh!");
	
		// Warning: this caused several bugs in build, it is safer to disabled manually before build
		this.gameMapCreator = GameObject.Find("GameMapCreator");
		if(this.gameMapCreator != null) {
			Debug.LogWarning("GameMapCreator must be disabled manually if you want to Build the game (Works fine in editor)");
			// gameMapCreator is just used to create the map by game designer.
			// If env is still present in editor. Must be removed first!
			// (Because the env is re-generated at runtime)
			// GameObject.Destroy(this.gameMapCreator);
		}
	}

	public void Start () {
		Debug.Log("GameManager::Start()");

		GameObject gameMapObject 		= GameObject.Find("GameMap");
		GameObject cameraObject 		= GameObject.Find("Main Camera");
		GameObject spawnObject 			= GameObject.Find("SpawnPoint");
		GameObject objGoalDoneText 		= GameObject.Find("GoalCounterDoneTextUI");
		GameObject objGoalUndoneText 	= GameObject.Find("GoalCounterUndoneTextUI");
		GameObject timeCounterObject 	= GameObject.Find("TimeCounterTextUI");
		GameObject objMenuCanvaslUI 	= GameObject.Find("CanvasUI_MenuGame");
		GameObject playerObject 		= GameObject.FindGameObjectWithTag("Player");

		Assert.IsNotNull(gameMapObject, "Unable to find GameMap object in scene");
		Assert.IsNotNull(cameraObject, "Unable to find Main Camera GameObject");
		Assert.IsNotNull(playerObject, "Unable to recover the Player GameObject");
		Assert.IsNotNull(spawnObject, "Unable to recover the SpawnObject GameObject");
		Assert.IsNotNull(objGoalDoneText, "Unable to find GoalCounter Object");
		Assert.IsNotNull(objGoalUndoneText, "Unable to find GoalCounter Object");
		Assert.IsNotNull(timeCounterObject, "Unable to find TimeCounter Object");
		Assert.IsNotNull(objMenuCanvaslUI, "Unable to find MenuUI object");

		this.gameMap 					= gameMapObject.GetComponent<GameMap>();
		this.playerMovement 			= playerObject.GetComponent<PlayerMovement>();
		this.playerHealth 				= playerObject.GetComponent<PlayerHealth>();
		this.roomCamera 				= cameraObject.GetComponent<CameraController>();
		this.spawnPoint 				= spawnObject.transform;

		this.goalCounterDoneTextUI		= objGoalDoneText.GetComponent<Text>();
		this.goalCounterUndoneTextUI 	= objGoalUndoneText.GetComponent<Text>();
		this.timeCounterTextUI 			= timeCounterObject.GetComponent<Text>();
		this.menuManager 				= objMenuCanvaslUI.GetComponent<MenuManager>();

		Assert.IsNotNull(this.gameMap, "Unable to recover GameMap script from GameMap Object");
		Assert.IsNotNull(this.roomCamera, "Unable to recover CameraController script");
		Assert.IsNotNull(this.playerMovement, "Unable to recover the PlayerMovement script");
		Assert.IsNotNull(this.playerHealth, "Unable to recover the PlayerHealth script");
		Assert.IsNotNull(this.goalCounterDoneTextUI, "Unable to recover Text component from Goal Counter");
		Assert.IsNotNull(this.goalCounterUndoneTextUI, "Unable to recover Text component from Goal Counter");
		Assert.IsNotNull(this.timeCounterTextUI, "Unable to recover Text component from Time Counter");
		Assert.IsNotNull(this.menuManager, "Unable to get the MenuManager Script");

		// Init Game (Important)
		this.playerMovement.transform.position = this.spawnPoint.position;

		this.currentRoom = this.gameMap.getRoomUnderWorldPos(this.playerMovement.transform.position);
		this.currentRoom.onRoomEnter();
		this.currentRoom.setActive(true);
		this.previousRoom = this.currentRoom;
		

		this.isRunning = true;
		this.timeManager.startStopwatch();

		AkSoundEngine.PostEvent("mus_start_bug", gameObject);
	}
	
	public void Update () {
		if(Input.GetButtonDown("Leave")) {
			this.menuManager.leave();
		}
		else if(Input.GetButtonDown("Restart")) {
			this.menuManager.restart();
		}
		
		if(this.isRunning) {
			this.updateTimeCounter();
			this.updateGoalCounter();
			this.updateCurrentRoom();
			this.updateCameraPosition();
			this.updateVictory();

			if(this.hasSwitchedRoom) {
				this.previousRoom.onRoomExit();
				this.currentRoom.onRoomEnter();
				this.previousRoom = this.currentRoom;
			}
		}
	}


	// -------------------------------------------------------------------------
	// GamePlay Methods
	// -------------------------------------------------------------------------

	public void respawnPlayer() {
		this.playerMovement.transform.position = this.spawnPoint.position;
	}

	public void victory() {
		this.isRunning = false;
		this.playerMovement.FreezeMovement();
		this.timeManager.stopStopwatch();

		this.stopwatchTime = this.timeManager.getStopwatchTime();
		this.timeManager.stopStopwatch();

		this.scoreData.addScoreEntry(this.stopwatchTime);
		
		this.menuManager.showVictory();
		
        AkSoundEngine.PostEvent("mus_victory", gameObject);
	}

	public void gameOver() {
		this.isRunning = false;
		this.playerMovement.FreezeMovement();
		this.timeManager.stopStopwatch();

		this.timeManager.stopStopwatch();
		this.stopwatchTime = this.timeManager.getStopwatchTime();
		
		this.menuManager.showGameOver();

        AkSoundEngine.PostEvent("end", gameObject);
	}

	public void restart() {
		this.menuManager.restart();
	}


	// -------------------------------------------------------------------------
	// Update Methods
	// -------------------------------------------------------------------------
	private void updateCurrentRoom() {
		this.currentRoom = this.gameMap.getRoomUnderWorldPos(this.playerMovement.transform.position);

		if(this.currentRoom != null) {
			this.hasSwitchedRoom = false;
			if(this.currentRoom.getId() != this.previousRoom.getId()) {
				this.hasSwitchedRoom = true;

				// This is had-hoc solution to start boss sound
				if(this.currentRoom.getId() == 7) {
					AkSoundEngine.PostEvent("fx_monster", gameObject);
				}
			}
		}
		else {
			this.respawnPlayer();
		}
	}

	private void updateCameraPosition() {
		if(this.currentRoom != null) {
			Vector3 center = this.gameMap.getCellCenterWorldFromId(this.currentRoom.getId());
			this.roomCamera.targetPosition = center;
		}
	}

	private void updateVictory() {
		int remaining = this.getNbRemainingGoals();
		if(remaining == 0) {
			this.victory();
		}
		if(!this.playerHealth.isAlive()) {
			this.gameOver();
		}
	}

	private void updateGoalCounter() {
		int remaining 	= this.getNbRemainingGoals();
		int done 		= this.getNbGoalsDone();

		this.goalCounterDoneTextUI.text = done.ToString("0");
		this.goalCounterUndoneTextUI.text = remaining.ToString("0");
	}

	private void updateTimeCounter() {
		float time = this.timeManager.getStopwatchTime();
		string timeStr = ScoreData.formatScoreTimestamp(time);
		this.timeCounterTextUI.text = timeStr;
	}


	// -------------------------------------------------------------------------
	// Getters / Setters
	// -------------------------------------------------------------------------
	public GameTimeManager getTimeManager() {
		return this.timeManager;
	}

	/**
	 * Get how many goals remain.
	 */
	public int getNbRemainingGoals() {
		int remaining = this.gameMap.listRooms.Length;
		foreach(Room roro in this.gameMap.listRooms){
			if(roro.getIsDone() == true) {
				remaining--;
			}
		}
		return remaining;
	}

	public int getNbGoalsDone() {
		int remaining = this.gameMap.listRooms.Length;
		int totalGoals = 0;
		foreach(Room roro in this.gameMap.listRooms){
			totalGoals++;
			if(roro.getIsDone() == true) {
				remaining--;
			}
		}
		return totalGoals - remaining;
	}

	public ScoreData getScoreData() {
		return this.scoreData;
	}
}
