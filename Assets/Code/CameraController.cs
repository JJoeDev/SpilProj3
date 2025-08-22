using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    InputManager m_iManager;

    [Header("Orbit settings")]
    [SerializeField] private float m_cameraSensitivity = 3.0f;
    [SerializeField] private float m_orbitDamping = 10.0f;
    [SerializeField] private float m_minPitch = -20.0f;
    [SerializeField] private float m_maxPitch = 60.0f;

    private float m_pitch = 10.0f;
    private float m_yaw = 0.0f;

    private void Start()
    {
        m_iManager = InputManager.Instance;

        Vector3 angles = transform.localEulerAngles;
        m_yaw = angles.y;
        m_pitch = angles.x;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        Vector2 input = m_iManager.OnLook();

        m_yaw += input.x * m_cameraSensitivity;
        m_pitch -= input.y * m_cameraSensitivity;
        m_pitch = Mathf.Clamp(m_pitch,m_minPitch,m_maxPitch);

        Quaternion targetRotation = Quaternion.Euler(m_pitch, m_yaw, 0.0f);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, m_orbitDamping * Time.deltaTime);
    }
}
