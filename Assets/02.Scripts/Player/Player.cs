using Photon.Pun;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    public PlayerStat PlayerStat => _playerStat;
    private PlayerStat _playerStat;
    
    [SerializeField] private TextMeshProUGUI _playerNameText;
    [SerializeField] private float _currentHealth;
    [SerializeField] private float _currentStamina;
    [SerializeField] private GameObject _minimpapIconGreen;
    [SerializeField] private GameObject _minimpapIconRed;
    
    private PhotonView _photonView;
    private float _staminaRegenTimer;
    private bool _staminaRegenActive = true;
    
    private void Awake()
    {
        _photonView = GetComponent<PhotonView>();
        _playerStat = GetComponent<PlayerStat>();
    }
    
    private void Start()
    {
        _currentHealth = _playerStat.MaxHealth;
        _currentStamina = _playerStat.MaxStamina;
        _playerNameText.text = $"{_photonView.Owner.NickName}_{_photonView.OwnerActorNr}";
        if (_photonView.IsMine)
        {
            _playerNameText.color = Color.green;
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
            HUD.Instance.UpdateStaminaBar(_currentStamina);
        }
    }
    
    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;
        if (_currentHealth <= 0)
        {
            Debug.Log("Player has died.");
        }
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
            HUD.Instance.UpdateStaminaBar(_currentStamina);
        }
        return true;
    }
}
