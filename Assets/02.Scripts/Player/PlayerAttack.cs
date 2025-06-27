using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : PlayerAbility
{
	[SerializeField] private Animator _animator;
	[SerializeField] private List<string> _attackAnimationTriggers;

	private float _cooldownTimer;

	private void Update()
	{
		if (!_photonView.IsMine) return;
		
		CooldownReduce();
		
		if (_cooldownTimer <= 0f && Input.GetMouseButton(0))
		{
			Attack();
		}
	}

	private void CooldownReduce()
	{
		if (_cooldownTimer > 0)
		{
			_cooldownTimer -= Time.deltaTime;
		}
	}

	private void Attack()
	{
		if (!_ownerPlayer.TryUseStamina(_ownerPlayer.PlayerStat.AttackStaminaCost)) return;
		
		int randomIndex = Random.Range(0, _attackAnimationTriggers.Count);
		
		_animator.SetTrigger(_attackAnimationTriggers[randomIndex]);
		_cooldownTimer = 1f / _ownerPlayer.PlayerStat.AttackSpeed;
	}
}
