using UnityEngine;
using UnityEngine.SceneManagement;

public class mainmenu : MonoBehaviour
{
    public GameObject settings;
    public GameObject root;

    public GameObject tutorial;

    public GameObject credits; 

   
    
    public void Play()
    {
        SceneManager.LoadScene(1);
    }
    public void HowToPlay()
    {
        root.SetActive(false);
        tutorial.SetActive(true);
    }
    public void Settings()
    {
        root.SetActive(false);
        settings.SetActive(true);
    }
    public void Credits()
    {
        root.SetActive(false);
        credits.SetActive(true);
    }
    public void QuitButton()
    {

        Application.Quit();

    }
    public void BackButton()
    {

        root.SetActive(true);
        settings.SetActive(false);
        tutorial.SetActive(false);
        credits.SetActive(false);

    }


}
