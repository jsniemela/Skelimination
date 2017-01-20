using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public GameObject skeletonObject;
	public int CurrentRound { get; private set; }
	public int EnemyKills { get; private set; }

	private List<SpawnArea> spawnAreas = new List<SpawnArea>();
	private List<GameObject> enemies = new List<GameObject>();
		
	void Start()
	{
		if (FindObjectsOfType<GameManager>().Length > 1)
			Debug.LogError("Multiple GameManager objects present in the scene");

		spawnAreas.AddRange(FindObjectsOfType<SpawnArea>());
		if (spawnAreas.Count == 0)
			Debug.LogError("No spawn areas found in the scene");
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

		int spawnIndex = UnityEngine.Random.Range(0, spawnAreas.Count);
		Vector3 startPosition = spawnAreas[spawnIndex].GetRandomPosition();

		GameObject skeleton = (GameObject)Instantiate(skeletonObject, skeletonsGroup);
		skeleton.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
		skeleton.transform.position = startPosition;

		enemies.Add(skeleton);
	}

	public void OnRoundStart()
	{
		CurrentRound++;

		SpawnEnemy();
	}

	public void OnRoundEnd()
	{

	}
		
	public void OnDeath(GameObject victim, GameObject killer)
	{
		// if victim is a skeleton
		enemies.Remove(victim);
	}
}