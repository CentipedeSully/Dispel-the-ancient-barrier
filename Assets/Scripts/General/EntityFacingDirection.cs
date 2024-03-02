using UnityEngine;

public class EntityFacingDirection : MonoBehaviour
{
    public float updateInterval = 0.1f;
    
    public float DotX { get; private set; } // -1 is left, 1 is right
    public float DotY { get; private set; } // -1 is down, 1 is up
    
    private Transform _cameraTransform;
    
    private void Awake() => _cameraTransform = Camera.main.transform;
    private void OnEnable() => InvokeRepeating(nameof(SetDirections), 0, updateInterval);
    private void OnDisable() => CancelInvoke();

    private void SetDirections()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        Vector3 other = (_cameraTransform.position - transform.position).normalized;
        
        DotX = Vector3.Dot(right, other);
        DotY = Vector3.Dot(forward, other);
    }
    
    
    

}