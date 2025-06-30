using UnityEngine;

public class PlayerAnimationRelay : MonoBehaviour
{
	private PlayerAttack _playerAttack;

	private void Start()
	{
		_playerAttack = GetComponentInParent<PlayerAttack>();
	}

	private void ActivateWeaponCollider()
	{
		_playerAttack.ActivateWeaponCollider();
	}
	
	private void DeactivateWeaponCollider()
	{
		_playerAttack.DeactivateWeaponCollider();
	}
}
