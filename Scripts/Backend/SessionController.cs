using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SessionController : MonoBehaviour
{
    public bool sessionRunning { get; private set; } = true;
    public CanvasGroup sessionMenuGroup;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            ToggleSessionMenu(sessionRunning);
        }
    }

    public void ToggleSessionMenu(bool paused)
    {
        if (paused)
        {
            sessionRunning = false;
            sessionMenuGroup.alpha = 1.0f;
            sessionMenuGroup.blocksRaycasts = true;
            sessionMenuGroup.interactable = true;
            GameObject.Find("PauseButton").GetComponent<Button>().interactable = false;
        }
        else
        {
            sessionRunning = true;
            sessionMenuGroup.alpha = 0.0f;
            sessionMenuGroup.blocksRaycasts = false;
            sessionMenuGroup.interactable = false;
            GameObject.Find("PauseButton").GetComponent<Button>().interactable = true;
        }
    }

    public void OptionsMenu() { throw new NotImplementedException("In-game options menu not implemented."); }
    public void MainMenu() { SceneManager.LoadScene(0); }
    public void Quit() { Application.Quit(0); }
}
