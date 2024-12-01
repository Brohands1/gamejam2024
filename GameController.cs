using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{



    public GameObject GameOverPanel;
    

    public static GameController Instance;
    
    public GameObject GameEnd;



    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
    }

    public void ShowGameOverPanel()
    {
        GameOverPanel.SetActive(true);
    }

    public void ShowGameEnd()
    {
        GameEnd.SetActive(true);
    }

    public void Restartlevel(string levelname)
    {
        SceneManager.LoadScene(levelname);
    }
}
