using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Buttons : MonoBehaviour
{
    // Start is called before the first frame update
    public void Quit()
    {
        Application.Quit();
    }

    // Update is called once per frame
    public void Resume()
    {
        Time.timeScale = 1f; // Resume the game
    }

    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f; // Resume the game before loading a new scene
        SceneManager.LoadScene(sceneName);
    }
}
