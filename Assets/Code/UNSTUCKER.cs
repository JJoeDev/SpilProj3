using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UNSTUCKER : MonoBehaviour
{
    [SerializeField] Rigidbody m_rb;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2))
        {
            m_rb.velocity += (transform.up + new Vector3(Random.Range(0,1), Random.Range(0, 1), Random.Range(0, 1))) * 10;
            m_rb.angularVelocity += new Vector3(Random.Range(0, 1), Random.Range(0, 1), Random.Range(0, 1)) * 100;
            Debug.Log("Unstucker Active!");
        }
    }
}
