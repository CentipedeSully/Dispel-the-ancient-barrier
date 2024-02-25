using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IDamageable
{
    public void ModifyHealth(float damage);

    public void SufferStun(float duration);

    public bool IsInInvincibilityRecovery();
}

public class DealDamageOnContact : MonoBehaviour
{
    //Declarations
    [Header("External Utils")]
    [SerializeField] private GameManager _gameManager;

    [Header("Collision-filtering utilities")]
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private List<string> _validTags;

    [Header("Damage Utilities")]
    [SerializeField][Min(0)] private float _damage;
    [SerializeField] [Min(0)] private float _stunDuration;
    [Tooltip("This is an impulse force")]
    [SerializeField] [Min(0)] private float _pushBackForce;


    //Monos
    private void Awake()
    {
        if (_validTags == null)
            _validTags = new List<string>();
    }

    private void OnCollisionStay(Collision collision)
    {
        if (IsCollisionValid(collision))
        {
            //Get object references
            GameObject touchedObject = collision.gameObject;
            IDamageable damageableScript = touchedObject.GetComponent<IDamageable>();

            //Be sure not to harm if the entity is invinsible
            if (damageableScript.IsInInvincibilityRecovery())
                return;

            Rigidbody objectRB = touchedObject.GetComponent<Rigidbody>();

            //Deal dmaage to object
            damageableScript.ModifyHealth(-_damage);

            //push object back
            if (objectRB != null)
            {
                //Calculate the direction that's towards the touched object. NORMALIZE it
                Vector3 pushDirection = (touchedObject.transform.position - transform.position).normalized;

                //Apply the push
                objectRB.AddForce(pushDirection * _pushBackForce * Time.deltaTime, ForceMode.Impulse);
            }

            //stun object
            damageableScript.SufferStun(_stunDuration);

        }
    }


    //Internal Utils
    private bool IsCollisionValid(Collision collision)
    {
        //Make sure the collision itself isn't null
        if (collision == null)
            return false;

        else if (collision.collider == null)
            return false;

        //get the game object
        GameObject objectHit = collision.collider.gameObject;

        //return true if both the tag is valid and the entity contains a script to handle the damage
        if (_validTags.Contains(objectHit.tag) && objectHit.GetComponent<IDamageable>() != null)
            return true;

        //else this object doesn't count
        else return false;
    }




    //External Utils
    public void SetLayerMask(LayerMask mask)
    {
        _layerMask = mask;
    }

    public void AddValidTag(string newTag)
    {
        if (!_validTags.Contains(newTag))
            _validTags.Add(newTag);
    }

    public bool DoesTagExist(string queryTag)
    {
        return _validTags.Contains(queryTag);
    }


}
