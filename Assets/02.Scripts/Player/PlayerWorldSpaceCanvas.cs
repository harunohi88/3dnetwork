using System;
using UnityEngine;
using Microlight.MicroBar;

public class PlayerWorldSpaceCanvas : MonoBehaviour
{
	public MicroBar HealthBar;
	
	public void Awake()
	{
		HealthBar.Initialize(0);
	}
	
	private void Update()
	{
		transform.forward = Camera.main.transform.forward;
	}
	
	public void SetMaxHealth(float maxHealth)
	{
		HealthBar.SetNewMaxHP(maxHealth);
	}
	
	public void UpdateHealthBar(float currentHealth)
	{
		HealthBar.UpdateBar(currentHealth);
	}
}
