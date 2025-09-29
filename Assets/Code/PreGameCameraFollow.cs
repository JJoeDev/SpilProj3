using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreGameCameraFollow : MonoBehaviour
{
    [SerializeField] GameObject[] m_cameraFollowPoints;
    [SerializeField] GameObject m_car;
    [SerializeField] float m_cameraMoveSpeed = 10f;

    Transform m_targetPos;
    int m_cameraFollowIndex = 0;

    bool m_reachedEnd = false;

    private void Awake()
    {
        m_targetPos = m_cameraFollowPoints[0].transform;
    }

    private void Update()
    {
        if (m_cameraFollowIndex >= m_cameraFollowPoints.Length) m_reachedEnd = true;
        if (m_cameraFollowIndex <= 0) m_reachedEnd = false;

        if ((transform.position - m_cameraFollowPoints[m_cameraFollowIndex].transform.position).magnitude < 0.01f)
        {
            if (m_reachedEnd) m_cameraFollowIndex--;
            else if (!m_reachedEnd) m_cameraFollowIndex++;
        }

        transform.position = Vector3.MoveTowards(transform.position, m_cameraFollowPoints[m_cameraFollowIndex].transform.position, m_cameraMoveSpeed * Time.deltaTime);
        transform.LookAt(m_car.transform); 
    }
}
