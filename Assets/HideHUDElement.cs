﻿using UnityEngine;
using System.Collections;

public class HideHUDElement : MonoBehaviour {

	private float hideSpeed = 10.0f;
	public Vector3 showLocation;
	public Vector3 hideLocation;

	private RectTransform rectTran;
	private bool hideHUDPiece = false;

	// Use this for initialization
	void Start () {
		rectTran = GetComponent<RectTransform> ();
	}
	
	// Update is called once per frame
	void Update () {

		if(Input.GetKeyDown(KeyCode.H))
		{
			toggleHide();
		}

		if(hideHUDPiece)
		{
			hideHUD();
		} 
		else
		{
			returnHUD();
		}
	}

	void hideHUD()
	{
		rectTran.anchoredPosition = Vector3.Lerp (rectTran.anchoredPosition, hideLocation, Time.deltaTime*hideSpeed/2);
	}
	
	void returnHUD()
	{
		rectTran.anchoredPosition = Vector3.Lerp (rectTran.anchoredPosition, showLocation, Time.deltaTime*hideSpeed);
	}

	// Use this function if it gets called only once
	public void toggleHide(){
		hideHUDPiece = !hideHUDPiece;
	}

	// Use the functions below if they will get called repeatedly in Update
	public void hide(){
		hideHUDPiece = true;
	}

	public void show(){
		hideHUDPiece = false;
	}

}
