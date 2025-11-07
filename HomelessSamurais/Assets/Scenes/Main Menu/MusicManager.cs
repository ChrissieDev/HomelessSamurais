using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;
    
    [Header("Music")]
    public AudioClip musicTrack;
    
    [Header("Settings")]
    public float volume = 0.5f;
    
    private AudioSource audioSource;
    
    void Awake()
    {
        // Singleton pattern persist across scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = true;
            audioSource.volume = volume;
            audioSource.playOnAwake = false;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        if (musicTrack != null)
        {
            audioSource.clip = musicTrack;
            audioSource.Play();
        }
    }
    
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }
    
    public void Pause()
    {
        if (audioSource != null) audioSource.Pause();
    }
    
    public void Resume()
    {
        if (audioSource != null) audioSource.UnPause();
    }
}