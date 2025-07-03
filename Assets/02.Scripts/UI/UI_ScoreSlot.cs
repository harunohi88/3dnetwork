using TMPro;
using UnityEngine;

public class UI_ScoreSlot : MonoBehaviour
{
	public TextMeshProUGUI RankTextUI;
	public TextMeshProUGUI NickNameTextUI;
	public TextMeshProUGUI ScoreTextUI;
	
	public void Set(string rank, string nickName, string score)
	{
		RankTextUI.text = rank;
		NickNameTextUI.text = nickName;
		ScoreTextUI.text = score;
	}
}
