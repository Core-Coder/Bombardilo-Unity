using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
  public static bool isPaused = false;
  public GameObject pauseMenuUI;
  public GameObject otherUI;
  public GameObject otherUI2;

  void Update()
  {
    if (Input.GetKeyDown(KeyCode.Escape))
    {
      if (isPaused)
      {
        Resume();
      }
      else
      {
        Pause();
      }
    }
  }

  public void Resume()
  {
    pauseMenuUI.SetActive(false);
    otherUI.SetActive(true);
    if (otherUI2)
    {
      otherUI2.SetActive(true);
    }
    Time.timeScale = 1f;
    isPaused = false;
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
  }

  void Pause()
  {
    pauseMenuUI.SetActive(true);
    otherUI.SetActive(false);
    if (otherUI2)
    {
      otherUI2.SetActive(false);
    }
    Time.timeScale = 0f;
    isPaused = true;
    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;
  }

  public void LoadMainMenu()
  {
    Time.timeScale = 1f;
    SceneManager.LoadScene(0);
  }
}