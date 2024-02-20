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
    //[SerializeField] private int _jumpForce;
    //[SerializeField] private bool _isJumping;


    //Monos
    private void Start()
    {
        _inputReader = _gameManager.GetInputReader();
        _playerRB = _playerObject.GetComponent<Rigidbody>();
        
    }

    private void FixedUpdate()
    {
        ApplyGravityModifier();
        MovePlayer();
        //Clamp the velocity
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




}
