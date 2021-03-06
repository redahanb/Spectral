﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class NavMeshPatrolv3 : MonoBehaviour {

	// Patrol waypoints
	public GameObject point1;
	public GameObject point2;
	public GameObject point3;
	public GameObject A_point1;
	public GameObject A_point2;
	public GameObject A_point3;
	public GameObject sentryPoint;

	// Public variables
	public float pauseTime = 2;
	public bool StationaryGuard;

	// Private variables/arrays
	private Vector3[] patrolRoute;
	private Vector3[] alertRoute;
	private int nextIndex;
	private int alertIndex;
	private float curTime = 0;
	private bool walking = false;
	private bool paused = false;
	private Quaternion lookRotation;

	// enum of guard behaviour state for state machine
	private enum GuardState
	{
		Sentry, Patrol, AlertPatrol, Search, Investigate, Attack
	}
	private GuardState state;

	// Script/Component references
	private GameObject player;
	private NavMeshAgent navMesh;
	private EnemySight vision;
	private GuardAI alertAI;
	private Animator anim;
	
	void Start () {
		// Cache references to other scripts/components
		vision = gameObject.GetComponent<EnemySight> ();
		anim = GetComponent<Animator> ();
		alertAI = GetComponent<GuardAI> ();
		navMesh = gameObject.GetComponent<NavMeshAgent>();
		player = GameObject.FindWithTag ("Player");

		// initialise starting patrol points to 0 index
		nextIndex = 0;
		alertIndex = 0;

		// Compile array of waypoints for guard to patrol
		patrolRoute = new Vector3[4]
		{
			point1.transform.position, 
			point2.transform.position, 
			point3.transform.position, 
			point2.transform.position
		};

		// Ditto, for the extra points to patrols when alerted
		alertRoute = new Vector3[4]{
			A_point1.transform.position,
			A_point2.transform.position,
			A_point3.transform.position,
			A_point2.transform.position,
			//point1.transform.position,
			//point2.transform.position,
			//point3.transform.position
		};

		// start patrolling at walking speed
		navMesh.SetDestination (patrolRoute[nextIndex]);
		anim.SetFloat ("Speed", 1.0f);

	}

	void Update () {
		// set walking boolean for OnAnimatorMove function
		if (navMesh.hasPath) {
			walking = true;
		} else {
			walking = false;
		}

		//////////////////////////////
		/// Movement Decision tree ///
		//////////////////////////////

		if (vision.alerted == false){ 
			// when guard cannot see the player, either patrol the standard route or the alerted route
			if(alertAI.alertSystem.alertActive == true){ 
				// patrol extended route if the system is alerted
				//print ("Alert patrolling");
				AlertPatrol();
				navMesh.SetDestination (alertRoute [alertIndex]);
				if(!paused){
					anim.SetFloat ("Speed", 1.5f);
				}
			}
			else{
				//print ("Standard patrol");
				Patrol();
				navMesh.SetDestination(patrolRoute[nextIndex]);
				if(!paused){
					anim.SetFloat("Speed", 1.0f);
				}
			}
		} 
		else {
			// if alerted and player is visibile, attack
			if(vision.playerInSight){ 
				//if player is in sight, rotate towards player and attack
				lookRotation = Quaternion.LookRotation(player.transform.position - transform.position);
				transform.rotation = Quaternion.RotateTowards (transform.rotation, lookRotation, navMesh.angularSpeed * Time.deltaTime);
				anim.SetBool("InSight", true);
				Attack ();
			}
			// if alerted and player is not visible, search last known location
			else{ 
				// if player is not in sight (but guard is alarmed), chase and search
				anim.SetBool("InSight", false);
				Search ();
			}
		}
	}

	void OnAnimatorMove(){
		// Function to sync the walking animation root motion with the NavMesh Agent
		if (walking)
		{
			// Set NavMesh velocity to the root motion of the current frame of the current animation clip
			GetComponent<NavMeshAgent>().velocity = anim.deltaPosition / Time.deltaTime;

			if(navMesh.desiredVelocity != Vector3.zero) 
			{
				lookRotation = Quaternion.LookRotation (navMesh.desiredVelocity);
				transform.rotation = Quaternion.RotateTowards (transform.rotation, lookRotation, navMesh.angularSpeed * Time.deltaTime);
			}
		}
	}

	void Patrol() {
		// looping patrol route, moves from point to point in the array, pausing briefly at each destination
		if(Vector3.Distance(transform.position, patrolRoute[nextIndex]) <= 0.5f ){
			// stop moving for a brief time at each waypoint
			if (curTime == 0){
				curTime = Time.time;
				anim.SetFloat ("Speed", 0.0f);
				paused = true;
			}
			// wait for the designated pause time before moving to next patrol point
			if((Time.time - curTime) >= pauseTime){
				paused = false;
				nextPatrolPoint();
				curTime = 0;
			}
		}
	}

	void AlertPatrol() {
		// looping patrol route, with more waypoints and faster walking, for greater coverage of area
		if(Vector3.Distance(transform.position, alertRoute[alertIndex]) <= 0.5f ){
			// stop moving for a brief time at each waypoint
			if (curTime == 0){
				curTime = Time.time;
				anim.SetFloat ("Speed", 0.0f);
				paused = true;
			}
			// wait for the designated pause time before moving to next patrol point
			if((Time.time - curTime) >= pauseTime*0.75f){ // shorter pause time when on alert
				paused = false;
				nextAlertPoint();
				curTime = 0;
			}
		}
	}

	public void nextPatrolPoint() {
		// function to set the guard back on patrol, at walking speed
		navMesh.Resume ();
		// update the target waypoint
		if (nextIndex == patrolRoute.Length-1) {
			nextIndex = 0;
		} 
		else {
			nextIndex += 1;
		}
		navMesh.SetDestination (patrolRoute [nextIndex]);
		anim.SetFloat ("Speed", 1.0f);
	}

	public void nextAlertPoint() {
		// function to set the guard on alert patrol, at faster walking speed
		navMesh.Resume ();
		// update the target waypoint
		if (alertIndex == alertRoute.Length-1) {
			alertIndex = 0;
		} 
		else {
			alertIndex += 1;
		}
		navMesh.SetDestination (alertRoute [alertIndex]);
		anim.SetFloat ("Speed", 1.5f);
	}

	void Search() {
		// if last position of player is known and player is not in sight, run to last known position
		//print ("Searching...");
		if( Vector3.Distance(transform.position, GetComponent<EnemySight>().lastPlayerSighting) > 1f ){
			navMesh.Resume();
			navMesh.SetDestination(GetComponent<EnemySight>().lastPlayerSighting);
			anim.SetFloat ("Speed", 2.0f);
		}
		else {
			// if at search coordinates and no sight of player, wait before resuming patrol
			navMesh.Stop();
			anim.SetFloat("Speed", 0.0f);
			walking = false;
		}
	}


	void Attack() {
		if(Vector3.Distance(transform.position, player.transform.position) < 4.0f){
			// stop moving and moving animations
			navMesh.Stop();
			anim.SetFloat("Speed", 0.0f);
			walking = false;
			// fire aim weapon and fire
			GetComponent<Shooting>().Shoot();
		}else{
			// run towards the player to get in range
			navMesh.SetDestination(player.transform.position);
			anim.SetFloat("Speed", 2.0f);
		}
	}

	public void Investigate(Vector3 searchPosition){
		// function for when guard is alerted by a noise
		print ("Investigating...");
		//GetComponent<EnemySight> ().globalAlert(searchPosition);
		GetComponent<EnemySight> ().visionCount = 120;
		GetComponent<EnemySight> ().lastPlayerSighting = searchPosition;
		if( Vector3.Distance(transform.position, GetComponent<EnemySight>().lastPlayerSighting) > 1f ){
			navMesh.Resume();
			navMesh.SetDestination(GetComponent<EnemySight>().lastPlayerSighting);
			anim.SetFloat ("Speed", 1.5f);
		}
		else {
			// if at search coordinates and no sight of player, wait before resuming patrol
			navMesh.Stop();
			anim.SetFloat("Speed", 0.0f);
			walking = false;
		}
	}

	void sentryGuard(){
		// if not at sentry point, set destination to it
		if (Vector3.Distance (transform.position, sentryPoint.transform.position) > 0.5f) {
			navMesh.SetDestination (sentryPoint.transform.position);
		} else {
			navMesh.Stop();
		}
	}
	
}
