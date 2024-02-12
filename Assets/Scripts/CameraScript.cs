using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        float mouseX = Input.GetAxis("Mouse ScrollWheel");
        
 
        Vector3 right = transform.right * h * Time.deltaTime * 25;
        Vector3 forward = transform.forward * v * Time.deltaTime * 25;
        Vector3 z = transform.up * mouseX * Time.deltaTime * 2500;
        
 
        transform.Translate(right + forward + z);
    }
    
}
