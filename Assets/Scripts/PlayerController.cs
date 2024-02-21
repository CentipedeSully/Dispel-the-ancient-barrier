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
    [SerializeField] private bool _jumpDuration;
    [SerializeField] private int _jumpForce;


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
        ApplyGravityModifier();
        MovePlayer();
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

    private void ApplyGravityModifier()
    {
        if (_playerRB != null)
            _playerRB.AddForce(Vector3.down * Time.deltaTime * _gravityModifier);
    }

    private void DetectGround()
    {

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
        Gizmos.color = _groundDetectionGizmoColor;

        Vector3 detectionCubeDimensions = new Vector3(_groundDetectionSurfaceArea, _groundDetectionDepth, _groundDetectionSurfaceArea);

        //Offset the y by boxHeight/2
        Vector3 detectionCubeOrigin = new Vector3(_feetPosition.position.x, _feetPosition.position.y - _groundDetectionDepth/2, _feetPosition.position.z); 
        Gizmos.DrawWireCube(detectionCubeOrigin, detectionCubeDimensions);
    }



}
