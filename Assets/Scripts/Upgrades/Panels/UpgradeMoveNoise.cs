﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UpgradeMoveNoise : MonoBehaviour {
	
	public 	bool 	noiseVisible = false;
	public 	int 	currentNoise = 5;
	
	private int 	currentIndex = 0, 
	nextIndex = 0;
	
	private bool 	fadeIn = true;
	
	public bool		displayPanel = false;

	public bool dampeningEnabled = false;
	
	private Color 	activeColor, 
	inactiveColor, 
	nextUpgradeColorVisible, 
	nextUpgradeColorTransparent;
	
	private Vector3 hiddenPos, 
	visiblePos;
	
	private 		RawImage[] noiseImages;
	private 		RectTransform noiseInfoTransform;
	
	
	Image		noiseButtonImage;
	
	Color		buttonTargetColor = Color.white;

	Text		statusText;

	GameObject upgradeButton;

	TimeScaler tScaler;

	// Use this for initialization
	void Start () {
		
		noiseButtonImage = GameObject.Find("Move Noise Button").GetComponent<Image>();
		buttonTargetColor = Color.white;
		noiseInfoTransform = transform.GetComponent<RectTransform>();
		
		hiddenPos = noiseInfoTransform.position;
		visiblePos = noiseInfoTransform.position + new Vector3(340,0,0);
		noiseInfoTransform.position = hiddenPos;
		
		activeColor = Color.white;
		inactiveColor = new Color(1,1,1,0.1f);
		
		nextUpgradeColorVisible 	= new Color(0,1,0,1);
		nextUpgradeColorTransparent = new Color(0,1,0,0.05f);

		statusText = GameObject.Find("Dampening Status").GetComponent<Text>();
		upgradeButton = transform.Find("UpgradeButton").gameObject;
		tScaler = GameObject.Find("Time Manager").GetComponent<TimeScaler>();
		dampeningEnabled = tScaler.noiseDampening;
		SetStatusText();
	}
	
	
	
	
	// Update is called once per frame
	void Update () {
		
		MovePanel();
		if(Vector3.Distance(noiseInfoTransform.position,visiblePos) < 5){
			//FillNoise();
			noiseVisible = true;
		}
		if(Vector3.Distance(noiseInfoTransform.position,visiblePos) > 330){
			//ClearNoise();
			noiseVisible = false;
		}
	}
	
	void MovePanel(){
		if(displayPanel){
			noiseInfoTransform.position = Vector3.Lerp(noiseInfoTransform.position, visiblePos, 0.1f);
			buttonTargetColor = Color.green;
		}
		else{
			noiseInfoTransform.position = Vector3.Lerp(noiseInfoTransform.position, hiddenPos, 0.1f);
			buttonTargetColor = Color.white;
		}
		noiseButtonImage.color = Color.Lerp(noiseButtonImage.color,buttonTargetColor, Time.deltaTime * 5);
	}
	

	void SetStatusText(){
		if(dampeningEnabled){
			statusText.text = "ENABLED";
			statusText.color = Color.green;
			Destroy(upgradeButton);
		}
		else{
			statusText.text = "DISABLED";
			statusText.color = Color.white;
		}
	}

	
	public void IncreaseNoise(){
		dampeningEnabled = true;
		SetStatusText();
		currentNoise = currentNoise + 1;
	}
}
