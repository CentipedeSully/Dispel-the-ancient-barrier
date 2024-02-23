using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager: MonoBehaviour
{
    //Declarations
    [Header("Player Utilities")]
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private PlayerController _playerController;

    [Header("UI Utilities")]
    [SerializeField] private GameObject _healthBar;
    [SerializeField] private GameObject _energyBar;
    [SerializeField] private GameObject _staminaBar;
    [SerializeField] private GameObject _healthDisplay;
    [SerializeField] private GameObject _energyDisplay;
    [SerializeField] private GameObject _staminaDisplay;

    [Header("Animation Utilities")]
    [SerializeField] private Animator _healthBarAnimator;
    [SerializeField] [Range(0, 1)] private float _lowHealthThreshold;
    [SerializeField] private string _healthLowParamName;
    [SerializeField] private string _healthFillCompleteParamName;

    [SerializeField] private Animator _energyBarAnimator;
    [SerializeField] private string _energyInsufficientParamName;
    [SerializeField] private string _energyFillCompleteParamName;
    [SerializeField] private string _energyDrainingParamName;

    [SerializeField] private Animator _staminaBarAnimator;
    [SerializeField] private string _staminaInsufficientParamName;
    [SerializeField] private string _staminaFillCompleteParamName;

    //[SerializeField] private GameObject _darkFadeObject;
    //[SerializeField] private GameObject _lightFadeObject;


    //Internal Utils
    private void SetBar(float newPercentage, GameObject barObject)
    {
        //ignore the command if the bar doesn't exist
        if (barObject == null)
            return;

        //Clamp the value
        newPercentage = Mathf.Clamp(newPercentage, 0, 1.0f);

        //Round the value to the nearest hundreth
        newPercentage = ((int)(newPercentage * 100)) / 100f;

        //Update the bar object
        Vector3 newScale = new Vector3(newPercentage, barObject.transform.localScale.y, barObject.transform.localScale.z);
        barObject.transform.localScale = newScale;

    }

    private void ShowDisplay(GameObject display)
    {
        if (display != null)
            display.SetActive(true);
    }

    private void HideDisplay(GameObject display)
    {
        if (display != null)
            display.SetActive(false);
    }


    //External Utils
    public InputReader GetInputReader()
    {
        return _inputReader;
    }

    public PlayerController GetPlayerController()
    {
        return _playerController;
    }

    public void UpdateHealthBar(float newRelativePercentage)
    {
        //Update the bar's size
        SetBar(newRelativePercentage, _healthBar);

        //Update the bar's feedback state
        bool _isHealthLow = newRelativePercentage <= _lowHealthThreshold;
        _healthBarAnimator.SetBool(_healthLowParamName, _isHealthLow);
    }

    public void UpdateEnergyBar(float newRelativePercentage)
    {
        SetBar(newRelativePercentage, _energyBar);
    }

    public void UpdateStaminaBar(float newRelativePercentage)
    {
        SetBar(newRelativePercentage, _staminaBar);
    }

    public void ShowAllDisplays()
    {
        ShowDisplay(_healthDisplay);
        ShowDisplay(_staminaDisplay);
        ShowDisplay(_energyDisplay);
    }

    public void HideAllDisplays()
    {
        HideDisplay(_healthDisplay);
        HideDisplay(_staminaDisplay);
        HideDisplay(_energyDisplay);
    }

    public void ShowHealth()
    {
        ShowDisplay(_healthDisplay);
    }

    public void ShowEnergy()
    {
        ShowDisplay(_energyDisplay);
    }

    public void ShowStamina()
    {
        ShowDisplay(_staminaDisplay);
    }


    public void TriggerFillCompletedHealthFeedback()
    {
        _healthBarAnimator.SetTrigger(_healthFillCompleteParamName);
    }


    public void TriggerInsufficientEnergyFeedback()
    {
        _energyBarAnimator.SetTrigger(_energyInsufficientParamName);
    }

    public void TriggerFillCompletedEnergyFeedback()
    {
        _energyBarAnimator.SetTrigger(_energyFillCompleteParamName);
    }

    public void UpdateDrainingEnergyAnimationFeedback(bool isDrainingEnergy)
    {
        _energyBarAnimator.SetBool(_energyDrainingParamName, isDrainingEnergy);
    }


    public void TriggerInsufficientStaminaFeedback()
    {
        _staminaBarAnimator.SetTrigger(_staminaInsufficientParamName);
    }

    public void TriggerFillCompletedStaminaFeedback()
    {
        _staminaBarAnimator.SetTrigger(_staminaFillCompleteParamName);
    }
    
    //Debugging




}
