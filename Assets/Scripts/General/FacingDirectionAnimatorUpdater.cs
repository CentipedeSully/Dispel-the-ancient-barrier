using UnityEngine;

public class FacingDirectionAnimatorUpdater : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private EntityFacingDirection facingDirection;
    
    [Header("Settings")]
    [SerializeField] private string xFloatParameter;
    [SerializeField] private string yFloatParameter;
    [SerializeField] private float updateInterval = 0.1f;
    
    private void OnEnable() => InvokeRepeating(nameof(UpdateAnimator), 0, updateInterval);
    private void OnDisable() => CancelInvoke();
    private void UpdateAnimator()
    {
        animator.SetFloat(xFloatParameter, facingDirection.DotX);
        animator.SetFloat(yFloatParameter, facingDirection.DotY); 
    }
}