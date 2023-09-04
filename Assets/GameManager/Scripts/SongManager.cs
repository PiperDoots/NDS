using UnityEngine;

public class SongManager : MonoBehaviour
{
	public AudioClip[] songs; // Array to hold the three versions of the song
	private AudioSource audioSource;
	private int currentSongIndex = 0;
	private float currentSongTime = 0; // Store the playback position of the current song

	private void Start()
	{
		audioSource = GetComponent<AudioSource>();
		if (songs.Length > 0)
		{
			// Initialize the audio source with the first song
			audioSource.clip = songs[currentSongIndex];
			audioSource.time = currentSongTime; // Set the playback position
			audioSource.Play();
		}
	}

	public void SwitchSong(int i)
	{
		// Store the playback position of the current song
		currentSongTime = audioSource.time;

		// Increment the current song index and loop back to 0 if necessary
		if (i < songs.Length)
		{
			currentSongIndex = i;
		}

		// Change the audio clip and set the playback position
		audioSource.clip = songs[currentSongIndex];
		audioSource.time = currentSongTime; // Set the playback position
		audioSource.Play();
	}
}