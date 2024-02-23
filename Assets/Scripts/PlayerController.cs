using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //Declarations
    [Header("External References")]
    [SerializeField] private GameManager _gameManager;
    private InputReader _inputReader;
    [SerializeField] private GameObject _playerObject;
    private Rigidbody _playerRB;
    private Vector2 _moveInput;
    private Vector3 _moveDirection;
    private bool _barrierInput;
    [SerializeField] private PlayerAttributes _playerAttributes;

    [Header("Movement Utilities")]
    [SerializeField] private bool _isMovementEnabled = true;
    [SerializeField] private int _moveSpeed;
    [SerializeField] private int _gravityModifier;
    [SerializeField] private int _moveSpeedCap;

    [Header("Dash Utilites")]
    [SerializeField] private int _dashForce;
    [SerializeField] private float _dashCooldown = .25f;
    [Tooltip("This script automatically offsets this value by the player's respective Regen attribute.")]
    [SerializeField] [Min(0)] private float _dashStaminaCost;
    [SerializeField] private bool _isDashReady = true;

    [Header("Jump Utilities")]
    [Tooltip("This script automatically offsets this value by the player's respective Regen attribute.")]
    [SerializeField] [Min(0)] private float _jumpStaminaCost;
    [SerializeField] private bool _isOnGround = false;
    [SerializeField] private Transform _feetPosition;
    [Tooltip("Starting from the feet, how far down will we check for solid ground?")]
    [SerializeField] private float _groundDetectionDepth = .2f;
    [Tooltip("Starting from the player's center, how far around the player will we check for ground?")]
    [SerializeField] private float _groundDetectionSurfaceArea = .5f;
    [SerializeField] private LayerMask _groundLayerMask;
    [SerializeField] private Color _groundDetectionGizmoColor = Color.magenta;
    [SerializeField] private bool _isJumping = false;
    [SerializeField] private bool _isJumpReady = true;
    [SerializeField] private int _jumpForce;
    [SerializeField] private int _jumpLevitationForce;
    [SerializeField] private float _jumpLevitationDuration;
    [SerializeField] private float _jumpCooldown = .2f;

    [Header("Barrier Utilities")]
    [Tooltip("This script automatically offsets this value by the player's respective Regen attribute.")]
    [SerializeField] [Min(0)] private float _initialBarrierEnergyCost;
    [Tooltip("This script automatically offsets this value by the player's respective Regen attribute.")]
    [SerializeField] [Min(0)] private float _barrierUpkeepEnergyCost;
    [SerializeField] private float _freeBarrierGracePeriod;
    [SerializeField] private bool _isBarrierCurrentlyFree = false;
    [SerializeField] private bool _isBarrierActive = false;
    [SerializeField] private bool _isBarrierAbilityReady = true;
    [SerializeField] private float _barrierCooldown = .3f;


    //Monos
    private void Start()
    {
        _inputReader = _gameManager.GetInputReader();
        if (_playerObject != null)
            _playerRB = _playerObject.GetComponent<Rigidbody>();
        
    }

    private void Update()
    {
        ReadInput();
        DetectGround();

        CastBarrier();
    }

    private void FixedUpdate()
    {
        CacheMoveDirection();
        ApplyMoreGravityIfFalling();
        DashPlayer();
        MovePlayer();
        JumpPlayer();
        //Clamp the velocity
    }

    private void OnDrawGizmosSelected()
    {
        DrawJumpDetectionGizmo();
    }

    //Internal Utils
    private void ReadInput()
    {
        _moveInput = _inputReader.GetMoveInput();
        _barrierInput = _inputReader.GetBarrierInput();
    }

    private void CacheMoveDirection()
    {
        //Calculate and cache this frame's current worldSpace moveDirection
        _moveDirection = new Vector3(_moveInput.x, 0, _moveInput.y).normalized;
    }

    private void MovePlayer()
    {
        if (_isMovementEnabled && _playerRB != null)
        {
            //Apply the velocity
            _playerRB.AddForce(_moveSpeed * Time.deltaTime * _moveDirection);
        }
    }

    private void JumpPlayer()
    {
        //are we legally trying to enter the jumpstate?
        if (_isOnGround && _inputReader.GetJumpInput() && _isJumpReady && !_isJumping)
        {
            //Check if we have enough stamina
            if (IsStaminaEnough(_jumpStaminaCost))
            {
                //Decrement stamina. Factor in the regen's presence if it's enabled
                if (_playerAttributes.IsRegenActive())
                    _playerAttributes.ModifyStamina(-(_jumpStaminaCost + _playerAttributes.GetStaminaRegen() * Time.deltaTime));
                else _playerAttributes.ModifyStamina(-_jumpStaminaCost);

                //Enter the jumpState
                _isJumping = true;
                Invoke("EndJump", _jumpLevitationDuration);

                //Apply Jump Force
                _playerRB.AddForce(Vector3.up * _jumpForce * Time.deltaTime, ForceMode.Impulse);

                //Enter the Jump Cooldown
                _isJumpReady = false;
                Invoke("ReadyJump", _jumpCooldown);
            }
            
            else
            {
                //trigger the insufficientStamina Feedback animation
                _gameManager.TriggerInsufficientStaminaFeedback();
            }
            
        }

        //are we currently in the jump state (while trying to prolong the jump)?
        else if (_inputReader.GetJumpInput() && _isJumping)
        {
            //Assist the jump via levitation
            _playerRB.AddForce(Vector3.up * _jumpLevitationForce * Time.deltaTime);
        }

        //are we discontinuing the jump early?
        else if (!_inputReader.GetJumpInput() && _isJumping)
        {
            //Exit the jump early
            CancelInvoke("EndJump");
            EndJump();
        }
    }

    private void DashPlayer()
    {
        if (_playerRB != null && _isOnGround && _isMovementEnabled && _isDashReady && _inputReader.GetDashInput())
        {
            //ONLY DASH IF THE PLAYER IS ALREADY MOVING!!
            if (_moveDirection.magnitude > 0)
            {
                //Make sure we have enough stamina
                if (IsStaminaEnough(_dashStaminaCost))
                {
                    //decrement stamina. factor in regen if it's enabled
                    if (_playerAttributes.IsRegenActive())
                        _playerAttributes.ModifyStamina(-(_dashStaminaCost + _playerAttributes.GetStaminaRegen() * Time.deltaTime));
                    else _playerAttributes.ModifyStamina(-_dashStaminaCost);

                    //Apply the dash force 
                    _playerRB.AddForce(_moveDirection * _dashForce * Time.deltaTime, ForceMode.Impulse);

                    //Enter Dash Cooldown
                    _isDashReady = false;
                    Invoke("ReadyDash", _dashCooldown);
                }

                else
                {
                    //trigger the insufficientStamina Feedback animation
                    _gameManager.TriggerInsufficientStaminaFeedback();
                }


            }
        }
    }

    private void ApplyMoreGravityIfFalling()
    {
        if (_playerRB != null)
        {
            //Add a more polished downwards force if the player is falling
            if (!_isOnGround && !_isJumping)
                _playerRB.AddForce(Vector3.down * Time.deltaTime * _gravityModifier);
        }
            
    }

    private void DetectGround()
    {
        //make the halfExtends argument more readable
        Vector3 halfExtends = new Vector3(_groundDetectionSurfaceArea / 2, _groundDetectionDepth / 2, _groundDetectionSurfaceArea / 2);

        //Properly offset the center of the boxscan
        Vector3 scanOrigin = new Vector3(_feetPosition.position.x, _feetPosition.position.y - _groundDetectionDepth/2, _feetPosition.position.z);

        //collect all collisions from the feet downwards.
        Collider[] detectedColliders = Physics.OverlapBox(scanOrigin, halfExtends, Quaternion.identity, _groundLayerMask);

        /*
        //Assume we're not on the ground anymore
        _isOnGround = false;

        //If we detect any ground, then correct our previous assumption
        foreach (Collider detection in detectedColliders)
        {
            if (detection.tag == "Ground")
            {
                _isOnGround = true;
                break;
            }
        }*/

        //update the groundstate
        if (detectedColliders.Length > 0)
            _isOnGround = true;
        else _isOnGround = false;
    }

    private void ReadyJump()
    {
        if (!_isJumpReady)
            _isJumpReady = true;
    }

    private void EndJump()
    {
        if (_isJumping)
            _isJumping = false;
    }

    private void ReadyDash()
    {
        if (!_isDashReady)
            _isDashReady = true;
    }

    private bool IsStaminaEnough(float staminaCost)
    {
        return staminaCost <= _playerAttributes.GetStamina();
    }

    private bool IsEnergyEnough(float energyCost)
    {
        return energyCost <= _playerAttributes.GetEnergy();
    }

    private void CastBarrier()
    {
        //Is the player casting a new barrier
        if (_barrierInput && _isBarrierAbilityReady && !_isBarrierActive)
        {
            //Make sure we have enough energy
            if (IsEnergyEnough(_initialBarrierEnergyCost))
            {
                //decrement the energy cost. Factor in the regen if it's enabled
                if (_playerAttributes.IsRegenActive())
                    _playerAttributes.ModifyEnergy(-(_initialBarrierEnergyCost + _playerAttributes.GetEnergyRegen() * Time.deltaTime));
                else _playerAttributes.ModifyEnergy(-_initialBarrierEnergyCost);

                //create the barrier
                _isBarrierActive = true;

                //cooldown the barrier ability
                _isBarrierAbilityReady = false;
                Invoke("ReadyBarrierAbility", _barrierCooldown);


                //Enter the free barrier grace period
                _isBarrierCurrentlyFree = true;
                Invoke("ExitFreeBarrierGracePeriod", _freeBarrierGracePeriod);
            }


            //The barrier never materializes due to lack of energy
            else
            {
                //trigger the insufficient energy UI feedback
                _gameManager.TriggerInsufficientEnergyFeedback();
            }
        }


        //Is the player is attempting to sustain a barrier
        else if (_isBarrierActive && _barrierInput)
        {
            //if the barrier is currently free
            if (_isBarrierCurrentlyFree)
            {
                //Suppress the player's passive energy regen
                if (_playerAttributes.IsRegenActive())
                    _playerAttributes.ModifyEnergy(-_playerAttributes.GetEnergyRegen() * Time.deltaTime);

                //show drain feedback, even though the barrier is currently free :)
                _gameManager.UpdateDrainingEnergyAnimationFeedback(true);
            }


            //Otherwise make sure we have enough energy if the barrier isn't free
            else if (IsEnergyEnough(_barrierUpkeepEnergyCost) && !_isBarrierCurrentlyFree)
            {
                //Decrement energy. Factor in the player's regen if it's enabled
                if (_playerAttributes.IsRegenActive())
                    _playerAttributes.ModifyEnergy(-(_barrierUpkeepEnergyCost + _playerAttributes.GetEnergyRegen()) * Time.deltaTime);
                else _playerAttributes.ModifyEnergy(-_barrierUpkeepEnergyCost * Time.deltaTime);

                //Keep the draining state alive
                _gameManager.UpdateDrainingEnergyAnimationFeedback(true);
            }

            
            //If the player ran out of energy
            else if (!IsEnergyEnough(_barrierUpkeepEnergyCost) && !_isBarrierCurrentlyFree)
            {
                //End the draining animation
                _gameManager.UpdateDrainingEnergyAnimationFeedback(false);

                //Trigger the insufficient energy feedback animation
                _gameManager.TriggerInsufficientEnergyFeedback();

                //Leave the active barrier state
                _isBarrierActive = false;
            }
        }

        //Is the player ending their barrier
        else if (_isBarrierActive && !_barrierInput)
        {
            //End the draining animation
            _gameManager.UpdateDrainingEnergyAnimationFeedback(false);

            //Cancel any previous unfinished Invokes
            if (_isBarrierCurrentlyFree)
            {
                _isBarrierCurrentlyFree = false;
                CancelInvoke("ExitFreeBarrierGracePeriod");
            }

            //Deactivate the barrier
            _isBarrierActive = false;
        }
    }

    private void ReadyBarrierAbility()
    {
        _isBarrierAbilityReady = true;
    }

    private void ExitFreeBarrierGracePeriod()
    {
        _isBarrierCurrentlyFree = false;
    }

    //External Utils
    public GameObject GetPlayer()
    {
        return _playerObject;
    }

    public void SetPlayer(GameObject newPlayerObj)
    {
        if (newPlayerObj != null)
        {
            //Set player obj
            _playerObject = newPlayerObj;

            //Set Camera to follow new obj

        }
            
    }


    //Debugging
    private void DrawJumpDetectionGizmo()
    {
        //Set the color
        Gizmos.color = _groundDetectionGizmoColor;

        //make the cube dimensions more readable for later
        Vector3 detectionCubeDimensions = new Vector3(_groundDetectionSurfaceArea, _groundDetectionDepth, _groundDetectionSurfaceArea);

        //Offset the y by boxHeight/2. Doing this will draw the box properly-- from the feet downwards
        Vector3 detectionCubeOrigin = new Vector3(_feetPosition.position.x, _feetPosition.position.y - _groundDetectionDepth/2, _feetPosition.position.z);

        //Draw the cube
        Gizmos.DrawWireCube(detectionCubeOrigin, detectionCubeDimensions);
    }



}
