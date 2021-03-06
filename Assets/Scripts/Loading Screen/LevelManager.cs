﻿//Name:			LevelManager.cs
//Project:		Spectral: The Silicon Domain
//Author(s)		Conor Hughes - conormpkhughes@yahoo.com
//Description:	Loads a selected level based on the level door and button press.


using UnityEngine;
using System.Collections;

public class LevelManager : MonoBehaviour {

	private bool 			displayButton;				//determines if button visible
	private Transform 		player;						//the player transform
	
	private RectTransform 	buttonTransform, 			//button transform
							canvasTransform;			//canvas transform
	
	private Vector2 		ViewportPosition, 			//the screen position of the door
							WorldObject_ScreenPosition;	//the target button position
	
	private ScreenFade 		sFade;						//screen fade instance
	
	private GameState 		gameState;					//gamestate instance
	
	public 	int 			levelIndex;					//the selected level index
	private Transform 		currentDoor;				//the current door transform
	
	// Use this for initialization
	void Start () {
		player = GameObject.FindGameObjectWithTag("Player").transform;
		canvasTransform = GameObject.Find("Canvas").GetComponent<RectTransform>();
		buttonTransform = GameObject.Find("Level Button").GetComponent<RectTransform>();
		sFade = GameObject.Find("Screen Fade").GetComponent<ScreenFade>();
	}
	
	// Update is called once per frame
	void Update () {
		if(levelIndex > 0)displayButton = true;
		else displayButton = false;
		
		ScaleButton();
		PositionButton();
	}

	//Sets the current level door
	public void SetCurrentDoor(Transform t){
		currentDoor = t;
	}

	//SHows/Hides the button
	void ScaleButton(){
		if(displayButton){
			buttonTransform.localScale = Vector3.Lerp(buttonTransform.localScale, new Vector3(0.8f, 0.8f, 0.8f), Time.deltaTime * 22);
		}
		else{
			buttonTransform.localScale = Vector3.Lerp(buttonTransform.localScale, new Vector3(0, 0, 0), Time.deltaTime * 22);
		}
	}

	//Sets button screen position
	void PositionButton(){
		if(displayButton){
			ViewportPosition=Camera.main.WorldToViewportPoint(currentDoor.position);
			WorldObject_ScreenPosition =new Vector2(
				((ViewportPosition.x*canvasTransform.sizeDelta.x)-(canvasTransform.sizeDelta.x*0.5f)),
				((ViewportPosition.y*canvasTransform.sizeDelta.y)-(canvasTransform.sizeDelta.y*0.5f)));
			
			buttonTransform.anchoredPosition=WorldObject_ScreenPosition;
		}
	}

	//Instantiates level loader object and selects next level
	public void EndLevel(){
		GameObject loadObject = Instantiate(Resources.Load("Basic Level Loader")) as GameObject;
		
		if (!gameState) {
			gameState = GameObject.Find("GameState").GetComponent<GameState>();
		}
		gameState.SaveGame ();

		switch(levelIndex){
			case 1 : print("Loading Stage 1");
			loadObject.GetComponent<BasicLevelLoader>().SetNextLevel("Gamma 7 rXoji");
			break;

			case 2 : print("Loading Stage 2");
			loadObject.GetComponent<BasicLevelLoader>().SetNextLevel("Ac-Me Processing");
			break;

			case 3 : print("Loading Stage 3");
			loadObject.GetComponent<BasicLevelLoader>().SetNextLevel("Preon Data Storage");
			break;

			case 4 : print("Loading Stage 4");
			loadObject.GetComponent<BasicLevelLoader>().SetNextLevel("Hk.U m@@A Server");
			break;

			case 5 : print("Loading Stage 5");
			loadObject.GetComponent<BasicLevelLoader>().SetNextLevel("Red Lotus Cryptography");
			break;

			case 6 : print("Loading Stage 6");
			loadObject.GetComponent<BasicLevelLoader>().SetNextLevel("A1R Systems");
			break;

			case 7 : print("Loading Stage 7");
			loadObject.GetComponent<BasicLevelLoader>().SetNextLevel("[Blue Box] Security");
			break;

		}
	}
}
