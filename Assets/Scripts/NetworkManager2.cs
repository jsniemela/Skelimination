using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkManager2 : NetworkManager
{

	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
	{
		base.OnServerAddPlayer(conn, playerControllerId);

		Player player = null;
		for (int i=0; i<conn.playerControllers.Count; i++)
		{
			player = conn.playerControllers[i].gameObject.GetComponent<Player>();
			if (player != null)
				break;
		}
		
		if (player == null)
			return;

		GameManager gameManager = GameManager.GetInstance();
		gameManager.RegisterPlayer(player);
	}

	public override void OnStopServer()
	{
		base.OnStopServer();
		GameManager gameManager = GameManager.GetInstance();
		gameManager.Reset();
	}

	public override void OnStopClient()
	{
		base.OnStopClient();
		GameManager gameManager = GameManager.GetInstance();
		gameManager.Reset();
	}

	public override void OnStopHost()
	{
		base.OnStopHost();
		GameManager gameManager = GameManager.GetInstance();
		gameManager.Reset();
	}

	public override void OnClientDisconnect(NetworkConnection conn)
	{
		base.OnClientDisconnect(conn);
		GameManager gameManager = GameManager.GetInstance();
		gameManager.Reset();
	}
}
