using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class EnemyStateMachine : MonoBehaviourPun
{
	public EnemyStat Stat;
	
	private Animator _animator;
	public Animator Animator => _animator;
	
	private CharacterController _characterController;
	public CharacterController CharacterController => _characterController;
	
	private NavMeshAgent _navMeshAgent;
	public NavMeshAgent NavMeshAgent => _navMeshAgent;
	
	private IState<EnemyStateMachine> _currentState;
	private Dictionary<EEnemyState, IState<EnemyStateMachine>> _stateDictionary;

	private void Awake()
	{
		_animator = GetComponent<Animator>();
		_characterController = GetComponent<CharacterController>();
		_navMeshAgent = GetComponent<NavMeshAgent>();
		Stat = GetComponent<EnemyStat>();
		
		_stateDictionary = new Dictionary<EEnemyState, IState<EnemyStateMachine>>
		{
			{ EEnemyState.Idle, new EnemyIdleState() },
			{ EEnemyState.Patrol, new EnemyPatrolState() },
			{ EEnemyState.Trace, new EnemyTraceState() },
			{ EEnemyState.Attack, new EnemyAttackState() },
			{ EEnemyState.Die, new EnemyDieState() }
		};
		
		_currentState = _stateDictionary[EEnemyState.Idle];
	}

	private void Update()
	{
		_currentState?.Update(this, Time.deltaTime);
	}
	
	public void RequestStateChange(EEnemyState newState)
	{
		if (!PhotonNetwork.IsMasterClient) return;
		
		photonView.RPC(nameof(RPCChangeState), RpcTarget.All, newState);
	}

	[PunRPC]
	public void EventStateChange(EEnemyState newState)
	{
		if (!PhotonNetwork.IsMasterClient) return;

		if (_currentState.IsInterruptable)
		{
			photonView.RPC(nameof(RPCChangeState), RpcTarget.All, newState);
		}
	}
	
	[PunRPC]
	public void RPCChangeState(EEnemyState newState)
	{
		_currentState?.Exit(this);

		if (_stateDictionary.TryGetValue(newState, out IState<EnemyStateMachine> state))
		{
			_currentState = state;
			_currentState.Enter(this);
		}
	}
}
