using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource backGroundaudioSource; //may need to be public
    public AudioClip background1;
    public AudioClip background2;
    // Start is called before the first frame update
    void Start(){
        PlayBackgroundMusic(background1);
    }

     // Function to play background music clip
    void PlayBackgroundMusic(AudioClip clip)
    {
        backGroundaudioSource.clip = clip;
        backGroundaudioSource.Play();
    }

    // Coroutine to manage the background music loop
    IEnumerator BackgroundMusicLoop(){
        while (true)
        {
            if(backGroundaudioSource !=null ){
                // Wait for the current clip to finish playing
                yield return new WaitForSeconds(backGroundaudioSource.clip.length);

                // Play the next background music clip
                if (backGroundaudioSource.clip == background1)
                {
                    PlayBackgroundMusic(background2);
                }
                else
                {
                    PlayBackgroundMusic(background1);
                }
            }   
        }
    }

     // Start the background music loop when the game starts
    void OnEnable(){
        StartCoroutine(BackgroundMusicLoop());
    }

    // Stop the background music loop when the game is stopped or disabled
    void OnDisable(){
        StopCoroutine(BackgroundMusicLoop());
    }
}
