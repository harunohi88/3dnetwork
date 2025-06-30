using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerAttack : PlayerAbility
{
	[SerializeField] private Animator _animator;
	[SerializeField] private List<string> _attackAnimationTriggers;
	[SerializeField] private Collider _weaponCollider;

	private float _cooldownTimer;

	private void Start()
	{
		DeactivateWeaponCollider();
	}
	
	private void Update()
	{
		if (!_photonView.IsMine) return;
		
		CooldownReduce();
		
		if (_cooldownTimer <= 0f && Input.GetMouseButton(0))
		{
			Attack();
		}
	}
	
	public void ActivateWeaponCollider()
	{
		_weaponCollider.enabled = true;
	}
	
	public void DeactivateWeaponCollider()
	{
		_weaponCollider.enabled = false;
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
		
		_photonView.RPC(nameof(PlayAttackAnimation), RpcTarget.All, randomIndex);
		_cooldownTimer = 1f / _ownerPlayer.PlayerStat.AttackSpeed;
	}

	[PunRPC]
	private void PlayAttackAnimation(int randomIndex)
	{
		_animator.SetTrigger(_attackAnimationTriggers[randomIndex]);
	}

	public void Hit(Collider other)
	{
		if (!_photonView.IsMine) return;
		
		// damaged.Damaged(_ownerPlayer.PlayerStat.AttackDamage);
		PhotonView otherPhotonView = other.GetComponent<PhotonView>();
		otherPhotonView.RPC(nameof(Player.Damaged), RpcTarget.AllBuffered, _ownerPlayer.PlayerStat.AttackDamage);
	}
}
