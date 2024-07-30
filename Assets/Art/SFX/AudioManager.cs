using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioSource audioSource;

    [Header("TitleScreen")]
    [SerializeField] AudioClip clickSound;
    [SerializeField] AudioClip titleScreenSoundtrack;

    [Header("Player")]
    [SerializeField] AudioClip footStep;
    [SerializeField] AudioClip footStepRunning;
    [SerializeField] AudioClip semiShot;
    [SerializeField] AudioClip autoShot;
    [SerializeField] AudioClip nonAutoShot;
    [SerializeField] AudioClip reloadSound;
    [SerializeField] AudioClip impactSound;

    [Header("Vehicle")]
    [SerializeField] AudioClip engineStart;
    [SerializeField] AudioClip drivingST;

    [Header("Soundtracks")]
    [SerializeField] AudioClip ambient1;
    [SerializeField] AudioClip ambient2;
    [SerializeField] AudioClip combatST;

    private Coroutine ambientCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            PlayTitleScreenSoundtrack();
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 0)
        {
            PlayTitleScreenSoundtrack();
        }
        else
        {
            StopSound();
            if (scene.buildIndex == 3)
            {
                StartAmbientSoundLoop();
            }
            else
            {
                StopAmbientSoundLoop();
            }
        }
    }

    private void StartAmbientSoundLoop()
    {
        if (ambientCoroutine == null)
        {
            ambientCoroutine = StartCoroutine(PlayAmbientSoundLoop());
        }
    }

    private void StopAmbientSoundLoop()
    {
        if (ambientCoroutine != null)
        {
            StopCoroutine(ambientCoroutine);
            ambientCoroutine = null;
        }
    }

    private IEnumerator PlayAmbientSoundLoop()
    {
        while (true)
        {
            PlayLoopedSound(ambient1);
            yield return new WaitForSeconds(ambient1.length);

            PlayLoopedSound(ambient2);
            yield return new WaitForSeconds(ambient2.length);
        }
    }

    // TitleScreen Sounds
    public void PlayClickSound()
    {
        PlaySound(clickSound);
    }

    public void PlayTitleScreenSoundtrack()
    {
        PlayLoopedSound(titleScreenSoundtrack);
    }

    // Player Sounds
    public void PlayFootStep()
    {
        PlaySound(footStep);
    }

    public void PlayFootStepRunning()
    {
        PlaySound(footStepRunning);
    }

    public void PlaySemiShot()
    {
        PlaySound(semiShot);
    }

    public void PlayAutoShot()
    {
        PlaySound(autoShot);
    }

    public void PlayNonAutoShot()
    {
        PlaySound(nonAutoShot);
    }

    public void PlayReloadSound()
    {
        PlaySound(reloadSound);
    }

    public void PlayImpactSound()
    {
        PlaySound(impactSound);
    }

    // Vehicle Sounds
    public void PlayEngineStart()
    {
        PlaySound(engineStart);
    }

    public void PlayDrivingSound()
    {
        PlayLoopedSound(drivingST);
    }

    // Soundtracks
    public void PlayAmbient1()
    {
        PlayLoopedSound(ambient1);
    }

    public void PlayAmbient2()
    {
        PlayLoopedSound(ambient2);
    }

    public void PlayCombatSoundtrack()
    {
        PlayLoopedSound(combatST);
    }

    // Helper Methods
    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void PlayLoopedSound(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    public void StopSound()
    {
        audioSource.Stop();
        audioSource.loop = false;
    }
}
