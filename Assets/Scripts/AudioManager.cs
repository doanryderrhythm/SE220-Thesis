using UnityEngine;

public enum BGMType
{
    MainMenu,
    Normal,
    Terrain,
}
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
    [SerializeField] AudioSource SFX3DPrefab;

    [Header("SFX")]
    public AudioClip platformerPlayerLandSound;
    public AudioClip platformerPlayerJumpSound;
    public AudioClip platformerPlayerDamageSound;
    public AudioClip platformerPlayerDeadSound;
    public AudioClip platformerPlayerShootSound;
    [Space(10.0f)]
    public AudioClip buildTowerSound;
    public AudioClip destroyTowerSound;
    [Space(10.0f)]
    public AudioClip enemyHitSound;
    public AudioClip enemyDeadSound;

    [Header("Loop SFX")]
    public AudioClip platformerPlayerMoveSound;

    [Header("BGM")]
    public AudioClip icyBGM;
    public AudioClip normalBGM; 
    public AudioClip terrainBGM; 
    
    [SerializeField] private AudioSource BGMSource;

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
        if (clip == null)
            return;

        GameObject audioGO = Instantiate(SFXPrefab.gameObject);
        AudioSource audioSource = audioGO.GetComponent<AudioSource>();
        if (audioSource == null)
            return;

        audioSource.clip = clip;
        audioSource.Play();
        audioSource.pitch = Random.Range(1f, 1f + pitchOffset);
        Destroy(audioGO, audioSource.clip.length);
    }

    public void InstantiateSFX3D(AudioClip clip, Vector3 pos, float pitchOffset = 0f)
    {
        if (clip == null)
            return;

        GameObject audioGO = Instantiate(SFX3DPrefab.gameObject, pos, Quaternion.identity);
        AudioSource audioSource = audioGO.GetComponent<AudioSource>();
        if (audioSource == null)
            return;

        audioSource.clip = clip;
        audioSource.Play();
        audioSource.pitch = Random.Range(1f, 1f + pitchOffset);
        Destroy(audioGO, audioSource.clip.length);
    }

    private void OnEnable()
    {
        GameEvent.OnPlayBGM += OnPlayBGM;
    }

    private void OnDisable()
    {
        GameEvent.OnPlayBGM -= OnPlayBGM;
    }
    void OnPlayBGM(BGMType type)
    {
        Debug.Log(type);
        switch (type)
        {
            case BGMType.MainMenu:
                PlayBGM(icyBGM);
                break;

            case BGMType.Normal:
                PlayBGM(normalBGM);
                break;

            case BGMType.Terrain:
                PlayBGM(terrainBGM);
                break;
        }
    }

    void PlayBGM(AudioClip clip)
    {
        if (BGMSource.clip == clip && BGMSource.isPlaying)
            return;
        BGMSource.Stop();
        BGMSource.clip = clip;
        BGMSource.loop = true;
        BGMSource.Play();
    }
}
