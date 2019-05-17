using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalBehaviour : MonoBehaviour {

	public Gamemanager gamemanager;

	// Use this for initialization
	void Start () {
        gamemanager = GameObject.Find("Gamemanager").GetComponent<Gamemanager>();
    }

	public Vector2[] checkpoints;
	private int currentCheckpoint = 0;


	public float speed;

	// Update is called once per frame
	void FixedUpdate () {
        if(checkpoints.Length > 0)
        {
            if ((Vector2)transform.position == checkpoints[currentCheckpoint])
                NextCheckpoint();

            transform.position = Vector2.MoveTowards((Vector2)transform.position, checkpoints[currentCheckpoint], speed * Time.deltaTime);
        }
	}

	private void NextCheckpoint()
	{
			if (currentCheckpoint < checkpoints.Length-1)
				currentCheckpoint += 1;
			else
				currentCheckpoint = 0;
	}



	void OnTriggerEnter2D(Collider2D collider)
	{

		if(collider.name.Contains("Projectile"))
		{
			gamemanager.WinGame();
		}
	}
}
