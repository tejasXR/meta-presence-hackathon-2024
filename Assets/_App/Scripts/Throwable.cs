using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Throwabble : MonoBehaviour
{
    private Vector3 throwDirection = Vector3.up; 
    private float throwForce = 100f;
    public Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       
    }
    private void OnTriggerExit(Collider other){
        //float triggerRight = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch);

       // if(other.gameObject.tag == "hand" && triggerRight > 0.5f){
            //pickedUp = true;
            Debug.Log("Picked up");
            ThrowObject(rb,throwDirection);
        //}
    }
     public void ThrowObject(Rigidbody rb, Vector3 throwDirection)
    {
        rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
    }
}
