public class EnemyAttackState : IState<EnemyStateMachine>
{
	public bool IsInterruptable { get; } = false;

	public void Enter(EnemyStateMachine context)
	{
	}

	public void Update(EnemyStateMachine context, float deltaTime)
	{
	}

	public void Exit(EnemyStateMachine context)
	{
	}
}
