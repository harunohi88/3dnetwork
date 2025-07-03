public class EnemyTraceState : IState<EnemyStateMachine>
{
	public bool IsInterruptable { get; } = true;
	
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
