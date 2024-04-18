using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowBlob : MonoBehaviour
{
    public Vector3 throwDirection = Vector3.up; 
    public float throwForce = 10f;
    public Rigidbody rb;

    // Start is called before the first frame update  public float throwForce = 10f;
private void OnTriggerExit(Collider other)
    {
    ThrowObject(rb,throwDirection);
    }
       public void ThrowObject(Rigidbody rb, Vector3 throwDirection)
    {
        rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
    }
}

