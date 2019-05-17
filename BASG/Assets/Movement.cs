using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour {

	Rigidbody2D rb;

	int directionalQuanitzation;

	public Gamemanager gamemanager;

	float timeSince = 0;
	bool isCd = false;
	public float force = 1;


	public bool isAgent;

	float inputDirection;

	// Use this for initialization
	void Start () {
        gamemanager = GameObject.Find("Gamemanager").GetComponent<Gamemanager>();
		rb = GetComponent<Rigidbody2D>();
		directionalQuanitzation = gamemanager.quantizationMatrix[0];
		SetInput(GetComponent<PrepareInputs>().ChooseAction());
	}
	
	void FixedUpdate () {

		rb.AddForce(new Vector2(Mathf.Sin(inputDirection), Mathf.Cos(inputDirection)) * force);	
	}

	public void SetInput(int direction)
	{
		inputDirection = direction * 2 * Mathf.PI / directionalQuanitzation;
	}


}
