using System;
using UnityEngine;

public class PlayerNicknameCanvas : MonoBehaviour
{
	private void Update()
	{
		transform.forward = Camera.main.transform.forward;
	}
}
