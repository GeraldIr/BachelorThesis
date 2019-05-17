using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleBehaviour : MonoBehaviour {

	public Gamemanager gamemanager;

    private void Start()
    {
        gamemanager = GameObject.Find("Gamemanager").GetComponent<Gamemanager>();
    }
    void OnTriggerEnter2D(Collider2D collider)
	{

		if (collider.name.Contains("Projectile"))
		{
			gamemanager.LoseGame();
		}
	}
}
