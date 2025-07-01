using Photon.Pun;
using TMPro;
using UnityEngine;

public class UI_RoomLog : MonoBehaviour
{
	public TextMeshProUGUI LogText;

	private string _logMessage = "방에 입장했습니다.";

	private void Start()
	{
		Refresh();
		RoomManager.Instance.OnPlayerJoined += PlayerEnterLog;
		RoomManager.Instance.OnPlayerLeft += PlayerExitLog;
		RoomManager.Instance.OnDeath += PlayerDeathLog;
	}
	
	private void Refresh()
	{
		LogText.text = _logMessage;
	}

	public void PlayerEnterLog(string playerName)
	{
		_logMessage += $"\n<color=green>{playerName}</color> <color=#00ffffff>님이 방에 입장했습니다.</color>";
		Refresh();
	}
	
	public void PlayerExitLog(string playerName)
	{
		_logMessage += $"\n<color=green>{playerName}</color><color=#00ffffff>님이 방에서 퇴장했습니다.</color>";
		Refresh();
	}
	
	public void PlayerDeathLog(int deathPlayerNumber, int killerPlayerNumber)
	{
		string deathPlayerName = PhotonNetwork.CurrentRoom.Players[deathPlayerNumber].NickName + deathPlayerNumber.ToString();
		string killerPlayerName = PhotonNetwork.CurrentRoom.Players[killerPlayerNumber].NickName + killerPlayerNumber.ToString();
		
		_logMessage += $"\n<color=red>{deathPlayerName}</color> <color=#00ffffff>님이</color> <color=green>{killerPlayerName}</color> <color=#00ffffff>님에게 처치당했습니다.</color>";
		Refresh();
	}
}
