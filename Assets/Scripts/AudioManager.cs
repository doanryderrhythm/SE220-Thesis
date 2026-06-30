using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    [SerializeField] AudioSource SFXSource;
    [SerializeField] AudioSource SFXLoopSource;

    [Header("Prefabs")]
    [SerializeField] AudioSource SFXPrefab;

    [Header("SFX")]
    public AudioClip platformerPlayerLandSound;
    public AudioClip platformerPlayerJumpSound;
    public AudioClip platformerPlayerDamageSound;
    public AudioClip platformerPlayerDeadSound;
    public AudioClip platformerPlayerShootSound;

    [Header("Loop SFX")]
    public AudioClip platformerPlayerMoveSound;

    public void PlaySFX(AudioClip sfx)
    {
        SFXSource.PlayOneShot(sfx);
    }

    public void PlayLoopSFX(AudioClip sfx, float pitchOffset = 0f)
    {
        if (SFXLoopSource.clip != null && SFXLoopSource.clip == sfx)
            return;

        SFXLoopSource.clip = sfx;
        SFXLoopSource.loop = true;
        SFXLoopSource.pitch = Random.Range(1f, 1f + pitchOffset);
        SFXLoopSource.Play();
    }

    public void StopLoopSFX()
    {
        if (SFXLoopSource.clip != null)
        {
            SFXLoopSource.Stop();
            SFXLoopSource.clip = null;
        }
    }

    public void InstantiateSFX(AudioClip clip, float pitchOffset = 0f)
    {
        GameObject audioGO = Instantiate(SFXPrefab.gameObject);
        AudioSource audioSource = audioGO.GetComponent<AudioSource>();
        if (audioSource == null)
            return;

        audioSource.clip = clip;
        audioSource.Play();
        audioSource.pitch = Random.Range(1f, 1f + pitchOffset);
        Destroy(audioGO, audioSource.clip.length);
    }
}
