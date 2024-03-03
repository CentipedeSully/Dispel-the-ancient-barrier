using Unity.Mathematics;
using UnityEngine;

// This is a bare bone implementation, and not an actual billboard, this should modify later
public class SpriteBillBoard : MonoBehaviour
{
    private void Update() => transform.rotation = quaternion.identity;
}
