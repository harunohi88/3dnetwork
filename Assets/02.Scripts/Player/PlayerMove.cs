using Photon.Pun;
using UnityEngine;

public class PlayerMove : PlayerAbility, IPunObservable
{
    [SerializeField] private Transform _cameraPosition;
    [SerializeField] private Animator _animator;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _gravity = -9.81f;
    [SerializeField] private float _groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private Vector3 _velocity;

    private CharacterController _characterController;
    
    private Vector3 _receivedPosition = Vector3.zero;
    private Quaternion _receivedRotation = Quaternion.identity;
    private bool _isRunning;

    protected override void Awake()
    {
        base.Awake();
        _characterController = GetComponent<CharacterController>();
        _moveSpeed = _ownerPlayer.PlayerStat.MovementSpeed;
    }

    private void Update()
    {
        if (!_photonView.IsMine)
        {
            UpdatePositionAndRotation();
            return;
        }
        
        Vector3 inputDirection = new Vector3(
            Input.GetAxis("Horizontal"),
            0f,
            Input.GetAxis("Vertical")
        ).normalized;

        ToggleRun();
        CheckGrounded();
        HandleJump();
        Move(inputDirection);
    }

    private void ToggleRun()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            _moveSpeed = _ownerPlayer.PlayerStat.RunSpeed;
            _isRunning = true;
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            CancelRun();
        }
    }

    private void CancelRun()
    {
        _moveSpeed = _ownerPlayer.PlayerStat.MovementSpeed;
        _isRunning = false;
    }
    
    private void UpdatePositionAndRotation()
    {
        transform.position = Vector3.Lerp(transform.position, _receivedPosition, Time.deltaTime * _moveSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, _receivedRotation, Time.deltaTime * _moveSpeed);
    }

    private void CheckGrounded()
    {
        if (_characterController.isGrounded && _velocity.y < 0f)
        {
            _velocity.y = -2f;
        }
    }

    private void HandleJump()
    {
        if (_characterController.isGrounded && Input.GetButtonDown("Jump"))
        {
            if (!_ownerPlayer.TryUseStamina(_ownerPlayer.PlayerStat.JumpStaminaCost)) return;
            _velocity.y = Mathf.Sqrt(_ownerPlayer.PlayerStat.JumpForce * -1f * _gravity);
        }
    }

    private void Move(Vector3 inputDirection)
    {
        Vector3 moveDirection = Vector3.zero;

        if (inputDirection.sqrMagnitude > 0.01f)
        {
            Vector3 forward = _cameraPosition.forward;
            Vector3 right = _cameraPosition.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            moveDirection = forward * inputDirection.z + right * inputDirection.x;
        }

        if (_isRunning && !_ownerPlayer.TryUseStamina(_ownerPlayer.PlayerStat.RunStaminaCostPerSecond * Time.deltaTime))
        { 
            CancelRun();
        }

        _velocity.y += _gravity * Time.deltaTime;

        Vector3 totalMove = moveDirection * _moveSpeed + _velocity;
        _animator.SetFloat("MoveTree", inputDirection.magnitude * _moveSpeed);

        _characterController.Move(totalMove * Time.deltaTime);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        { 
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else if (stream.IsReading)
        {
            _receivedPosition = (Vector3)stream.ReceiveNext();
            _receivedRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
