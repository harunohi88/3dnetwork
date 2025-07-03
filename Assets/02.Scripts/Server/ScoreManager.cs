using System;
using System.Collections.Generic;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Pun;
using UnityEngine;

public class ScoreManager : MonoBehaviourPunCallbacks
{
	private static ScoreManager _instance;
	public static ScoreManager Instance => _instance;

	public int ScorePerKill = 5000;
	
	private int _score = 0;
	public int Score => _score;
	
	private int _killCount = 0;
	public int KillCount => _killCount;
	
	private Dictionary<string, int> _scores = new Dictionary<string, int>();
	public Dictionary<string, int> Scores => _scores;
	
	public event Action OnDataChanged;

	public void Awake()
	{
		if (_instance == null)
		{
			_instance = this;
		}
		else
		{
			Destroy(gameObject);
		}
	}
	
	public override void OnJoinedRoom()
	{
		UpdateScore();
	}

	public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps)
	{
		Debug.Log($"{targetPlayer.NickName}_{targetPlayer.ActorNumber}: {changedProps["Score"]}");
		
		Photon.Realtime.Player[] players = PhotonNetwork.PlayerList;
		foreach (Photon.Realtime.Player player in players)
		{
			if (player.CustomProperties.ContainsKey("Score"))
			{
				_scores[$"{player.NickName}_{player.ActorNumber}"] = (int)player.CustomProperties["Score"];
			}
		}
		
		OnDataChanged?.Invoke();
	}


	private void UpdateScore()
	{
		Hashtable hashTable = new Hashtable();
		hashTable.Add("Score", _killCount * ScorePerKill + _score);
		
		PhotonNetwork.LocalPlayer.SetCustomProperties(hashTable);
	}
	
	public void AddScore(int addedScore)
	{
		_score += addedScore;
		UpdateScore();
	}

	public void AddKillCount()
	{
		++_killCount;
		UpdateScore();
	}
}
