using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonServerManager : MonoBehaviourPunCallbacks
{
	private const string GameVersion = "1.0.0";
	private const string NickName = "eoheo";
	
	private void Start()
	{
		PhotonNetwork.SendRate = 60;
		PhotonNetwork.SerializationRate = 60;
		
		PhotonNetwork.GameVersion = GameVersion;
		PhotonNetwork.NickName = NickName;
		
		// 방장이 로드 한 씬으로 다른 참여자가똑같이 이동하게끔 동기화 해주는 옵션
		// 방마다 한명의 마스터 클라이언트가 존재
		PhotonNetwork.AutomaticallySyncScene = true;
		
		PhotonNetwork.ConnectUsingSettings();
	}

	public override void OnConnected()
	{
		Debug.Log("Connected");
		Debug.Log(PhotonNetwork.CloudRegion);
	}

	public override void OnConnectedToMaster()
	{
		Debug.Log("Connected to Master");
		Debug.Log(PhotonNetwork.InLobby);
		
		PhotonNetwork.JoinLobby(TypedLobby.Default);
	}

	public override void OnJoinedLobby()
	{
		Debug.Log($"Joined Lobby: {PhotonNetwork.InLobby}");

		PhotonNetwork.JoinRandomRoom();
	}

	public override void OnJoinedRoom()
	{
		Debug.Log($"Joined Room: {PhotonNetwork.CurrentRoom.Name}");
		
	}

	public override void OnJoinRandomFailed(short returnCode, string message)
	{
		Debug.Log($"Join Random Failed: {returnCode} {message}");
		
		RoomOptions roomOptions = new RoomOptions();
		roomOptions.MaxPlayers = 4; // 최대 플레이어 수 설정
		roomOptions.IsOpen = true; // 방을 열어둠
		roomOptions.IsVisible = true; // 방을 공개로 설정

		PhotonNetwork.CreateRoom("testRoom", roomOptions);
	}

	public override void OnCreatedRoom()
	{
		Debug.Log($"Created Room: {PhotonNetwork.CurrentRoom.Name}");
	}

	public override void OnCreateRoomFailed(short returnCode, string message)
	{
		Debug.Log($"OnCreateRoomFailed: {returnCode} {message}");
	}
}
