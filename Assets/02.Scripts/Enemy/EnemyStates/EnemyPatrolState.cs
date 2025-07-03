using NUnit.Framework;
using UnityEngine;

public class EnemyPatrolState : IState<EnemyStateMachine>
{
	public bool IsInterruptable { get; } = true;

	private Vector3 _targetPosition;
	private float _maximumDistance = 5f;

	public void Enter(EnemyStateMachine context)
	{
		context.Animator.Play("Walk Forward");
		_targetPosition = context.transform.position + Random.insideUnitSphere * _maximumDistance;
		_targetPosition.y = context.transform.position.y;
		context.NavMeshAgent.SetDestination(_targetPosition);
	}

	public void Update(EnemyStateMachine context, float deltaTime)
	{
		_targetPosition.y = context.transform.position.y;
		context.NavMeshAgent.SetDestination(_targetPosition);
		
		if (context.NavMeshAgent.remainingDistance <= context.NavMeshAgent.stoppingDistance)
		{
			_targetPosition = context.transform.position + Random.insideUnitSphere * _maximumDistance;
			_targetPosition.y = context.transform.position.y;
			context.NavMeshAgent.SetDestination(_targetPosition);
		}
	}

	public void Exit(EnemyStateMachine context)
	{
		context.NavMeshAgent.ResetPath();
	}
}
