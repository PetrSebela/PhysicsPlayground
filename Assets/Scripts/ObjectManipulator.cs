using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManipulator : MonoBehaviour
{
    Rigidbody grabbed;
    [SerializeField] LayerMask canManipulate;
    [SerializeField] Transform howerTarget;

    public bool TryGrab()
    {
        if (!Physics.Raycast(this.transform.position, this.transform.forward, out RaycastHit hit, 10f))
            return false;

            
        if(!hit.transform.tag.Contains("Manipulatable"))
            return false;

        grabbed = hit.transform.gameObject.GetComponent<Rigidbody>();       

        if (!grabbed)
            return false;

        grabbed.useGravity = false;
        return true;
    }

    public void Drop()
    {
        if(!grabbed)
            return;

        grabbed.useGravity = true;
        grabbed = null;
    }

    void FixedUpdate()
    {
        if(!grabbed)
            return;

        Vector3 howerVector = howerTarget.transform.position - grabbed.transform.position;
        
        if (howerVector.magnitude < 0.025f)
        {
            grabbed.velocity = Vector3.zero;
            return;
        }

        grabbed.velocity = howerVector * 6.5f;
    }
}
