using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    private GameManager(){} // Private constructor to prevent instantiation
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                // Find the InventoryManager in the scene if it exists
                instance = FindObjectOfType<GameManager>();

                // If it doesn't exist, create a new GameObject and add the InventoryManager component
                if (instance == null)
                {
                    GameObject gameManager = new GameObject("GameManager");
                    instance = gameManager.AddComponent<GameManager>();
                }
            }
            return instance;
        }
    }
    public GameObject gameOverUI;
    [SerializeField] private int yellowPillarsUnlocked;
    [SerializeField] private int bluePillarsUnlocked;
    [SerializeField] private int greenPillarsUnlocked;

    private void Awake() {
        yellowPillarsUnlocked = 0;
        bluePillarsUnlocked = 0;
        greenPillarsUnlocked = 0;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(yellowPillarsUnlocked >= 1 && bluePillarsUnlocked >= 1 && greenPillarsUnlocked >= 1){
            //Game is completed Logic
            GameOver();
        }
    }

    public void setYellowPillar(){
        yellowPillarsUnlocked++;
    }
    public void setBluePillar(){
        bluePillarsUnlocked++;
    }
    public void setGreenPillar(){
        greenPillarsUnlocked++;
    }
    
    public void GameOver(){
        Time.timeScale = 0; //pause the game
        gameOverUI.SetActive(true);
        Utils.Instance.CursorManagement(false);
    }

    public void Restart(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Debug.Log("Restart");
    }

    public void MainMenu(){
        SceneManager.LoadScene("MainMenuScene");
        Debug.Log("Main M");
    }

    public void Quit(){
        Application.Quit();
        Debug.Log("Quit");
    }
}
