using System.Collections.Generic;
using UnityEngine;

public class RandomSpawn : BehaviourSingleton<RandomSpawn>
{
	[SerializeField] private List<Transform> _spawnPoints;
	
	public Vector3 GetRandomSpawnPoint()
	{
		int randomIndex = Random.Range(0, _spawnPoints.Count);
		return _spawnPoints[randomIndex].position;
	}
}
