using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //Declarations
    [SerializeField] private bool _isTimeStopped = false;
    [Header("Player Utilities")]
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private CamFocusController _cameraFocus;

    [Header("UI Attribute Display Utilities")]
    [SerializeField] private GameObject _attributeBarsDisplay;
    [SerializeField] private GameObject _healthbar;
    [SerializeField] private GameObject _energyBar;
    [SerializeField] private GameObject _staminaBar;
    [SerializeField] private GameObject _healthDisplay;
    [SerializeField] private GameObject _energyDisplay;
    [SerializeField] private GameObject _staminaDisplay;

    [Header("UI Attribute Animation Utilities")]
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


    [Header("UI Controls Display Utilities")]
    [SerializeField] private GameObject _controlsDisplay;
    [SerializeField] private GameObject _upKeyHighlight;
    [SerializeField] private GameObject _leftKeyHighlight;
    [SerializeField] private GameObject _downKeyHighlight;
    [SerializeField] private GameObject _rightKeyHighlight;
    [SerializeField] private GameObject _barrierKeyHighlight;
    [SerializeField] private GameObject _dashKeyHighlight;
    [SerializeField] private GameObject _jumpKeyHighlight;

    //Monobehaviors
    private void Update()
    {
        ShowButtonControlInputs();
    }


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

    public void UpdateButtonHighlight(bool isButtonPressed, GameObject highlight)
    {
        if (highlight != null)
            highlight.SetActive(isButtonPressed);
    }

    private void ShowButtonControlInputs()
    {
        if (_controlsDisplay.activeSelf)
        {
            UpdateUpKeyHighlight();
            UpdateLeftKeyHighlight();
            UpdateDownKeyHighlight();
            UpdateRightKeyHighlight();

            UpdateBarrierKeyHighlight();
            UpdateDashKeyHighlight();
            UpdateJumpKeyHighlight();
        }
    }

    private IEnumerator ManageTimeStop(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        _isTimeStopped = false;
        Time.timeScale = 1;
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
        SetBar(newRelativePercentage, _healthbar);

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



    public void UpdateUpKeyHighlight()
    {
        bool isButtonPressed = _inputReader.GetMoveInput().y > 0;
        UpdateButtonHighlight(isButtonPressed, _upKeyHighlight);
    }

    public void UpdateLeftKeyHighlight()
    {
        bool isButtonPressed = _inputReader.GetMoveInput().x < 0;
        UpdateButtonHighlight(isButtonPressed, _leftKeyHighlight);
    }

    public void UpdateDownKeyHighlight()
    {
        bool isButtonPressed = _inputReader.GetMoveInput().y < 0;
        UpdateButtonHighlight(isButtonPressed, _downKeyHighlight);
    }

    public void UpdateRightKeyHighlight()
    {
        bool isButtonPressed = _inputReader.GetMoveInput().x > 0;
        UpdateButtonHighlight(isButtonPressed, _rightKeyHighlight);
    }

    public void UpdateBarrierKeyHighlight()
    {
        UpdateButtonHighlight(_inputReader.GetBarrierInput(), _barrierKeyHighlight);
    }

    public void UpdateDashKeyHighlight()
    {
        UpdateButtonHighlight(_inputReader.GetDashInput(), _dashKeyHighlight);
    }

    public void UpdateJumpKeyHighlight()
    {
        UpdateButtonHighlight(_inputReader.GetJumpInput(), _jumpKeyHighlight);
    }

    public void ShakeScreen(float magnitude, float duration)
    {
        _cameraFocus.TriggerCameraShake(duration, magnitude);
    }

    public void StutterTime(float duration)
    {
        if (!_isTimeStopped)
        {
            _isTimeStopped = true;
            Time.timeScale = 0;
            StartCoroutine(ManageTimeStop(duration));
        }
        
    }

    //Debugging



}
