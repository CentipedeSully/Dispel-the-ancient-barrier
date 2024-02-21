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

    [Header("Movement Utilities")]
    [SerializeField] private bool _isMovementEnabled = true;
    [SerializeField] private int _moveSpeed;
    [SerializeField] private int _gravityModifier;
    [SerializeField] private int _moveSpeedCap;

    [Header("Jump Utilities")]
    [SerializeField] private bool _isOnGround = false;
    [SerializeField] private Transform _feetPosition;
    [Tooltip("Starting from the feet, how far down will we check for solid ground?")]
    [SerializeField] private float _groundDetectionDepth = .2f;
    [Tooltip("Starting from the player's center, how far around the player will we check for ground?")]
    [SerializeField] private float _groundDetectionSurfaceArea = .5f;
    [SerializeField] private Color _groundDetectionGizmoColor = Color.magenta;
    [SerializeField] private bool _isJumping = false;
    [SerializeField] private bool _isJumpReady = true;
    [SerializeField] private int _jumpForce;
    [SerializeField] private int _jumpLevitationForce;
    [SerializeField] private float _jumpLevitationDuration;
    [SerializeField] private float _jumpCooldown = .2f;


    //Monos
    private void Start()
    {
        _inputReader = _gameManager.GetInputReader();
        if (_playerObject != null)
            _playerRB = _playerObject.GetComponent<Rigidbody>();
        
    }

    private void Update()
    {
        DetectGround();
    }

    private void FixedUpdate()
    {
        ApplyMoreGravityIfFalling();
        MovePlayer();
        JumpPlayer();
        //Clamp the velocity
    }

    private void OnDrawGizmosSelected()
    {
        DrawJumpDetectionGizmo();
    }

    //Internal Utils
    private void MovePlayer()
    {
        if (_isMovementEnabled && _playerRB != null)
        {
            //get the readInput
            Vector2 moveVector = _inputReader.GetMoveInput().normalized;

            //translate the moveVector into world space
            Vector3 worldMoveVector = new Vector3(moveVector.x, 0, moveVector.y); // only move along x & z

            //Apply the velocity
            _playerRB.AddForce(_moveSpeed * Time.deltaTime * worldMoveVector);
        }
    }

    private void JumpPlayer()
    {
        //are we legally trying to enter the jumpstate?
        if (_isOnGround && _inputReader.GetJumpInput() && _isJumpReady && !_isJumping)
        {
            //Enter the jumpState
            _isJumping = true;
            Invoke("EndJump", _jumpLevitationDuration);

            //Apply Jump Force
            _playerRB.AddForce(Vector3.up * _jumpForce * Time.deltaTime, ForceMode.Impulse);

            //Enter the Jump Cooldown
            _isJumpReady = false;
            Invoke("ReadyJump", _jumpCooldown);
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
        Collider[] detectedColliders = Physics.OverlapBox(scanOrigin, halfExtends);

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
        }
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
