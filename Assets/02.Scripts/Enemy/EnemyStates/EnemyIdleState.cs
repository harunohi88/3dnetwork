public class EnemyIdleState : IState<EnemyStateMachine>
{
	public bool IsInterruptable { get; } = true;

	private float _maximumIdleTime = 3f;
	private float _elapsedTime;

	public void Enter(EnemyStateMachine context)
	{
		context.Animator.Play("Combat Idle");
		_elapsedTime = 0f;
	}

	public void Update(EnemyStateMachine context, float deltaTime)
	{
		_elapsedTime += deltaTime;
		
		if (_elapsedTime >= _maximumIdleTime)
		{
			context.RequestStateChange(EEnemyState.Patrol);
		}
	}

	public void Exit(EnemyStateMachine context)
	{
		// 애니메이션 정지
		_elapsedTime = 0f;
	}
}
