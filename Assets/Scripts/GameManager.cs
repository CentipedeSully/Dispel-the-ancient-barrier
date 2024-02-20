using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager: MonoBehaviour
{
    //Declarations
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private PlayerController _playerController;


    //Internal Utils




    //External Utils
    public InputReader GetInputReader()
    {
        return _inputReader;
    }

    public PlayerController GetPlayerController()
    {
        return _playerController;
    }


    //Debugging




}
