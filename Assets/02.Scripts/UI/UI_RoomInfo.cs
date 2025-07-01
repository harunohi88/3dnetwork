using UnityEngine;
using TMPro;

public class UI_RoomInfo : MonoBehaviour
{
	public TextMeshProUGUI RoomNameText;
	public TextMeshProUGUI RoomCountText;

	public void OnClickExitButton()
	{
		RoomManager.Instance.Exit();
	}
}
