using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour
{
    //Declarations
    [SerializeField] private Vector2 _moveInput = Vector2.zero;
    [SerializeField] private bool _barrierInput = false;
    [SerializeField] private bool _burstInput = false;


    //Monos
    //...



    //Internal Utils
    //...



    //External Utils
    public void SetMoveInput(InputAction.CallbackContext context)
    {
        if (context.started || context.performed) //Read the move input only if something's being pressed
            _moveInput= context.ReadValue<Vector2>();

        else 
            _moveInput= Vector2.zero; 
    }


    public void SetBarrierInput(InputAction.CallbackContext context)
    {
        _barrierInput = context.ReadValueAsButton();
    }

    public void SetBurstInput(InputAction.CallbackContext context)
    {
        _burstInput = context.ReadValueAsButton();
    }

    public Vector2 GetMoveInput()
    {
        return _moveInput;
    }

    public bool GetBarrierInput()
    {
        return _barrierInput;
    }

    public bool GetBurstInput()
    {
        return _burstInput;
    }


    //Debugging


}
