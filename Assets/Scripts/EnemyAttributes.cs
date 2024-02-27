using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttributes : MonoBehaviour, IDamageable
{
    //Declarations
    [SerializeField] private float _health;
    [SerializeField] private float _maxHealth;
    [SerializeField] private bool _isInvincible = false;
    [SerializeField] private float _invincibleRecoveryDuration = .1f;
    [SerializeField] private bool _isStunned = false;


    //Monos




    //Internal Utils
    private void EnterStun(float stunDuration)
    {
        _isStunned = true;
        Invoke("ExitStun", Mathf.Max(0,stunDuration));
    }

    private void ExitStun()
    {
        _isStunned = false;
    }

    private void EnterInvincRecovery()
    {
        _isInvincible = true;
        Invoke("ExitInvincRecovery", _invincibleRecoveryDuration);
    }

    private void ExitInvincRecovery()
    {
        _isInvincible = false;
    }



    //External Utils
    public bool IsInInvincibilityRecovery()
    {
        return _isInvincible;
    }

    public void ModifyHealth(float value)
    {
        //set health
        _health = Mathf.Clamp(_health + value, 0, _maxHealth);

        //if health reduced then enter invinc recoveyr
        if (value < 0)
            EnterInvincRecovery();
    }

    public void SufferStun(float duration)
    {
        EnterStun(duration);
    }



    //Debugging





}
