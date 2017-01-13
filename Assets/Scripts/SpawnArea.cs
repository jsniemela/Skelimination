using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnArea : MonoBehaviour
{
	List<Collider> colliders = new List<Collider>();

	private void Start()
	{
		Debug.Log("start");
		colliders.AddRange(GetComponentsInChildren<Collider>(true));
		if (colliders.Count == 0)
			Debug.LogError("SpawnArea must have active Colliders");
	}

	public Vector3 GetRandomPosition()
	{
		Vector3 position = Vector3.zero;
		int randomCollider = Random.Range(0, colliders.Count);
		Collider collider = colliders[randomCollider];
		
		if (collider is BoxCollider)
		{
			BoxCollider box = collider as BoxCollider;
			Vector3 size = box.bounds.size;

			position.x = Random.Range(0, size.x/2);
			position.y = -size.y/2;
			position.z = Random.Range(0, size.z/2);

			position += collider.transform.position;
		}
		else
			Debug.LogError("Unsupported Collider of type " + collider.GetType().ToString());

		return position;
	}
}
