﻿/// <summary>
/// 2015
/// Ben Redahan, redahanb@gmail.com
/// Project: Spectral - The Silicon Domain (Unity)
/// GameState component: handles the GameState for transitions between scenes, Saving and Loading savefiles
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class GameState : MonoBehaviour {

	public	static 		GameState 			gameState;

	private 			HealthManager 		pHealth;
	private 			HUD_Healthbar		healthHUD;
	private 			PlayerInventory		pInventory;
	private 			InventoryItem 		invItem;
	private 			HUD_Inventory 		invHUD;
	private 			TimeScaler 			pTime;
	private 			Color 				tempColor;
	
	// Singleton design pattern - there can only be one GameState Object: the one from the first scene loaded
	void Awake () 
	{
		if (gameState == null) {
			// If this is the first scene loaded, set this as the GameState object
			DontDestroyOnLoad (gameObject);
			gameState = this;
		} else {
			// If a GameState object already exists, destroy this object, leaving the pre-existing GameState in the scene
			if(gameState != gameObject){
				Destroy(gameObject);
			}
		}
	}
	
	void Start()
	{
		// Cache references to scripts that utilise GameState variables
		pHealth = GameObject.Find ("Health Manager").GetComponent<HealthManager> ();
		healthHUD = GameObject.Find ("HUD_Healthbar").GetComponent<HUD_Healthbar> ();
		pInventory = GameObject.Find ("Inventory Manager").GetComponent<PlayerInventory> ();
		invHUD = GameObject.Find ("HUD_Inventory").GetComponent<HUD_Inventory> ();
		pTime = GameObject.Find("Time Manager").GetComponent<TimeScaler>();

		// Load game at the start of every scene
		LoadGame ();
	}

	void Update () 
	{

	}

	public void SaveGame()
	{
		// Create a biary formatter, save file and empty savedata object
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Create (Application.persistentDataPath + "/playerInfo.dat");
		SaveData data = new SaveData ();

		// Store player stats into savedata
		data.currentHealth = pHealth.playerHealth;
		data.maxHealth = pHealth.maxHealth;

		data.inventorySize = pInventory.inventorySize;

		// Inventory contents have to be stored as primitive values: can't use binary formatter on GameObjects or prefabs
		data.itemNames = new string[pInventory.inventorySize];
		data.itemValues = new string[pInventory.inventorySize];
		data.itemColours = new float[pInventory.inventorySize][];

		for(int x = 0; x < pInventory.playerInventory.Length; x++)
		{ // Store the key parameters for each item in the inventory 
			if(pInventory.playerInventory[x] != null){
				InventoryItem tempInfo = pInventory.playerInventory[x].GetComponent<InventoryItem>();
				data.itemNames[x] = tempInfo.name;
				tempColor = tempInfo.itemColor;
				//data.itemColors[x] = new Vector3(tempColor.r, tempColor.b, tempColor.g);
				float[] tempArray = new float[3]{tempColor.r, tempColor.b, tempColor.g};
				data.itemColours[x] = tempArray;
				data.itemValues[x] = tempInfo.itemValue;
			}
		}

		// Save time upgrade parameters
		data.maxStoredTime = pTime.GetMaxStoredTime ();
		data.noiseDampening = pTime.GetNoiseDampening ();

		// Serialize and commit the savedata into the savefile
		bf.Serialize (file, data);
		file.Close ();
		print ("Saving Game...");
	} // end SaveGame

	public void LoadGame()
	{
		BinaryFormatter bf = new BinaryFormatter();
		SaveData data;

		if (File.Exists (Application.persistentDataPath + "/playerInfo.dat")) {
			print ("Save file found!");
			FileStream file = File.Open (Application.persistentDataPath + "/playerInfo.dat", FileMode.Open);
			data = (SaveData)bf.Deserialize (file);
			file.Close ();

			// Update player stats from save file
			pHealth.maxHealth = data.maxHealth;
			pHealth.playerHealth = data.maxHealth; // give player full health at start of a new scene
			pInventory.inventorySize = data.inventorySize;
			pInventory.playerInventory = new GameObject[pInventory.inventorySize];

			// Compile inventory contents
			for(int x = 0; x < data.inventorySize; x++){
				if(data.itemNames[x] != null)
				{
					GameObject tempItem = Instantiate( Resources.Load("Pickups/" + data.itemNames[x]), new Vector3(-50,-50,-50), Quaternion.identity ) as GameObject;
					invItem = tempItem.GetComponent<InventoryItem>();

					// Update item stats
					invItem.name = data.itemNames[x];
					invItem.itemName = data.itemNames[x];
					invItem.itemValue = data.itemValues[x];
					tempColor = new Color();
					tempColor.r = data.itemColours[x][0];
					tempColor.b = data.itemColours[x][1];
					tempColor.g = data.itemColours[x][2];
					tempColor.a = 1.0f;
					invItem.itemColor = tempColor;

					// Hide and disable the item, and add to player inventory
					pInventory.playerInventory[x] = tempItem;
					}
			}

			// Build inventory HUD and update the icons
			invHUD.buildInventoryUI (pInventory.inventorySize);
			invHUD.UpdateAllIcons();

			// Build the health HUD, set health to full
			healthHUD.buildHealthbarUI(pHealth.maxHealth);
			healthHUD.healthBarSize = pHealth.maxHealth;

			// Set time upgrade data
			pTime.SetMaxStoredTime(data.maxStoredTime);
			pTime.SetNoiseDampening(data.noiseDampening);
		} 
		else {
			// If no save file, use the default settings
			print ("No save file found... Initialising default player stats.");
			// Default health
			pHealth.maxHealth = 3;
			pHealth.playerHealth = 3;

			// Default inventory
			pInventory.inventorySize = 4;
			pInventory.playerInventory = new GameObject[4];

			// Build default HUD
			invHUD.buildInventoryUI (4);
			healthHUD.buildHealthbarUI(3);
			healthHUD.healthBarSize = 3;

			// Default time upgrades
			pTime.SetMaxStoredTime(5.0f);
			pTime.SetNoiseDampening(false);
		}
	} // end LoadGame

	public void ResetGame()
	{
		// Function for use in the Restore Point, clear all data in the save file, restart the game
		print ("Clearing saved data...");

		// Default health
		pHealth.maxHealth = 3;
		pHealth.playerHealth = 3;

		// Default inventory
		pInventory.inventorySize = 4;
		pInventory.playerInventory = new GameObject[4];

		// Default time upgrades
		pTime.SetMaxStoredTime(5.0f);
		pTime.SetNoiseDampening(false);

		// Save data to the save file
		SaveGame ();

		// Restart the game from scene 0 (splash screen)
		Application.LoadLevel (0);
	}

	/// SAVE FILE DATA MODEL ///
	[Serializable]
	private class SaveData
	// Private class to serve as a container for all relevant data to be saved
	{
		// Health Data
		public int 				currentHealth;
		public int 				maxHealth;

		// Inventory Data
		public int 				inventorySize;

		public string[] 		itemNames;
		public string[] 		itemValues;
		public float[][] 		itemColours;

		// Time Upgrade Data
		public float 			maxStoredTime;
		public bool 			noiseDampening;

	} // end SaveData class
	
}
