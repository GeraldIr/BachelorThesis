using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EnvironmentInteraction : MonoBehaviour {

	public Gamemanager manager;

	private GameObject goal;

	public GameObject[] avoid;

	// Use this for initialization
	void Start () {

		goal = manager.goal;
		avoid = manager.obstacles;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (goal == other.gameObject)
		{
			manager.WinGame();
		}
		else if(avoid.Contains(other.gameObject))
		{
			manager.LoseGame();
		}
	}
}
