using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreGameCamera : MonoBehaviour
{
    [SerializeField] GameObject[] m_cameraFollowPoints;
    [SerializeField] GameObject m_car;
    [SerializeField] float cameraMoveSpeed = 10f;

    Transform targetPos;
    int cameraFollowIndex = 0;

    private void Awake()
    {
        targetPos = m_cameraFollowPoints[0].transform;
    }

    private void Update()
    {
        transform.LookAt(m_car.transform.position);

        if ((transform.position - m_cameraFollowPoints[cameraFollowIndex].transform.position).magnitude < 0.01f)
        {
            if (cameraFollowIndex >= m_cameraFollowPoints.Length - 1) cameraFollowIndex = 0;
            if (cameraFollowIndex < m_cameraFollowPoints.Length) cameraFollowIndex++;
        }



        transform.position = Vector3.MoveTowards(transform.position, m_cameraFollowPoints[cameraFollowIndex].transform.position, cameraMoveSpeed * Time.deltaTime);
    }
}
