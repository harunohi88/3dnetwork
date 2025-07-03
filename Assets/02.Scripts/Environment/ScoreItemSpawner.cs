using Photon.Pun;
using UnityEngine;

public class ScoreItemSpawner : MonoBehaviour
{
	public float Interval;
	public float Range;
	public float SpawnHeight;

	private float _timer;

	private void Awake()
	{
		Interval = Random.Range(2f, 5f);
		Range = Random.Range(4f, 10f);
		SpawnHeight = 5f;
	}

	private void Update()
	{
		if (!PhotonNetwork.IsMasterClient) return;
		
		_timer += Time.deltaTime;
		
		if (_timer >= Interval)
		{
			_timer = 0f;
			Vector3 randomPosition = transform.position + Random.insideUnitSphere * Range;
			randomPosition.y = SpawnHeight;
			ItemObjectFactory.Instance.RequestCreate(EItemType.Score, randomPosition);
		}
	}
}
