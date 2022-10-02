using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour{
    //~ inspector (private)
    [Header("Audio sources")]
    [SerializeField][Tooltip("The audio source for effects")]                             private AudioSource EffectsSource;
    [SerializeField][Tooltip("The audio source for music")]                               private AudioSource MusicSource;
    [Header("Pitch limits")]
    [SerializeField][Min(0f)][Tooltip("The lower limit for random pitch for random sfx")] private float LowPitchRange = 0.95f;
    [SerializeField][Min(0f)][Tooltip("The upper limit for random pitch for random sfx")] private float HighPitchRange = 1.05f;
    [Header("Audio clips")]
    [SerializeField][Tooltip("The main menu music audio file")]                           private AudioClip MenuMusic;
    [SerializeField][Tooltip("The secret menu music audio file")]                         private AudioClip AlternativeMenuMusic;
    [SerializeField][Tooltip("The in-game music audio file")]                             private AudioClip Music;
    [SerializeField][Tooltip("The secret in-game music audio file")]                      private AudioClip AlternativeMusic;
    //~ private
    private static SoundManager Instance = null;
    private static float random;

    private void Awake(){
        //~ Destroy if a sound manager instance already exists
        if(SoundManager.Instance != null){
            Destroy(this.gameObject);
            return;
        }
        //~ Instantiate singleton instance as this instance
        SoundManager.Instance = this;
        SoundManager.random = Random.value;
        SceneManager.sceneLoaded += this.OnSceneLoaded;
        //~ Set SoundManager to DontDestroyOnLoad so that it won't be destroyed when reloading our scene
        Object.DontDestroyOnLoad(this.gameObject);
        //~ Play menu music (do first scene load manually)
        this.OnSceneLoaded(SceneManager.GetActiveScene(),LoadSceneMode.Single);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode){
        //~ set music dependend on scene loaded
        // TODO change scene name ~ only in-game or main menu as scenes ?
        if(scene.name == "SampleScene"){
            if(SoundManager.random <= 0.94f) this.PlayMusic(Music);
            else this.PlayMusic(AlternativeMusic);
        }else if(scene.name == "MainMenu"){
            if(SoundManager.random <= 0.94f) this.PlayMusic(MenuMusic);
            else this.PlayMusic(AlternativeMenuMusic);
        }
    }

    /// <summary> Play a single clip through the sound effects source. </summary>
    public void Play(AudioClip clip){
        EffectsSource.clip = clip;
        EffectsSource.Play();
    }
    /// <summary> Play a single clip through the music source. </summary>
    public void PlayMusic(AudioClip clip){
        MusicSource.clip = clip;
        MusicSource.Play();
    }
    /// <summary> Play a random clip from an array, and randomize the pitch slightly. </summary>
    public void RandomSoundEffect(params AudioClip[] clips){
        int randomIndex = Random.Range(0, clips.Length);
        float randomPitch = Random.Range(LowPitchRange, HighPitchRange);
        EffectsSource.pitch = randomPitch;
        EffectsSource.clip = clips[randomIndex];
        EffectsSource.Play();
    }
}
