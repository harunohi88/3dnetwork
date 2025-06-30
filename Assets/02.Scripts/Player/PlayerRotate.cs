using Unity.Cinemachine;
using UnityEngine;

public class PlayerRotate : PlayerAbility
{
    [SerializeField] private Transform _cameraRoot;
    [SerializeField] private Transform _minimapCameraRoot;

    private float _cameraPitch = 0f; // 상하 누적 회전값

    protected override void Awake()
    {
        base.Awake();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (_photonView.IsMine)
        {
            CinemachineCamera camera = GameObject.FindGameObjectWithTag("FollowCamera").GetComponent<CinemachineCamera>();
            camera.Follow = _cameraRoot;
            MinimapCameraFollow minimapCamera = GameObject.FindGameObjectWithTag("MinimapCamera").GetComponent<MinimapCameraFollow>();
            minimapCamera.SetTarget(_minimapCameraRoot);
        }
    }

    private void Update()
    {
        if (!_photonView.IsMine) return;
        if (_ownerPlayer.IsDead) return;
        
        Rotate();
    }

    private void Rotate()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // 좌우 회전 (플레이어 본체)
        transform.Rotate(mouseX * _ownerPlayer.PlayerStat.RotationSpeed * Vector3.up);

        // 상하 회전 (카메라 루트)
        _cameraPitch += -mouseY * _ownerPlayer.PlayerStat.RotationSpeed;
        _cameraPitch = Mathf.Clamp(_cameraPitch, 0f, 80f);

        _cameraRoot.localEulerAngles = new Vector3(_cameraPitch, 0f, 0f);
    }
}