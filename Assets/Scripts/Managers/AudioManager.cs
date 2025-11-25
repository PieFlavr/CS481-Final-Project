using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    #region Fields and Properties
    public static AudioManager Instance { get; private set; } // Singleton instance

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource; // Theoretically only need one BGM source
    [SerializeField] private AudioSource sfxSourcePrefab;   // Prefab'd b/c using for pooling + cool debug stuff
                                                            // NOTE (L): We're using 3D audio sources for SFX
                                                            // This is just because I felt it'd be easier for the creepy vibes...
                                                            // ...since apparently it automatically handles audio panning based on position. 
                                                            // Also let spatial blend be adjustable per SFX, default 1. -L
    
    [Header("Settinsg")]
    [Range(0f, 1f)] [SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float bgmVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;
    
    [SerializeField] private int sfxPoolSize = 30;

    [Header("Audio Assets")]    // All audio are stored as SOs -- just easier to manage + work together -L
    public SFXData[] sfxAssets; // See AudioData.cs for the actual SO definitions!
    public BGMData[] bgmAssets;

    private Dictionary<string, SFXData> sfxLibrary;
    private Dictionary<string, BGMData> bgmLibrary;
    private List<AudioSource> sfxPool;  // Pool of AudioSources for SFX playback
                                        // Apparently its better to cycle through a pool than create/destroy on the fly -L
    private int sfxPoolIndex = 0;
    //private Coroutine fadeCoroutine; 
    //TODO: Implement music fading later -L

    #endregion Fields and Properties



    #region Unity Methods

    private void Awake()
    {
        Debug.Log("[AudioManager] Hello World!");
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[AudioManager] Duplicate instance detected -- annihilating it now!");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeSources();
        UpdateVolumes();
    }

    #endregion Unity Methods



    #region Audio Methods
    
    /// <summary>
    /// Plays a sound effect (SFX) by its ID at an optional position.
    /// </summary>
    /// <param name="id">The ID of the SFX to play.</param>
    /// <param name="position">Optional position to play the SFX at. If null, plays at the origin.</param>
    /// <returns>The AudioSource used to play the SFX, or null if the SFX ID was not found.</returns>
    public AudioSource PlaySFX(string id, Vector2? position = null)
    {
        // Lookup SFX data by ID
        if(!sfxLibrary.TryGetValue(id, out SFXData sfxData))
        {
            Debug.LogWarning("[AudioManager] No SFX found with ID: " + id);
            return null;
        }
        
        // Get next available AudioSource from pool
        AudioSource sfxSource = GetNextSFXSource();

        // Configure AudioSource
        sfxSource.clip = sfxData.clip;
    
        if (position.HasValue)
        {
            sfxSource.transform.position = new Vector3(position.Value.x, position.Value.y, 0f);
        }
        else
        {
            sfxSource.transform.position = Vector3.zero;
        }
        sfxSource.pitch = 1f + Random.Range(-sfxData.pitchVariance, sfxData.pitchVariance);     // Pitch variance so SFX don't sound too repetitive + testing out SO; feel free to remove/tweak -L
        sfxSource.volume = masterVolume * sfxVolume * sfxData.volume;   // Apply master and SFX volume settings
        sfxSource.Play();

        return sfxSource;
    }

    /// <summary>
    /// Plays background music (BGM) by its ID, with an option to loop.
    /// </summary>
    /// <param name="id">The ID of the BGM to play.</param>
    /// <param name="loop">Whether the BGM should loop. Default is true.</param>
    public void PlayBGM(string id, bool loop = true)
    {
        if(!bgmLibrary.TryGetValue(id, out BGMData bgmData))
        {
            Debug.LogWarning("[AudioManager] No BGM found with ID: " + id);
            return;
        }

        if(bgmSource.clip == bgmData.clip && bgmSource.isPlaying)
        {
            Debug.Log("[AudioManager] BGM already playing: " + id);
            return;
        }

        //TODO: Add fade out/in later here -L

        bgmSource.clip = bgmData.clip;
        bgmSource.loop = loop;
        bgmSource.volume = masterVolume * bgmVolume * bgmData.volume; 
        bgmSource.Play();
    }

    /// <summary>
    /// Stops the currently playing background music (BGM).
    /// </summary>
    public void StopBGM()
    {
        if (bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }
    }

    /// <summary>
    /// Mutes or unmutes all audio sources managed by the AudioManager.
    /// </summary>
    /// <param name="muted">If true, mutes all audio; if false, unmutes all audio.</param>
    public void MuteAll(bool muted)
    {
        bgmSource.mute = muted;
        foreach (AudioSource sfxSource in sfxPool)
        {
            sfxSource.mute = muted;
        }
    }

    #endregion Audio Methods



    #region Settings Methods

    /// <summary>
    /// Sets the master, BGM, and SFX volume levels.
    /// TODO: Persist settings to player prefs later -L
    /// </summary>
    /// <param name="master">Master volume level (0.0 to 1.0).</param>
    /// <param name="bgm">BGM volume level (0.0 to 1.0).</param>
    /// <param name="sfx">SFX volume level (0.0 to 1.0).</param>
    public void SetVolume(float master, float bgm, float sfx)
    {
        masterVolume = Mathf.Clamp01(master);
        bgmVolume = Mathf.Clamp01(bgm);
        sfxVolume = Mathf.Clamp01(sfx);

        //TODO: Save settings to player prefs later -L

        UpdateVolumes();
    }

    /// <summary>
    /// Loads volume settings from persistent storage.
    /// TODO: Implement loading from player prefs later -L
    /// </summary>
    public void LoadVolumeSettings()
    {
        //TODO: Load settings from player prefs later -L
        UpdateVolumes();
    }

    #endregion Settings Methods



    #region Private Utils

    /// <summary>
    /// Gets the next available AudioSource from the SFX pool, cycling through the pool.
    /// </summary>
    /// <returns>The next AudioSource to use for playing an SFX.</returns>
    private AudioSource GetNextSFXSource()
    {
        // Get next AudioSource from pool
        AudioSource sfxSource = sfxPool[sfxPoolIndex];

        // Try to find a free AudioSource if the current one is playing
        if(sfxSource.isPlaying)
        {
            Debug.LogWarning("[AudioManager] SFX AudioSource pool exhausted! Consider increasing pool size.");
            AudioSource freeSource = sfxPool.Find(source => !source.isPlaying);
            if(freeSource != null)
            {
                sfxSource = freeSource;
                // If no free source found, will just override the oldest one
            }
        }

        // Update pool index for next call
        sfxPoolIndex = (sfxPoolIndex + 1) % sfxPoolSize;

        return sfxSource;
    }
    
    /// <summary>
    /// Updates the volumes of all audio sources based on the current master, BGM, and SFX volume settings.
    /// At the moment, only the BGM source volume is updated here.
    /// </summary>
    private void UpdateVolumes()
    {
        if(bgmSource != null)
        {
            bgmSource.volume = masterVolume * bgmVolume;
        }

        // NOTE (L): In case of SFX volume issues...
        // Here just as a emergency fallback if SFX volume handling explodes 
        // TODO: Remove when fully tested SFX volume handling -L
        // foreach (AudioSource sfxSource in sfxPool)
        // {
        //     sfxSource.volume = masterVolume * sfxVolume;
        // }
    }

    private void InitializeSources()
    {
        // Building lookup dicts for quick access
        sfxLibrary = new Dictionary<string, SFXData>();
        bgmLibrary = new Dictionary<string, BGMData>();

        foreach (SFXData sfx in sfxAssets)
        {
            sfxLibrary[sfx.id] = sfx;
        } 
        foreach (BGMData bgm in bgmAssets)
        {
            bgmLibrary[bgm.id] = bgm;
        }

        // Initializing SFX AudioSource pool
        sfxPool = new List<AudioSource>();
        for (int i = 0; i < sfxPoolSize; i++)
        {
            AudioSource sfxSource = Instantiate(sfxSourcePrefab, transform);
            sfxSource.playOnAwake = false; // Since we're pooling, this is off
            sfxPool.Add(sfxSource);
        }
    }
    #endregion Private Utils
}