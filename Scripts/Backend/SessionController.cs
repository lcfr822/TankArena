using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SessionController : MonoBehaviour
{
    private Dictionary<Rigidbody2D, Vector2> delayedVelocities = new Dictionary<Rigidbody2D, Vector2>();

    public bool sessionRunning = true;
    public CanvasGroup sessionMenuGroup;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleSessionMenu()
    {
        if (sessionMenuGroup.alpha <= 0.0f)
        {
            sessionMenuGroup.alpha = 1.0f;
            sessionMenuGroup.blocksRaycasts = true;
            sessionMenuGroup.interactable = true;
            GameObject.Find("PauseButton").GetComponent<Button>().interactable = false;
            FindObjectOfType<TwoPlayerController>().p1Tank.gameObject.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
            foreach(Rigidbody2D rb2d in FindObjectsOfType<Rigidbody2D>())
            {
                delayedVelocities.Add(rb2d, rb2d.velocity);
            }
        }
        else
        {
            sessionMenuGroup.alpha = 0.0f;
            sessionMenuGroup.blocksRaycasts = false;
            sessionMenuGroup.interactable = false;
            GameObject.Find("PauseButton").GetComponent<Button>().interactable = true;
            FindObjectOfType<TwoPlayerController>().p1Tank.gameObject.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
            foreach(Rigidbody2D key in delayedVelocities.Keys)
            {
                Debug.Log(delayedVelocities[key]);
                key.velocity = delayedVelocities[key];
            }
        }
    }

    public void MainMenu() { SceneManager.LoadScene(0); }
    public void Quit() { Application.Quit(0); }
}
