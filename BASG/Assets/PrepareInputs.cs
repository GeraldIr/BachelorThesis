using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrepareInputs : MonoBehaviour {






	

	public float ld = 10;


	public GameObject goal;

	Movement move;

	public Gamemanager gamemanager;



	public float actionCooldown = 0.2f;


	private float timeSinceLastAction = 0;


	void Start()
	{
        gamemanager = GameObject.Find("Gamemanager").GetComponent<Gamemanager>();
        
        Physics2D.queriesHitTriggers = true;
		move = GetComponent<Movement>();
	}

	void FixedUpdate()
	{
		timeSinceLastAction += Time.deltaTime;

		if (timeSinceLastAction >= actionCooldown)
		{
			int stateIndex = GetCurrentStateIndex();
			int action = ChooseAction();

			
			move.SetInput(action);
			gamemanager.stateActionsCurrentEpisode.Add(new StateAction(stateIndex, action));
			if (gamemanager.stateActionsCurrentEpisode.Count > gamemanager.maxActionsPerEpisode)
			{
				Debug.Log("Episode too long!");
				gamemanager.LoseGame();
			}

			timeSinceLastAction = 0;
		}

	}


	public int ChooseAction()
	{
		if(gamemanager == null)
			gamemanager = GameObject.Find("Gamemanager").GetComponent<Gamemanager>();

		if ((float)Random.Range(0, 10000) / 10000 < gamemanager.curiosity)
		{
			return Random.Range(0, gamemanager.quantizationMatrix[0]);
		} else
		{
			return GetBestAction(GetCurrentStateIndex());
			
		}
	}

	int GetCurrentStateIndex()
	{

		int[] temp = QuanitizeAndEncodeInputs(GetCurrentInputs());

		return InputToStateIndex(temp);
	}

	int GetBestAction(int stateIndex)
	{

		int bestAction = 0;
		for (int a = 0; a < gamemanager.quantizationMatrix[0]; a++)
		{
			if(gamemanager.qTable[stateIndex, a] > gamemanager.qTable[stateIndex, bestAction])
			{
				bestAction = a;
			}
		}

		return bestAction;
	}


	public int InputToStateIndex(int[] q)
	{


		int stateIndex = 0;

		for (int i = 0; i < gamemanager.inputSize; i++)
		{
			stateIndex += q[i] * gamemanager.value[i];
		}


		return stateIndex;
	}


	public float[] GetCurrentInputs()
	{
		
		float[] inputs = new float[gamemanager.inputSize];


		inputs[0] = Vector2.SignedAngle(Vector2.up, (goal.transform.position - this.transform.position).normalized) + 180;

		



		if(gamemanager.inputSize > 1)
		{
			float stepsize = 2 * Mathf.PI / gamemanager.nrOfRaycasts;

			for (int i = 0; i < gamemanager.nrOfRaycasts; i++)
			{
				RaycastHit2D hit = Physics2D.Raycast(transform.position, new Vector2(Mathf.Cos(i * stepsize), Mathf.Sin(i * stepsize)).normalized, ld);
				if (hit.collider != null)
				{
					inputs[i+1] = hit.distance;
				}
				else
                {
                    inputs[i + 1] = 0;
                }
					
			}
		}



        return inputs;
	}

	private int[] QuanitizeAndEncodeInputs(float[] inputs)
	{


		int[] quantizedInput = new int[gamemanager.inputSize];

		if(gamemanager.inputSize > 1)
		{
			for (int i = 1; i < gamemanager.inputSize; i++)
			{
				if(inputs[i] == 0)
				{
					quantizedInput[i] = gamemanager.quantizationMatrix[i];
				} else
				{
					quantizedInput[i] = Mathf.FloorToInt((inputs[i] / ld) * gamemanager.quantizationMatrix[i]);
				}
				
			}
		}





        int index = Mathf.RoundToInt((inputs[0] / 360.0f) * gamemanager.quantizationMatrix[0]);

		if (index == gamemanager.quantizationMatrix[0])
			quantizedInput[0] = 0;
		else
			quantizedInput[0] = index;


        return quantizedInput;
	}
}
