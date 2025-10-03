using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private static InputManager _instance;
    public static InputManager Instance { get { return _instance; } }

    PlayerInputs pInput;

    private void Awake() {
        if(_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        pInput = new PlayerInputs();
    }

    private void OnEnable() => pInput.Enable();
    private void OnDisable() => pInput.Disable();

    public Vector2 OnMove() => pInput.Player.Move.ReadValue<Vector2>().normalized;
    public Vector2 OnLook() => pInput.Player.Look.ReadValue<Vector2>();
    public InputAction OnHandBreak() => pInput.Player.HandBreak;
    public InputAction OnJump() => pInput.Player.Jump;
    public InputAction OnOpenUpgradeRoadmap() => pInput.Player.OpenUpgradeRoadmap;
    public InputAction OnRevealEnemies() => pInput.Player.RevealEnemies;
    public float OnRoadMapScroll() => pInput.Player.ScrollRoadMap.ReadValue<float>();
}