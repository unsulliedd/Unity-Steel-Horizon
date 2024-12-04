using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioSource audioSource;
    public AudioSource environmentAudioSource;

    public Button[] buttons;

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;
    public AudioMixerGroup menuMusicGroup;
    public AudioMixerGroup ambientGroup;
    public AudioMixerGroup gunfireGroup;
    public AudioMixerGroup characterMovementGroup;

    [Header("TitleScreen")]
    [SerializeField] AudioClip clickSound;
    [SerializeField] AudioClip titleScreenSoundtrack;

    [Header("Player")]
    public AudioClip footStep;
    public AudioClip footStepRunning;
    public AudioClip semiShot;
    public AudioClip autoShot;
    public AudioClip nonAutoShot;
    public AudioClip reloadSound;
    public AudioClip impactSound;
    public AudioClip actionSound;

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

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            PlayTitleScreenSoundtrack();
            foreach (Button button in buttons)
            {
                button.onClick.AddListener(() => PlayClickSound());
            }
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 0)
            PlayTitleScreenSoundtrack();
        else if (scene.buildIndex == SaveGameManager.Instance.GetWorldSceneIndex())
            StartAmbientSoundLoop();
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
            PlayLoopedSound(ambient1, ambientGroup);
            yield return new WaitForSeconds(ambient1.length);

            PlayLoopedSound(ambient2, ambientGroup);
            yield return new WaitForSeconds(ambient2.length);
        }
    }

    // TitleScreen Sounds
    public void PlayClickSound()
    {
        PlaySound(clickSound, menuMusicGroup);
    }

    public void PlayTitleScreenSoundtrack()
    {
        PlayLoopedSound(titleScreenSoundtrack, menuMusicGroup);
    }

    // Player Sounds
    public void PlayFootStep()
    {
        PlaySound(footStep, characterMovementGroup);
    }

    public void PlayFootStepRunning()
    {
        PlaySound(footStepRunning, characterMovementGroup);
    }

    public void PlaySemiShot()
    {
        PlayEnvSound(semiShot, gunfireGroup);
    }

    public void PlayAutoShot()
    {
        PlayEnvSound(autoShot, gunfireGroup);
    }

    public void PlayNonAutoShot()
    {
        PlayEnvSound(nonAutoShot, gunfireGroup);
    }

    public void PlayReloadSound()
    {
        PlayEnvSound(reloadSound, gunfireGroup);
    }

    public void PlayImpactSound()
    {
        PlayEnvSound(impactSound, gunfireGroup);
    }

    // Vehicle Sounds
    public void PlayEngineStart()
    {
        PlaySound(engineStart, ambientGroup);
    }

    public void PlayDrivingSound()
    {
        PlayLoopedSound(drivingST, ambientGroup);
    }

    // Soundtracks
    public void PlayAmbient1()
    {
        PlayLoopedSound(ambient1, ambientGroup);
    }

    public void PlayAmbient2()
    {
        PlayLoopedSound(ambient2, ambientGroup);
    }

    public void PlayCombatSoundtrack()
    {
        PlayLoopedSound(combatST, ambientGroup);
    }

    // Helper Methods
    private void PlaySound(AudioClip clip, AudioMixerGroup mixerGroup)
    {
        if (clip != null)
        {
            audioSource.outputAudioMixerGroup = mixerGroup;
            audioSource.PlayOneShot(clip);
        }
    }

    private void PlayEnvSound(AudioClip clip, AudioMixerGroup mixerGroup)
    {
        if (clip != null)
        {
            environmentAudioSource.outputAudioMixerGroup = mixerGroup;
            environmentAudioSource.PlayOneShot(clip);
        }
    }

    private void PlayLoopedSound(AudioClip clip, AudioMixerGroup mixerGroup)
    {
        if (clip != null)
        {
            audioSource.outputAudioMixerGroup = mixerGroup;
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
