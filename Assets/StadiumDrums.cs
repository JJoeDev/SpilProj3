using System.Collections;
using UnityEngine;

public class StadiumDrums : MonoBehaviour
{
    public AudioSource stadiumDrumSound;
    [SerializeField] private float CooldownStarter = 5f; // start delay
    [SerializeField] private float CooldownPauser; // pause delay

    public AudioBehaviour PassiveSound; // passive sound 

    private bool isPlaying = false;
    private bool isPaused = false;

    Coroutine loopCo;

    void Start()
    {

        if (stadiumDrumSound == null)
            stadiumDrumSound = GetComponent<AudioSource>();

        loopCo = StartCoroutine(nameof(LoopRoutine));
    }

    void OnEnable()
    {
        if (stadiumDrumSound == null)
            stadiumDrumSound = GetComponent<AudioSource>();

        if (stadiumDrumSound == null) return;

        stadiumDrumSound.loop = true;

        if (loopCo == null)
            loopCo = StartCoroutine(nameof(LoopRoutine));
    }

    void OnDisable()
    {
        if (loopCo != null) { StopCoroutine(loopCo); loopCo = null; }
        if (stadiumDrumSound != null) stadiumDrumSound.Stop();

        isPlaying = false;
        isPaused = false;
    }
    void OnDestroy()
    {
        OnDisable();
    }

    IEnumerator LoopRoutine()
    {
        yield return new WaitForSeconds(Mathf.Max(0f, CooldownStarter));

        stadiumDrumSound.Play();
        isPlaying = true;
        isPaused = false;


        while (true)
        {
            CooldownPauser = Random.Range(0f, 10f); // tilføjer nyt random interval hver gang. 

            if (CooldownPauser <= 0f) { yield return null; continue; }
            yield return new WaitForSeconds(CooldownPauser);

            if (stadiumDrumSound == null) yield break;

            if (!isPaused)
            {
                stadiumDrumSound.Pause();
                isPaused = true;
                isPlaying = false;
            }
            else
            {
                if (stadiumDrumSound.time > 0f) stadiumDrumSound.UnPause();
                else stadiumDrumSound.Play();

                isPaused = false;
                isPlaying = true;
            }
        }
    }
}
