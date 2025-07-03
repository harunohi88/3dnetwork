using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class UI_Score : MonoBehaviour
{
	public List<UI_ScoreSlot> ScoreSlots;
	public UI_ScoreSlot MyScoreSlot;

	private void Start()
	{
		ScoreManager.Instance.OnDataChanged += Refresh;
	}

	private void Refresh()
	{
		Dictionary<string, int> scores = ScoreManager.Instance.Scores;
		List<KeyValuePair<string, int>> sortedScores = scores.ToList().OrderByDescending(kvp => kvp.Value).ToList();

		for (int i = 0; i < sortedScores.Count; i++)
		{
			ScoreSlots[i].Set(
				(i + 1).ToString(),
				sortedScores[i].Key, 
				sortedScores[i].Value.ToString()
			);

			if (i == 3) break;
		}
		
	}
}
