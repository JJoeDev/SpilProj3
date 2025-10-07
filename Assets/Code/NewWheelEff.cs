using UnityEngine;

public class NewWheelEff : MonoBehaviour
{
    [System.Serializable]  // <-- makes this class show up in Inspector
    public class WheelEffect
    {
        public WheelCollider wheelCollider;     // Drag your WheelCollider here
        public Transform fxSpawnPoint;          // Empty GameObject at wheel position
        public ParticleSystem dustPrefab;       // Dust prefab reference

        [HideInInspector] 
        public ParticleSystem dustInstance;     // Runtime instance
    }

    public WheelEffect[] wheels;  // <-- shows up as array in Inspector

    public float slipThreshold = 0.3f;
    public float minSpeed = 2f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Spawn all particle systems
        foreach (var w in wheels)
        {
            if (w.dustPrefab != null && w.fxSpawnPoint != null)
            {
                w.dustInstance = Instantiate(
                    w.dustPrefab,
                    w.fxSpawnPoint.position,
                    w.fxSpawnPoint.rotation,
                    w.fxSpawnPoint
                );
                w.dustInstance.Stop();
            }
        }
    }

    void Update()
    {
        foreach (var w in wheels)
        {
            if (w.wheelCollider == null) continue;

            WheelHit hit;
            if (w.wheelCollider.GetGroundHit(out hit))
            {
                float slip = Mathf.Max(Mathf.Abs(hit.forwardSlip), Mathf.Abs(hit.sidewaysSlip));
                bool shouldPlay = rb.velocity.magnitude > minSpeed && slip > slipThreshold;

                if (shouldPlay && !w.dustInstance.isPlaying) w.dustInstance.Play();
                else if (!shouldPlay && w.dustInstance.isPlaying) w.dustInstance.Stop();
            }
            else
            {
                if (w.dustInstance != null && w.dustInstance.isPlaying)
                    w.dustInstance.Stop();
            }
        }
    }
}
