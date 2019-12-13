using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Add this "using" statement to allow level loading.

public class MainMenuController : MonoBehaviour
{
    public CanvasGroup mainMenuGroup, optionMenuGroup;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Load the play scene when the player clicks "PLAY".
    /// </summary>
    public void Play()
    {
        SceneManager.LoadScene(1);
    }

    /// <summary>
    /// Show or hide the game's main menu based on its color's alpha value.
    /// </summary>
    public void ToggleMainMenuVisibility()
    {
        if(mainMenuGroup.alpha < 1.0f)
        {
            mainMenuGroup.alpha = 1.0f;
            mainMenuGroup.blocksRaycasts = true;
            mainMenuGroup.interactable = true;
        }
        else
        {
            mainMenuGroup.alpha = 0.0f;
            mainMenuGroup.blocksRaycasts = false;
            mainMenuGroup.interactable = false;
        }
    }

    /// <summary>
    /// Show or hide the game's options based on their color's alpha value.
    /// </summary>
    public void ToggleOptionsVisibility()
    {
        if(optionMenuGroup.alpha < 1.0f)
        {
            ToggleMainMenuVisibility();

            optionMenuGroup.alpha = 1.0f;
            optionMenuGroup.blocksRaycasts = true;
            optionMenuGroup.interactable = true;
        }
        else
        {
            ToggleMainMenuVisibility();

            optionMenuGroup.alpha = 0.0f;
            optionMenuGroup.blocksRaycasts = false;
            optionMenuGroup.interactable = false;
        }
    }

    public void ReturnToMainMenu()
    {
        if(optionMenuGroup.alpha >= 1.0f) { ToggleOptionsVisibility(); }
    }

    /// <summary>
    /// Quit the game with exit code 0 (normal exit).
    /// </summary>
    public void QuitGame() { Application.Quit(0); }
}
