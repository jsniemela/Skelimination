using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public GameObject EnemyObject;
	public int CurrentRound { get; private set; }

	public List<PlayerScore> Scores { get; private set; }
	public int EnemyKills { get; private set; }

	private List<SpawnArea> spawnAreas = new List<SpawnArea>();
	private List<Enemy> enemies = new List<Enemy>();
		
	void Start()
	{
		if (FindObjectsOfType<GameManager>().Length > 1)
			Debug.LogError("Multiple GameManager objects present in the scene");

		spawnAreas.AddRange(FindObjectsOfType<SpawnArea>());
		if (spawnAreas.Count == 0)
			Debug.LogError("No spawn areas found in the scene");

		if (EnemyObject == null)
			Debug.LogError("EnemyObject not set");
	}

	void Update()
	{
		if (CurrentRound == 0)
			OnRoundStart();
	}

	public static GameManager GetInstance()
	{
		GameManager instance = FindObjectOfType<GameManager>();
		if (instance == null)
			Debug.LogError("GameManager object not found in the scene");

		return instance;
	}

	private void SpawnPlayer()
	{

		// update PlayerScore.PlayerObject with newly spawned player character
	}

	private void SpawnEnemy()
	{
		GameObject imageTarget = GameObject.Find("ImageTarget");
		if (imageTarget == null)
		{
			Debug.LogError("ImageTarget object not found in the scene");
			return;
		}

		Transform skeletonsGroup = imageTarget.transform.FindChild("Skeletons");
		if (skeletonsGroup == null)
		{
			Debug.LogError("ImageTarget does not have Skeletons child object");
			return;
		}

		// try to get a random spawn position which is not already occupied by other enemies or players

		Vector3 startPosition = Vector3.zero;
		int spawnAttempts = 10;
		float checkRadius = 2.0f;
		for (; spawnAttempts > 0; spawnAttempts--)
		{
			int spawnIndex = UnityEngine.Random.Range(0, spawnAreas.Count);
			startPosition = spawnAreas[spawnIndex].GetRandomPosition();

			bool occupied = false;
			Collider[] colliders = Physics.OverlapSphere(startPosition, checkRadius);
			for (int i = 0; i < colliders.Length; i++)
			{
				Collider col = colliders[i];
				Transform parent = col.transform.parent;
				string tag = (parent != null && parent.gameObject != null) ? parent.gameObject.tag : "";
				if (tag == "Skeleton" || tag == "Player")
				{
					occupied = true;
					break;
				}
			}

			if (!occupied)
				break;
		}
		if (spawnAttempts <= 0)
		{
			Debug.LogWarning("Unable to spawn enemy, all spawn areas are already occupied");
			return;
		}

		GameObject skeleton = Instantiate(EnemyObject, skeletonsGroup);
		skeleton.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
		skeleton.transform.position = startPosition;

		Enemy enemy = skeleton.GetComponent<Enemy>();
		if (enemy != null)
			enemies.Add(enemy);
	}

	public void OnRoundStart()
	{
		CurrentRound++;

		SpawnEnemy();
		SpawnEnemy();
		SpawnEnemy();
	}

	public void OnRoundEnd()
	{

	}
		
	public void OnDeath(Character victim, Character killer)
	{
		Enemy victimEnemy = victim as Enemy;
		if (victimEnemy != null)
		{
			enemies.Remove(victimEnemy);

			// scoring for players
			Player killerPlayer = killer as Player;
			if (killerPlayer != null)
			{
				for (int i=0; i<Scores.Count; i++)
				{
					if (Scores[i].PlayerObject == killerPlayer)
					{
						Scores[i].Score += victimEnemy.Score;
						Scores[i].Kills++;
					}
				}
			}
		}
	}

	public int GetScoreForPlayer(Player player)
	{
		for (int i=0; i<Scores.Count; i++)
		{
			if (Scores[i].PlayerObject == player)
				return Scores[i].Score;
		}

		Debug.LogError("Player does not have a score!");
		return -1;
	}

	public int GetScoreForPlayerIndex(int index)
	{
		// index 0 should be the local player
		if (index >= 0 && index < Scores.Count)
			return Scores[index].Score;

		Debug.LogError("Invalid player index " + index.ToString() + ", Player does not have a score!");
		return -1;
	}
}