using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeBoundaryOnDetection : MonoBehaviour
{
    //Declarations
    [SerializeField] private GameObject _targetFocus;
    [SerializeField] private LayerMask _boundaryLayerMask;
    [SerializeField] private Color _gizmoColor = Color.red;
    [SerializeField] private List<string> _detectedNames;
    [SerializeField] private List<GameObject> _detectedBoundaries;
    [SerializeField] private List<GameObject> _fadedBoundaries;
    private Dictionary<GameObject, Color> _rememberedColors;


    //Monos
    private void Awake()
    {
        _detectedBoundaries = new List<GameObject>();
        _fadedBoundaries = new List<GameObject>();
        _rememberedColors = new Dictionary<GameObject, Color>();
    }

    private void Update()
    {
        DetectBoundary();
        FadeBundaries();
        UnfadeBoundaries();
    }

    private void OnDrawGizmosSelected()
    {
        DrawDetectionRaycast();
    }


    //Internal Utils
    private void DetectBoundary()
    {
        //Clear the detected names list
        _detectedNames = new List<string>();

        //Clear detected boundaries
        _detectedBoundaries = new List<GameObject>();

        //Calculate Direction
        Vector3 castDirection = (_targetFocus.transform.position - transform.position).normalized;

        //Calculate Distance
        float distance = Vector3.Distance(transform.position, _targetFocus.transform.position);

        //Scan from the main camera to the target focus
        RaycastHit[] detections = Physics.RaycastAll(transform.position, castDirection, distance, _boundaryLayerMask);

        List<GameObject> detectedBoundaries = new List<GameObject>();

        //If a detection is captured (that isn't the target focus itself), then save it
        foreach (RaycastHit detection in detections)
        {
            if (detection.collider != null)
            {
                //Track the name for debugging purposes
                _detectedNames.Add(detection.collider.name);

                //Add this object to the list of boundaries to Fade
                _detectedBoundaries.Add(detection.collider.gameObject);
            }
        }
    }

    private void FadeBundaries()
    {
        foreach(GameObject boundary in _detectedBoundaries)
        {
            //Ignore boundaries that have despawned unexpectedly
            if (boundary == null)
                continue;

            //Only fade the objects that aren't already faded
            else if (!_fadedBoundaries.Contains(boundary))
            {
                //Get the renderer & material for this object
                Renderer boundaryRenderer = boundary.GetComponent<Renderer>();
                Color color = boundaryRenderer.material.color;

                //Don't forget this object's original color!
                _rememberedColors.Add(boundary, color);

                //Change the transparency
                color.a = .5f;

                //Set the new transparency to the object
                boundaryRenderer.material.color = color;

                //Add that boundary to the list of faded boundaries
                _fadedBoundaries.Add(boundary);
            }
        }
    }

    private void UnfadeBoundaries()
    {
        //Iterate backwards to allow removing from the list during iteration
        for (int i =  _fadedBoundaries.Count - 1; i >= 0; i--)
        {
            //Remove boundaries that have despawned unexpectedly
            if (_fadedBoundaries[i] == null)
                _fadedBoundaries.Remove(_fadedBoundaries[i]);

            //Only unfade the objects that aren't detected
            else if (!_detectedBoundaries.Contains(_fadedBoundaries[i]))
            {
                //Get the renderer from this object
                Renderer boundaryRenderer = _fadedBoundaries[i].GetComponent<Renderer>();

                //Set the color back to the remembered value
                boundaryRenderer.material.color = _rememberedColors[_fadedBoundaries[i]];

                //remove the remembered color
                _rememberedColors.Remove(_fadedBoundaries[i]);

                //Remove the boundary
                _fadedBoundaries.Remove(_fadedBoundaries[i]);
            }
        }
    }


    //External Utils



    //DEbugging
    private void DrawDetectionRaycast()
    {
        //Set gizmo color
        Gizmos.color = _gizmoColor;

        //Draw the line
        if (_targetFocus != null)
            Gizmos.DrawLine(transform.position, _targetFocus.transform.position);
    }


}
