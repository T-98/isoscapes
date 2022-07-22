using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private float speed = 10.0f, scale = 0.1f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.W))
            this.transform.position+= new Vector3(1, 0, 1) * speed * Time.deltaTime;
        if(Input.GetKey(KeyCode.S))
            this.transform.position-= new Vector3(1, 0, 1) * speed * Time.deltaTime;
        if(Input.GetKey(KeyCode.A))
            this.transform.position += new Vector3(-1, 0, 1) * speed * Time.deltaTime;
        if(Input.GetKey(KeyCode.D))
            this.transform.position += new Vector3(1, 0, -1) * speed * Time.deltaTime;
        zoom();
    }

    void zoom()
    {
       this.GetComponent<Camera>().orthographicSize -= Input.mouseScrollDelta.y* scale;
       if(this.GetComponent<Camera>().orthographicSize > 15.0) this.GetComponent<Camera>().orthographicSize = 15.0f;
       if (this.GetComponent<Camera>().orthographicSize < 4) this.GetComponent<Camera>().orthographicSize = 4.0f;
    }
}
