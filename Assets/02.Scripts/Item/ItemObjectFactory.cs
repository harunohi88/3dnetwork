using System;
using Photon.Pun;
using UnityEngine;

public class ItemObjectFactory : MonoBehaviourPun
{
	public static ItemObjectFactory Instance;
	private PhotonView _photonView;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Destroy(gameObject);
		}
		_photonView = GetComponent<PhotonView>();
	}

	public void RequestCreate(EItemType type, Vector3 position)
	{
		if (PhotonNetwork.IsMasterClient)
		{
			Create(type, position);
		}
		else
		{
			_photonView.RPC(nameof(Create), RpcTarget.MasterClient, type, position);
		}
	}
	
	[PunRPC]
	private void Create(EItemType type, Vector3 position)
	{
		switch (type)
		{
			case EItemType.Score:
				CreateScoreItem(position);
				break;
			case EItemType.Health:
				CreateHealthItem(position);
				break;
			default:
				Debug.LogError("Unknown item type: " + type);
				break;
		}
	}

	public void RequestDelete(int viewId)
	{
		if (PhotonNetwork.IsMasterClient)
		{
			Delete(viewId);
		}
		else
		{
			_photonView.RPC(nameof(Delete), RpcTarget.MasterClient, viewId);
		}
	}
	
	[PunRPC]
	private void Delete(int viewId)
	{
		GameObject objectToDelete = PhotonView.Find(viewId)?.gameObject;
		
		Debug.Log(objectToDelete);
		if (objectToDelete != null)
		{
			PhotonNetwork.Destroy(objectToDelete);
		}
		else
		{
			Debug.LogError("Object with view ID " + viewId + " not found.");
		}
	}
	
	private void CreateScoreItem(Vector3 position)
	{
		PhotonNetwork.InstantiateRoomObject("ScoreItem", position, Quaternion.identity);
	}
	
	private void CreateHealthItem(Vector3 position)
	{
		PhotonNetwork.InstantiateRoomObject("HealthPotion", position, Quaternion.identity);
	}
}
