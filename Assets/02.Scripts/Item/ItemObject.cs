using Photon.Pun;
using UnityEngine;

public enum EItemType
{
	Score,
	Health,
	Stamina,
}

public class ItemObject : MonoBehaviour
{
	private PhotonView _photonView;

	private void Awake()
	{
		_photonView = gameObject.GetComponent<PhotonView>();
	}
	
	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			ScoreManager.Instance.AddScore(100);
			ItemObjectFactory.Instance.RequestDelete(_photonView.ViewID);
		}
	}
}