using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CamFocusController : MonoBehaviour
{
    //Declarations
    [SerializeField] private Transform _target;
    [SerializeField] private CinemachineVirtualCamera _vCam;
    private float _originalDampeningX;
    private float _originalDampeningY;
    private float _originalDampeningZ;
    [SerializeField] private bool _isCameraShaking = false;
    private float _magnitude;
    [SerializeField] private bool _isDebugEnabled = false;
    [SerializeField] private bool _shakeCameraCMD = false;
    [SerializeField] private float _debugShakeDuration;
    [SerializeField] private float _debugShakeMagnitude;


    //Monos
    private void Start()
    {
        _originalDampeningX = _vCam.GetCinemachineComponent<CinemachineTransposer>().m_XDamping;
        _originalDampeningY = _vCam.GetCinemachineComponent<CinemachineTransposer>().m_YDamping;
        _originalDampeningZ = _vCam.GetCinemachineComponent<CinemachineTransposer>().m_ZDamping;
    }

    private void Update()
    {
        ListenForDebugCommands();
        followTargetXZ();
        ShakeCamera();
    }


    //Internal Utils
    private void followTargetXZ()
    {
        if (_target != null)
        {
            //Apply the new position to self's transform
            transform.position = _target.transform.position;
        }
    }

    private void ShakeCamera()
    {
        if (_isCameraShaking)
        {
            //generate a random displacement vector
            float randomX = Random.Range(-_magnitude, _magnitude);
            float randomY = Random.Range(-_magnitude, _magnitude);
            float randomZ = Random.Range(-_magnitude, _magnitude);
            Vector3 cameraDisplacement = new Vector3(randomX, randomY, randomZ);

            //turn off camera smoothing


            //offset the camera's position by the displacement vector
            transform.position = _target.position + cameraDisplacement;
        }
    }

    private void EndShake()
    {
        //Exit the shake state
        _isCameraShaking = false;

        //Refocus on the target
        transform.position = _target.position;

        //Reset the original camera dampening
        _vCam.GetCinemachineComponent<CinemachineTransposer>().m_XDamping = _originalDampeningX;
        _vCam.GetCinemachineComponent<CinemachineTransposer>().m_YDamping = _originalDampeningY;
        _vCam.GetCinemachineComponent<CinemachineTransposer>().m_ZDamping = _originalDampeningZ;
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

    public void TriggerCameraShake(float duration, float magnitude)
    {
        if (!_isCameraShaking)
        {
            //Set new state 
            _isCameraShaking = true;

            //Remove the dampening to make the camera jerky
            _vCam.GetCinemachineComponent<CinemachineTransposer>().m_XDamping = 0;
            _vCam.GetCinemachineComponent<CinemachineTransposer>().m_YDamping = 0;
            _vCam.GetCinemachineComponent<CinemachineTransposer>().m_ZDamping = 0;

            //Save and bound the shake range
            _magnitude = Mathf.Max(0,magnitude);

            //Start the end shake countdown
            Invoke("EndShake", Mathf.Max(0,duration));
        }
    }


    //Debug
    private void ListenForDebugCommands()
    {
        if (_isDebugEnabled)
        {
            if (_shakeCameraCMD)
            {
                _shakeCameraCMD = false;
                TriggerCameraShake(_debugShakeDuration, _debugShakeMagnitude);
            }
        }
    }

}
