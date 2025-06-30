using System;
using UnityEngine;
using Microlight.MicroBar;

public class HUD : BehaviourSingleton<HUD>
{
	[SerializeField] private MicroBar _healthBar;
	[SerializeField] private MicroBar _staminaBar;

	private void Awake()
	{
		_healthBar.Initialize(0);
		_staminaBar.Initialize(0);
	}

	private void Start()
	{
		EventManager.Instance.OnPlayerHealthChanged += UpdateHealthBar;
		EventManager.Instance.OnPlayerStaminaChanged += UpdateStaminaBar;
	}
	
	public void SetMaxHealth(float maxHealth)
	{
		_healthBar.SetNewMaxHP(maxHealth);
	}
	
	public void SetMaxStamina(float maxStamina)
	{
		_staminaBar.SetNewMaxHP(maxStamina);
	}

	public void UpdateHealthBar(float currentHealth)
	{
		_healthBar.UpdateBar(currentHealth);
	}
	
	public void UpdateStaminaBar(float currentStamina)
	{
		_staminaBar.UpdateBar(currentStamina);
	}
}
