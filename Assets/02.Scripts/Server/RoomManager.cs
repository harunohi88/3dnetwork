using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class RoomManager : MonoBehaviourPunCallbacks
{
	public static RoomManager Instance { get; private set; }
	
	private Room _room;
	public Room Room => _room;

	public Action OnRoomDataChanged;
	public Action<string> OnPlayerJoined;
	public Action<string> OnPlayerLeft;
	public Action<int, int> OnDeath;

	private void Awake()
	{
		if (Instance != null)
		{
			Destroy(gameObject);
		}
		else
		{
			Instance = this;
			// DontDestroyOnLoad(gameObject);
		}
	}
	
	public override void OnJoinedRoom()
	{
		_room = PhotonNetwork.CurrentRoom;
		
		GeneratePlayer();
	}

	public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
	{
		OnPlayerJoined?.Invoke(newPlayer.NickName);
	}
	
	public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
	{
		OnPlayerLeft?.Invoke(otherPlayer.NickName);
	}

	private void GeneratePlayer()
	{
		PhotonNetwork.Instantiate("Player", RandomSpawn.Instance.GetRandomSpawnPoint(), Quaternion.identity);
	}

	public void Exit()
	{
		// 방을 나가는 메서드
	}

	public void OnPlayerDeath(int actorNumber, int otherActorNumber)
	{
		OnDeath(actorNumber, otherActorNumber);
	}
}
