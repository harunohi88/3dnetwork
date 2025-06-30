using System;
using UnityEngine;

public class EventManager : BehaviourSingleton<EventManager>
{
	public Action<float> OnPlayerHealthChanged;
	public Action<float> OnPlayerStaminaChanged;
}
