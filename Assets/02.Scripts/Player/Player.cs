using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;

public class Player : MonoBehaviour, IDamaged
{
    public PlayerStat PlayerStat => _playerStat;
    private PlayerStat _playerStat;
    
    [SerializeField] private TextMeshProUGUI _playerNameText;
    [SerializeField] private PlayerWorldSpaceCanvas _worldCanvas;
    [SerializeField] private float _currentHealth;
    [SerializeField] private float _currentStamina;
    [SerializeField] private GameObject _minimpapIconGreen;
    [SerializeField] private GameObject _minimpapIconRed;
    [SerializeField] private Animator _animator;
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private CinemachineImpulseSource _cinemachineImpulseSource;
    
    private PhotonView _photonView;
    private float _staminaRegenTimer;
    private bool _staminaRegenActive = true;
    public bool IsDead = false;
    
    private void Awake()
    {
        Debug.Log("Awake");
        _photonView = GetComponent<PhotonView>();
        _playerStat = GetComponent<PlayerStat>();
        _worldCanvas = GetComponent<PlayerWorldSpaceCanvas>();
        _characterController = GetComponent<CharacterController>();
        _cinemachineImpulseSource = GetComponent<CinemachineImpulseSource>();
        _currentHealth = _playerStat.MaxHealth;
        _currentStamina = _playerStat.MaxStamina;
    }
    
    private void Start()
    {
        Debug.Log("Player Start");
        _worldCanvas.SetMaxHealth(_playerStat.MaxHealth);
        _worldCanvas.UpdateHealthBar(_currentHealth);
        _playerNameText.text = $"{_photonView.Owner.NickName}_{_photonView.OwnerActorNr}";
        if (_photonView.IsMine)
        {
            _playerNameText.color = Color.green;
            HUD.Instance.SetMaxHealth(_playerStat.MaxHealth);
            HUD.Instance.SetMaxStamina(_playerStat.MaxStamina);
            _minimpapIconGreen.SetActive(true);
        }
        else
        {
            _playerNameText.color = Color.red;
            _minimpapIconRed.SetActive(true);
        }
    }

    private void Update()
    {
        StaminaRegen();   
    }

    private void StaminaRegen()
    {
        if (!_staminaRegenActive)
        {
            _staminaRegenTimer -= Time.deltaTime;
            if (_staminaRegenTimer <= 0)
            {
                _staminaRegenActive = true;
            }
            return;
        }
        _currentStamina += _playerStat.StaminaRegenPerSecond * Time.deltaTime;
        _currentStamina = Mathf.Min(_playerStat.MaxStamina, _currentStamina);
        if (_photonView.IsMine)
        {
            EventManager.Instance.OnPlayerStaminaChanged?.Invoke(_currentStamina);
        }
    }
    
    [PunRPC]
    public void Damaged(float damage)
    {
        if (IsDead) return;
        
        _currentHealth -= damage;
        _currentHealth = Mathf.Max(_currentHealth, 0);
        _worldCanvas.UpdateHealthBar(_currentHealth);
        if (_photonView.IsMine)
        {
            EventManager.Instance.OnPlayerHealthChanged?.Invoke(_currentHealth);
            _cinemachineImpulseSource.GenerateImpulse();
        }
        if (_currentHealth <= 0)
        {
            StartCoroutine(OnDeathCoroutine());
        }
    }

    public IEnumerator OnDeathCoroutine()
    {
        IsDead = true;
        _characterController.enabled = false;
        _animator.SetTrigger("Die");
        
        yield return new WaitForSeconds(5f);
        
        Debug.Log("Respawning player...");
        _currentHealth = _playerStat.MaxHealth;
        _currentStamina = _playerStat.MaxStamina;
        _worldCanvas.SetMaxHealth(_playerStat.MaxHealth);
        _worldCanvas.UpdateHealthBar(_currentHealth);
        _staminaRegenActive = true;
        _staminaRegenTimer = _playerStat.StaminaRegenDelay;
        IsDead = false;
        _animator.SetTrigger("Respawn");
        if (_photonView.IsMine)
        {
            gameObject.transform.position = RandomSpawn.Instance.GetRandomSpawnPoint();
            EventManager.Instance.OnPlayerHealthChanged?.Invoke(_currentHealth);
            EventManager.Instance.OnPlayerStaminaChanged?.Invoke(_currentStamina);
        }
        _characterController.enabled = true;
        Debug.Log("Player respawned.");
    }
    
    public bool TryUseStamina(float amount)
    {
        if (_currentStamina < amount)
        {
            return false;
        }
        _currentStamina -= amount;
        _staminaRegenActive = false;
        _staminaRegenTimer = _playerStat.StaminaRegenDelay;
        if (_photonView.IsMine)
        {
            EventManager.Instance.OnPlayerStaminaChanged?.Invoke(_currentStamina);
        }
        return true;
    }
}
