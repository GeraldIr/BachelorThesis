using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using UnityEngine.SceneManagement;

public class Gamemanager : MonoBehaviour {



	public bool saveRunAsFile = false;
	public string runName;	
	private List<bool> winLossList = new List<bool>();
	private string currentFilePath;

	public float projectileSpawnX;
	public float projectileSpawnY;

	public float projectileSpawnXTest;
	public float projectileSpawnYTest;

	public float[,] qTable;

	//Parity State
	public int maxActionsPerEpisode = 500;

	//Curiosity
	public float curiosity = 1;
	public float curiosityDecrease = 0.001f;

	//Reward Punishment
	public float reward = 1000;
	public float punishment = 1;

	//Variable Reward / Punishment Ratio
	public bool variableRP;
	public float c; //constant that the ratio is less than n-1
	public int a; //avg over a games
	public int x; //check all x games
	public float prRatio;


	//Reward Splitting
	public bool rewardSplitting = false;
	


	//INPUT
	private int stateAmount = 1;
	public int[] value;
	public int[] quantizationMatrix;
	public int nrOfRaycasts;


	//Calculated
	public int inputSize;


	//Simulation Parameters
	public float timeFactor = 100;
	public int episodes = 1000;
	public int testEpisodes = 200;
	private bool done = false;
	private bool testDone = false;
	public int currentEpisode = 0;

	//Statistics
	private int wins = 0;
	private int losses = 0;
	private Queue<bool> wlq; //queue of all the wins and losses



	public List<StateAction> stateActionsCurrentEpisode;


	//map setup
	public GameObject projectilePrefab;
	public GameObject[] obstacles;
	public GameObject goal;

	public GameObject projectile;

	PrepareInputs pInputs;
	

	// Use this for initialization
	void Start () {
        goal = GameObject.Find("Target");

        if (done)
        {
            Debug.Log("hello new scene");
            done = false;
        }
        else
        {
            if (saveRunAsFile)
            {
                currentFilePath = Path.Combine("./Runs/", "Run_" + runName + ".txt");
                File.Create(currentFilePath).Close();
            }


            Time.timeScale = timeFactor;

            inputSize = nrOfRaycasts + 1;

            wlq = new Queue<bool>();
            value = new int[inputSize];


            value[0] = 1;
            for (int i = 1; i < inputSize; i++)
            {
                value[i] = value[i - 1] * quantizationMatrix[i - 1];
            }

            stateActionsCurrentEpisode = new List<StateAction>();
            pInputs = projectile.GetComponent<PrepareInputs>();



            int[] stateAmountInput = quantizationMatrix;
            stateAmountInput[0] -= 1;

            stateAmount = pInputs.InputToStateIndex(stateAmountInput);

            qTable = new float[stateAmount, quantizationMatrix[0]];

            for (int s = 0; s < qTable.GetLength(0); s++)
            {
                for (int a = 0; a < qTable.GetLength(1); a++)
                {
                    qTable[s, a] = 0;
                }
            }

            Debug.Log("Length:" + qTable.Length);
            Debug.Log("Stateamount:" + stateAmount);
        }


	}
	
	public float GetAverageWLQ()
	{
		return (float)wlq.Where(x => x).Count() / (float)wlq.Count;
	}

	public void ResetGame()
	{
		goal = GameObject.Find("Target");
		if (currentEpisode % x == 0 && variableRP)
		{
			float wr = GetAverageWLQ();
			if(wr != 0)
			{
				prRatio = (Mathf.Pow(wr, -1) - 1) * c;
			}
			else
			{
				prRatio = 100;
			}
			

			punishment = 1;
			reward = 1 * prRatio;

			Debug.Log("Winrate: " + wr + " Ratio: " + prRatio);
		} 

		currentEpisode++;

		

		float winPercentage = (float)wins / (float)(losses + wins);

		Debug.Log("CurrentEpisode: " + currentEpisode + " Curiosity: " +  curiosity + " Win-Percentage: " + winPercentage);
		stateActionsCurrentEpisode = new List<StateAction>();

		if (projectile != null)
			Destroy(projectile);

		projectile = Instantiate(projectilePrefab, new Vector2(projectileSpawnX, projectileSpawnY), Quaternion.identity);
		pInputs = projectile.GetComponent<PrepareInputs>();
		pInputs.goal = this.goal;
		pInputs.gamemanager = this;
		projectile.GetComponent<Movement>().gamemanager = this;


		if (curiosity > 0.001)
			curiosity *= (1 - curiosityDecrease);
		else
			curiosity = 0;

		if (currentEpisode > episodes)
		{
			if(!done || (currentEpisode > (episodes + testEpisodes) && !testDone))
			{
				if (saveRunAsFile)
				{
					Debug.Log("Writing File!");
					using (StreamWriter sw = new StreamWriter(File.OpenWrite(currentFilePath)))
					{
						foreach(bool wl in winLossList)
						{
							if (wl)
								sw.Write("w");
							else
								sw.Write("l");
						}
						
					}

					if (currentEpisode > (episodes + testEpisodes))
						testDone = true;
				}

				projectileSpawnX = projectileSpawnXTest;
				projectileSpawnY = projectileSpawnYTest;

                SceneManager.LoadScene(1);
            }
			

			done = true;

            
        }
	}

	public void WinGame()
	{
		//rewarding
		//No rewardSplit
		if(rewardSplitting)
		{
			foreach (StateAction sa in stateActionsCurrentEpisode)
			{
				qTable[sa.stateIndex, sa.action] += (reward / stateActionsCurrentEpisode.Count);
			}
		} else
		{
			foreach (StateAction sa in stateActionsCurrentEpisode)
			{
				qTable[sa.stateIndex, sa.action] += reward;
			}
		}

		wins++;
		AddToWLQueue(true);
		ResetGame();
	}

	public void LoseGame()
	{
		if (rewardSplitting)
		{
			foreach (StateAction sa in stateActionsCurrentEpisode)
			{
				qTable[sa.stateIndex, sa.action] -= (punishment / stateActionsCurrentEpisode.Count);
			}
		}
		else
		{
			foreach (StateAction sa in stateActionsCurrentEpisode)
			{
				qTable[sa.stateIndex, sa.action] -= punishment;
			}
		}

		//Statistics
		losses++;
		AddToWLQueue(false);

		ResetGame();
	}

	private void AddToWLQueue(bool wl)
	{
		winLossList.Add(wl);
		if (wlq.Count >= this.a)
		{
			wlq.Dequeue();
			wlq.Enqueue(wl);
		} else
		{
			wlq.Enqueue(wl);
		}
	}

	void OnTriggerExit2D(Collider2D collider)
	{
		
		
		if (collider.name.Contains("Projectile"))
		{
			LoseGame();
		}
			
	}


}

public class Environment
{
    public Transform goalTransform;
    public Transform projectileTransform;
    public List<Transform> obstaclesTransforms;
}

public struct StateAction
{
	public StateAction(int stateIndex, int action)
	{
		this.stateIndex = stateIndex;
		this.action = action;
	}

	public int stateIndex;
	public int action;
}
