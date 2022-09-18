using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SoundManager : MonoBehaviour
{
	// Audio players components.
	public AudioSource EffectsSource;
	public AudioSource MusicSource;
	// Random pitch adjustment range.
	public float LowPitchRange = .95f;
	public float HighPitchRange = 1.05f;
	// Singleton instance.
	public static SoundManager Instance = null;
	public AudioClip MenuMusic;
	public AudioClip AlternativeMenuMusic;
	public AudioClip Music;
	public AudioClip AlternativeMusic;
	private int random;
	// Initialize the singleton instance.
	private void Awake()
	{
		// If there is not already an instance of SoundManager, set it to this.
		if (Instance == null)
		{
			Instance = this;
		}
		//If an instance already exists, destroy whatever this object is to enforce the singleton.
		else if (Instance != this)
		{
			Destroy(gameObject);
		}
		//Set SoundManager to DontDestroyOnLoad so that it won't be destroyed when reloading our scene.
		random = Random.Range(0, 99);
		if (random <= 94)
		{
			PlayMusic(MenuMusic);
		}
		else
        {
			PlayMusic(AlternativeMenuMusic);
        }
		Object.DontDestroyOnLoad(gameObject);
	}
    private void OnEnable()
    {
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
		if (scene.name == "SampleScene")
        {
			if (random <= 94)
			{
				PlayMusic(Music);
			}
			else
			{
				PlayMusic(AlternativeMusic);
			}
		}
    }
 
	// Play a single clip through the sound effects source.
	public void Play(AudioClip clip)
	{
		EffectsSource.clip = clip;
		EffectsSource.Play();
	}
	// Play a single clip through the music source.
	public void PlayMusic(AudioClip clip)
	{
		MusicSource.clip = clip;
		MusicSource.Play();
	}
	// Play a random clip from an array, and randomize the pitch slightly.
	public void RandomSoundEffect(params AudioClip[] clips)
	{
		int randomIndex = Random.Range(0, clips.Length);
		float randomPitch = Random.Range(LowPitchRange, HighPitchRange);
		EffectsSource.pitch = randomPitch;
		EffectsSource.clip = clips[randomIndex];
		EffectsSource.Play();
	}

}

