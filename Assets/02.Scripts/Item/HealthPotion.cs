using Photon.Pun;
using UnityEngine;

public class HealthPotion : MonoBehaviour
{
	private PhotonView _photonView;

	private void Awake()
	{
		_photonView = GetComponent<PhotonView>();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			PhotonView photonView = other.GetComponent<PhotonView>();
			if (photonView.IsMine)
			{
				Debug.Log("PlayerPhotonView: " + photonView.ViewID);
				photonView.RPC("Heal", RpcTarget.AllBuffered, 30f);
			}
			ItemObjectFactory.Instance.RequestDelete(_photonView.ViewID);
		}
	}
}
