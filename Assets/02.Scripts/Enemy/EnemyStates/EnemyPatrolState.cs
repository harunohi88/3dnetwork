using UnityEngine;

public class EnemyPatrolState : IState<EnemyStateMachine>
{
	public bool IsInterruptable { get; }

	private Vector3 _startPosition;
	private Vector3 _targetPosition;
	private float _minimumDistance = 0.1f;
	
	public void Enter(EnemyStateMachine context)
	{
		_startPosition = context.transform.position;
		// 목표 위치 지정
	}

	public void Update(EnemyStateMachine context, float deltaTime)
	{
		// context.CharacterController 이용해서 목표 위치로 이동

		if (Vector3.Distance(context.transform.position, _startPosition) < _minimumDistance)
		{
			// 새로운 목표 위치 설정
		}
	}

	public void Exit(EnemyStateMachine context)
	{
	}
}
