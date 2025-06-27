using UnityEngine;

public class MinimapCameraFollow : MonoBehaviour
{
	[SerializeField] private Transform _target;

	private void Update()
	{
		if (_target == null)
		{
			return;
		}
		
		transform.position = _target.position;
		transform.rotation = Quaternion.Euler(90f, _target.eulerAngles.y, 0f);
	}

	public void SetTarget(Transform target)
	{
		_target = target;
	}
}
