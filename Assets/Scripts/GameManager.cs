﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	public List<EnemyWave> enemyWaves = new List<EnemyWave>();
	public GameObject EnemyObject;

	public int CurrentWave { get; private set; }
	public List<PlayerScore> Scores { get; private set; }

	private List<SpawnArea> spawnAreas = new List<SpawnArea>();
	private List<Enemy> enemies = new List<Enemy>();

	private bool sceneLoaded = false;

	void Start()
	{
		CurrentWave = -1;

		if (FindObjectsOfType<GameManager>().Length > 1)
		{
			Debug.LogWarning("Multiple GameManagers present in the scene");
			Destroy(this);
			return;
		}

		SceneManager.sceneLoaded += OnSceneLoad;
		SceneManager.LoadScene("Scenes/GameplayScene", LoadSceneMode.Additive);

		spawnAreas.AddRange(FindObjectsOfType<SpawnArea>());
		if (spawnAreas.Count == 0)
			Debug.LogError("No spawn areas found in the scene");

		if (EnemyObject == null)
			Debug.LogError("EnemyObject not set");
	}

	private void OnSceneLoad(Scene scene, LoadSceneMode mode)
	{
		SceneManager.sceneLoaded -= OnSceneLoad;

		GameObject[] objects = FindObjectsOfType<GameObject>();

		GameObject imageTarget = GameObject.FindGameObjectWithTag("ImageTarget");
		Transform levelTransform = imageTarget.transform.FindChild("Level") ?? imageTarget.transform;

		// parent all level related objects under ImageTarget.Level
		GameObject attached = this.transform.gameObject;
		for (int i = 0; i < objects.Length; i++)
		{
			GameObject obj = objects[i];
			if (obj == attached || obj.scene.name == "GameplayScene")
				continue;

			obj.transform.SetParent(levelTransform, true);
		}

		sceneLoaded = true;
	}

	void Update()
	{
		if (!sceneLoaded)
			return;

		if (CurrentWave == -1)
		{
			// wait for player to get spawned by network manager
			if (GameObject.FindGameObjectWithTag("Player") == null)
				return;

			StartCoroutine(OnWaveStart());
		}
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
		GameObject imageTarget = GameObject.FindWithTag("ImageTarget");
		if (imageTarget == null)
		{
			Debug.LogError("ImageTarget object not found in the scene");
			return;
		}

		Transform skeletonsGroup = imageTarget.transform.FindChild("Skeletons") ?? imageTarget.transform;

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

	protected IEnumerator OnWaveStart()
	{
		CurrentWave++;

		Debug.Log("Current wave: " + CurrentWave + "/" + enemyWaves.Count);

		if (CurrentWave >= enemyWaves.Count)
		{
			// TODO: no more waves left in this level, go to next level?
			Debug.Log("No more waves left");
			yield return null;
		}

		yield return new WaitForSeconds(1.0f);

		// spaw enemies
		for (int i = 0; i < enemyWaves[CurrentWave].numEnemies; i++)
			SpawnEnemy();

		if (enemies.Count == 0)
		{
			Debug.LogWarning("No enemies were spawned in this wave");
			StartCoroutine(OnWaveEnd());
		}
	}

	protected IEnumerator OnWaveEnd()
	{
		Debug.Log("All enemies are dead, wave ended");

		yield return new WaitForSeconds(1.0f);

		StartCoroutine(OnWaveStart());
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
				for (int i = 0; i < Scores.Count; i++)
				{
					if (Scores[i].PlayerObject == killerPlayer)
					{
						Scores[i].Score += victimEnemy.Score;
						Scores[i].Kills++;
					}
				}
			}
		}

		if (enemies.Count == 0)
			StartCoroutine(OnWaveEnd());
	}

	public int GetScoreForPlayer(Player player)
	{
		for (int i = 0; i < Scores.Count; i++)
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