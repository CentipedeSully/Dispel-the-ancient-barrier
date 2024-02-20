using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFocusController : MonoBehaviour
{
    //Declarations
    [SerializeField] private Transform _target;



    //Monos
    private void Update()
    {
        followTargetXZ();
    }


    //Internal Utils
    private void followTargetXZ()
    {
        if (_target != null)
        {
            //Create get the target's new position
            Vector3 updatedPosition = new Vector3(_target.position.x,transform.position.y, _target.position.z);

            //Apply the new position to self's transform
            transform.position = updatedPosition;
        }
    }



    //External Utils
    public void SetNewTarget(Transform newTarget)
    {
        if (newTarget != null)
            _target = newTarget;
    }

    public void ClearTarget()
    {
        _target = null;
    }



}
