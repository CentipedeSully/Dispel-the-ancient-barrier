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
    [SerializeField] private GameObject _barrierObject;

    [Header("Movement Utilities")]
    [SerializeField] private bool _isStunned = false;
    [SerializeField] private bool _isMovementEnabled = true;
    [SerializeField] private int _moveSpeed;
    [SerializeField] private int _gravityModifier;
    [SerializeField] private int _moveSpeedCap;

    [Header("Dash Utilites")]
    [SerializeField] private int _dashForce;
    [SerializeField] private float _dashCooldown = .25f;
    [Tooltip("This script automatically offsets this value by the player's respective Regen attribute.")]
    [SerializeField] [Min(0)] private float _dashStaminaCost;
    private bool _isDashReady = true;

    [Header("Jump Utilities")]
    [Tooltip("This script automatically offsets this value by the player's respective Regen attribute.")]
    [SerializeField] [Min(0)] private float _jumpStaminaCost;
    private bool _isOnGround = false;
    [SerializeField] private Transform _feetPosition;
    [Tooltip("Starting from the feet, how far down will we check for solid ground?")]
    [SerializeField] private float _groundDetectionDepth = .2f;
    [Tooltip("Starting from the player's center, how far around the player will we check for ground?")]
    [SerializeField] private float _groundDetectionSurfaceArea = .5f;
    [SerializeField] private LayerMask _groundLayerMask;
    [SerializeField] private Color _groundDetectionGizmoColor = Color.magenta;
    private bool _isJumping = false;
    private bool _isJumpReady = true;
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
    private bool _isBarrierCurrentlyFree = false;
    private bool _isBarrierActive = false;
    private bool _isBarrierAbilityReady = true;
    [SerializeField] private float _barrierCooldown = .3f;
    [SerializeField] private float _barrierRadius;
    [Tooltip("This is an impulse force")]
    [SerializeField] private float _barrierPushForce;
    [SerializeField] [Min(0)] private float _barrierDamage;
    [SerializeField] [Min(0)] private float _barrierOnContactDrain;
    [Tooltip("This is an impulse force. Note the recoil may add up if deflecting many entities at once ;3")]
    [SerializeField] [Min(0)] private float _barrierRecoilMagnitude;
    [SerializeField] private Color _barrierGizmoColor = Color.blue;
    [SerializeField] private LayerMask _barrierRepulsableLayerMask;
    [SerializeField] private List<string> _validBarrierCollisionTags;
    private Vector3 _barrierRecoilVector;
    private Dictionary<int, float> _repulsedEntitiesDict; // holds (rigidbody ID, remainingCooldown) key-value pairs
    [Tooltip("How long will the barrier wait before repulsing a previously pushed entity. " +
        "Used to avoid re-Repulsing entities that've already been pushed back but have failed to get out of the barrier=s radius")]
    [SerializeField] [Min(.05f)] private float _instanceRepulsionCooldown;




    //Monos
    private void Start()
    {
        _inputReader = _gameManager.GetInputReader();
        if (_playerObject != null)
            _playerRB = _playerObject.GetComponent<Rigidbody>();

        //Initialize the list if it hasn't yet been defined;
        if (_validBarrierCollisionTags == null)
            _validBarrierCollisionTags = new List<string>();

        _repulsedEntitiesDict = new Dictionary<int, float>();
        
    }

    private void Update()
    {
        ReadInput();
        DetectGround();

        CastBarrier();
        ControlBarrierVisibility();
        TickInternalRepulsions();
        RepulseEntitiesWithinBarrier();
    }

    private void FixedUpdate()
    {
        CacheMoveDirection();
        ApplyMoreGravityIfFalling();
        DashPlayer();
        MovePlayer();
        JumpPlayer();
        ApplyRecoil();
    }

    private void OnDrawGizmosSelected()
    {
        DrawJumpDetectionGizmo();
        DrawBarrierGizmo();
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
        if (_isMovementEnabled && _playerRB != null && !_isStunned)
        {
            //Apply the velocity
            _playerRB.AddForce(_moveSpeed * Time.deltaTime * _moveDirection);
        }
    }

    private void JumpPlayer()
    {
        //are we legally trying to enter the jumpstate?
        if (_isOnGround && _inputReader.GetJumpInput() && _isJumpReady && !_isJumping && !_isStunned)
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
        else if (_inputReader.GetJumpInput() && _isJumping && !_isStunned)
        {
            //Assist the jump via levitation
            _playerRB.AddForce(Vector3.up * _jumpLevitationForce * Time.deltaTime);
        }

        //are we discontinuing the jump early?
        else if ((!_inputReader.GetJumpInput() && _isJumping) || (_isJumping && _isStunned))
        {
            //Exit the jump early
            CancelInvoke("EndJump");
            EndJump();
        }
    }

    private void DashPlayer()
    {
        if (_playerRB != null && _isOnGround && _isMovementEnabled && _isDashReady && _inputReader.GetDashInput() && !_isStunned)
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
        if (_barrierInput && _isBarrierAbilityReady && !_isBarrierActive && !_isStunned)
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
        else if (_isBarrierActive && _barrierInput && !_isStunned)
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
        else if ((_isBarrierActive && !_barrierInput) || _isStunned && _isBarrierActive)
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

    private void ExitStun()
    {
        _isStunned = false;
    }

    private bool IsDetectionValid(Collider detectedCollider)
    {
        if (detectedCollider == null)
            return false;

        if (_validBarrierCollisionTags.Contains(detectedCollider.tag))
            return true;
        else return false;
    }

    private void RepulseEntitiesWithinBarrier()
    {
        if (_isBarrierActive)
        {
            //Cast a sphere on player's position, and catch all detections
            Collider[] detectedColliders = Physics.OverlapSphere(transform.position, _barrierRadius, _barrierRepulsableLayerMask);

            bool hasRepulsionOccured = false;
            bool hasDamageBeenDealth = false;

            for (int i = 0; i < detectedColliders.Length; i++)
            {
                if (IsDetectionValid(detectedColliders[i]))
                {
                    Rigidbody entityRB = detectedColliders[i].GetComponent<Rigidbody>();

                    //If this entity has a rigidbody to push..
                    if (entityRB != null)
                    {
                        //make sure this entity hasn't already been repulsed
                        if (!_repulsedEntitiesDict.ContainsKey(entityRB.GetInstanceID()))
                        {
                            //Add a new record of the entity
                            _repulsedEntitiesDict.Add(entityRB.GetInstanceID(), _instanceRepulsionCooldown);

                            //get the entity's directional distance vector
                            Vector3 vectorFromSelfToEntity = (entityRB.transform.position - transform.position);

                            //ignore the y if it's below 0. This way the entity won't get stuck on the floor
                            vectorFromSelfToEntity = new Vector3(vectorFromSelfToEntity.x, Mathf.Max(vectorFromSelfToEntity.y, 0),vectorFromSelfToEntity.z).normalized;

                            //repulse entity
                            entityRB.AddForce(vectorFromSelfToEntity * _barrierPushForce * Time.deltaTime, ForceMode.Impulse);

                            //Accrue the kickback force from pushing the entity. 
                            _barrierRecoilVector += -vectorFromSelfToEntity; //make sure it's negative

                            hasRepulsionOccured = true;
                        }
                    }


                    /* 
                    //if entity is damageable...
                    IDamageable entityAttributes = detectedColliders[i].GetComponent<IDamageable>();

                    if (entityAttributes != null)
                    {
                        //Deal damage to the detection's damageable attribute, if it exists
                        detectedColliders[i].GetComponent<IDamageable>()?.ModifyHealth(_barrierDamage);

                        //Set stun
                        detectedColliders[i].GetComponent<IDamageable>()?.SufferStun(_barrierDamage);

                        hasDamageBeenDealt = true;
                    }
                    */

                    //trigger feedback and pay costs if anything has occured
                    if (hasDamageBeenDealth || hasRepulsionOccured)
                    {
                        //trigger repulsion feedback anim
                        //...

                        //reduce the player's energy if the barrier isn't free
                        if (!_isBarrierCurrentlyFree)
                            _playerAttributes.ModifyEnergy(-_barrierOnContactDrain);
                    }
                }
            }
        }

    }

    private void ApplyRecoil()
    {
        if (_barrierRecoilVector.magnitude > 0)
        {
            //repulse player by the accrued recoil
            _playerRB.AddForce(_barrierRecoilVector * _barrierRecoilMagnitude * Time.deltaTime, ForceMode.Impulse);

            //clear the accrued recoil
            _barrierRecoilVector = Vector3.zero;
        }
    }

    private void TickInternalRepulsions()
    {
        //create a temporary, updated collection of records. [Dictionaries can't be modified while being iterated]
        Dictionary<int, float> updatedRecordsDict = new Dictionary<int, float>();

        //populate the temporary records ceollection with newly updated records
        foreach( KeyValuePair<int,float> repulsedRecord in _repulsedEntitiesDict)
        {
            float newCooldownValue = _repulsedEntitiesDict[repulsedRecord.Key] - Time.deltaTime;

            //only add an updated version of this record if its new cooldown isn't expired
            if (newCooldownValue > 0)
                updatedRecordsDict.Add(repulsedRecord.Key, newCooldownValue);
        }

        //overwrite the original dict
        _repulsedEntitiesDict = updatedRecordsDict;
            
    }

    private void ControlBarrierVisibility()
    {
        if (_isBarrierActive)
        {
            //show barrier if it's not showing
            if (!_barrierObject.activeSelf)
                _barrierObject.SetActive(true);
        }
        else
        {
            //hide barrier if it's still showing
            if (_barrierObject.activeSelf)
                _barrierObject.SetActive(false);
        }
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


    public void StunPlayer(float duration)
    {
        _isStunned = true;
        Invoke("ExitStun", duration);
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

    private void DrawBarrierGizmo()
    {
        //only draw when the barrier is up
        if (_isBarrierActive)
        {
            //set the color
            Gizmos.color = _barrierGizmoColor;

            //draw the sphere
            Gizmos.DrawWireSphere(transform.position, _barrierRadius);
        }


    }

}
