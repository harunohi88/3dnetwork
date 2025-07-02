public interface IState<T> where T : class
{
	public bool IsInterruptable { get; }
	
	public void Enter(T context);
	public void Update(T context, float deltaTime);
	public void Exit(T context);
}
