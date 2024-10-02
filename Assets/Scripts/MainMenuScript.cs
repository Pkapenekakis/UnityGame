using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    public void Play(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); //Loads the next scene from the Main Menu
    }   

    public void Quit(){
        Application.Quit();
        Debug.Log("Player Has Quit The Game"); //since it does not close unity we get a message to ensure this works
    }

    public void Credits(){

    }
}
