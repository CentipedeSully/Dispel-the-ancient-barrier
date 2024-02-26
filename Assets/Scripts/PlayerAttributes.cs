using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttributes : MonoBehaviour, IDamageable
{
    //Declarations
    [Header("External References")]
    [SerializeField] private GameManager _gameManager;
    private PlayerController _playerController;

    [Header("Player Attributes")]
    [SerializeField] private bool _isDead = false;
    [SerializeField] private bool _isRegenEnabled = true;

    [Header("Health")]
    [SerializeField] private float _health = 100;
    [SerializeField] private float _maxHealth = 100;
    [SerializeField] private float _healthRegen = 0;
    [SerializeField] private bool _isInvincible = false;
    [Tooltip("Allows the player some grace in between hits taken. Helps the player escape stun locks.")]
    [SerializeField] private float _invincibleRecoveryDuration = .15f;
    [SerializeField] private Animator _playerAnimator;
    [SerializeField] private string _onHurtTriggerParamName;
    [SerializeField] private string _invincibleStateParamName;
    [SerializeField] private float _screenShakeOnHitDuration;
    [SerializeField] private float _screenShakeOnHitMagnitude;
    [SerializeField] private float _timeStutterOnHitMagnitude;

    [Header("Energy")]
    [SerializeField] private float _energy = 100;
    [SerializeField] private float _maxEnergy = 100;
    [SerializeField] private float _energyRegen =5;

    [Header("Stamina")]
    [SerializeField] private float _stamina = 100;
    [SerializeField] private float _maxStamina = 100;
    [SerializeField] private float _staminaRegen = 20;

    [Header("Debug Commands")]
    [SerializeField] private bool _isDebugEnabled = false;
    [SerializeField] private float _debugValue;
    [SerializeField] private bool _changeHealthByValueCMD = false;
    [SerializeField] private bool _changeEnergyByValueCMD = false;
    [SerializeField] private bool _changeStaminaByValueCMD = false;

    [SerializeField] private bool _changeMaxHealthByValueCMD = false;
    [SerializeField] private bool _changeMaxEnergyByValueCMD = false;
    [SerializeField] private bool _changeMaxStaminaByValueCMD = false;

    [SerializeField] private bool _disableRegenCMD = false;
    [SerializeField] private bool _enableRegenCMD = false;

    [SerializeField] private bool _killCMD = false;
    [SerializeField] private bool _unkillCMD = false;


    //Monos
    private void Start()
    {
        if (_gameManager!= null)
            _playerController = _gameManager.GetPlayerController();

        SetHealth(_health);
        SetEnergy(_energy);
        SetStamina(_stamina);
    }

    private void Update()
    {
        ListenForDebugCommands();
        TickRegenIfAlive();
    }


    //Internal Uitls
    private void SetHealth(float newValue)
    {
        //Clamp the new value
        _health = Mathf.Clamp(newValue, 0, _maxHealth);

        //Update the healthbar
        _gameManager.UpdateHealthBar(_health / _maxHealth);

        //update death state if health is 0
        if (_health == 0)
            _isDead = true;

        //trigger feedback anim if health is full
        if (_health == _maxHealth)
            _gameManager.TriggerFillCompletedHealthFeedback();
    }

    private void SetEnergy(float newValue)
    {
        //Clamp the new value
        _energy = Mathf.Clamp(newValue, 0, _maxEnergy);

        //Update the energyBar
        _gameManager.UpdateEnergyBar(_energy / _maxEnergy);

        //Show feedback if energy is full
        if (_energy == _maxEnergy)
            _gameManager.TriggerFillCompletedEnergyFeedback();
    }

    private void SetStamina(float newValue)
    {
        //Clamp the new value
        _stamina = Mathf.Clamp(newValue, 0, _maxStamina);

        //Update the staminaBar
        _gameManager.UpdateStaminaBar(_stamina / _maxStamina);

        //trigger CompletedFill stamina feedback animation
        if (_stamina == _maxStamina)
            _gameManager.TriggerFillCompletedStaminaFeedback();
    }

    private void TickRegenIfAlive()
    {
        if (!_isDead)
        {
            RegenHealth();
            RegenEnergy();
            RegenStamina();
        }
    }

    private void RegenHealth()
    {
        if (_health < _maxHealth && _healthRegen > 0)
            SetHealth(_health + _healthRegen * Time.deltaTime);
    }

    private void RegenEnergy()
    {
        if (_energy < _maxEnergy && _energyRegen > 0)
            SetEnergy(_energy + _energyRegen * Time.deltaTime);
    }

    private void RegenStamina()
    {
        if (_stamina < _maxStamina && _staminaRegen > 0)
        SetStamina(_stamina + _staminaRegen * Time.deltaTime);
    }

    private void EndInvincibility()
    {
        _isInvincible = false;

        //exit invinc anim
        _playerAnimator.SetBool(_invincibleStateParamName, false);
    }


    //External Utils
    public void SetMaxHealth(float newValue)
    {
        //Make sure the new value isn't below 1
        _maxHealth = Mathf.Max(newValue, 1);

        //update the health
        SetHealth(_health);
    }

    public void SetMaxEnergy(float newValue)
    {
        //Make sure the new value isn't below 1
        _maxEnergy = Mathf.Max(newValue, 1);

        //update the energy
        SetEnergy(_energy);

    }

    public void SetMaxStamina(float newValue)
    {
        //Make sure the new value isn't below 1
        _maxStamina = Mathf.Max(newValue, 1);

        //update the stamina
        SetStamina(_stamina);

    }

    public void DisableRegen()
    {
        if (_isRegenEnabled)
            _isRegenEnabled = false;
    }

    public void EnableRegen()
    {
        if (!_isRegenEnabled)
            _isRegenEnabled = true;
    }

    public bool IsRegenActive()
    {
        return _isRegenEnabled;
    }

    public float GetHealth()
    {
        return _health;
    }

    public float GetEnergy()
    {
        return _energy;
    }

    public float GetStamina()
    {
        return _stamina;
    }

    public float GetMaxHealth()
    {
        return _maxHealth;
    }

    public float GetMaxEnergy()
    {
        return _maxEnergy;
    }

    public float GetMaxStamina()
    {
        return _maxStamina;
    }

    public float GetHealthRegen()
    {
        return _healthRegen;
    }

    public float GetEnergyRegen()
    {
        return _energyRegen;
    }

    public float GetStaminaRegen()
    {
        return _staminaRegen;
    }

    public void ModifyHealth(float value)
    {
        SetHealth(_health + value);

        //Trigger negatve effects if the player took damage
        if (value < 0)
        {
            //trigger screen shake
            _gameManager.ShakeScreen(_screenShakeOnHitMagnitude, _screenShakeOnHitDuration);

            //trigger time studder
            _gameManager.StutterTime(_timeStutterOnHitMagnitude);

            //trigger damaged anim
            _playerAnimator.SetTrigger(_onHurtTriggerParamName);


            //enter invinc anim
            _playerAnimator.SetBool(_invincibleStateParamName, true);

            //enter invincible recovery state
            _isInvincible = true;
            Invoke("EndInvincibility", _invincibleRecoveryDuration);

        }
    }

    public void ModifyEnergy(float value)
    {
        SetEnergy(_energy + value);
    }

    public void ModifyStamina(float value)
    {
        SetStamina(_stamina + value);
    }


    public void ModifyMaxHealth(float value)
    {
        SetMaxHealth(_maxHealth + value);
    }

    public void ModifyMaxEnergy(float value)
    {
        SetMaxEnergy(_maxEnergy + value);
    }

    public void ModifyMaxStamina(float value)
    {
        SetMaxStamina(_maxStamina + value);
    }

    public void SufferStun(float stunDuration)
    {
        //lock player actions
        _playerController.StunPlayer(stunDuration);
    }

    public bool IsInInvincibilityRecovery()
    {
        return _isInvincible;
    }


    //Debugging
    private void ListenForDebugCommands()
    {
        if (_isDebugEnabled)
        {
            if (_changeHealthByValueCMD)
            {
                _changeHealthByValueCMD = false;
                ModifyHealth(_debugValue);
            }

            if (_changeEnergyByValueCMD)
            {
                _changeEnergyByValueCMD = false;
                ModifyEnergy(_debugValue);
            }

            if (_changeStaminaByValueCMD)
            {
                _changeStaminaByValueCMD = false;
                ModifyStamina(_debugValue);
            }

            if (_changeMaxHealthByValueCMD)
            {
                _changeMaxHealthByValueCMD = false;
                ModifyMaxHealth(_debugValue);
            }

            if (_changeMaxEnergyByValueCMD)
            {
                _changeMaxEnergyByValueCMD = false;
                ModifyMaxEnergy(_debugValue);
            }

            if (_changeMaxStaminaByValueCMD)
            {
                _changeMaxStaminaByValueCMD = false;
                ModifyMaxStamina(_debugValue);
            }

            if (_disableRegenCMD)
            {
                _disableRegenCMD = false;
                DisableRegen();
            }

            if (_enableRegenCMD)
            {
                _enableRegenCMD = false;
                EnableRegen();
            }

            if (_killCMD)
            {
                _killCMD = false;
                _isDead = true;
            }

            if (_unkillCMD)
            {
                _unkillCMD = false;
                _isDead = false;
            }
        }
    }



}
