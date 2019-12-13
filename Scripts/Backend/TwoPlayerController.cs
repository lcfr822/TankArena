using System;
using System.Collections;
using System.Collections.Generic;
using TankArena.Tanks;
using UnityEngine;
using UnityEngine.UI;

public class TwoPlayerController : MonoBehaviour
{
    private enum DriveState { idle, rev, driving, breaking };
    private DriveState currentState = DriveState.idle;
    private DriveState prevState = DriveState.idle;
    

    [Header("Gameplay and GameObject Variables")]
    public bool isP2Human = false;
    public bool isP2Turn = false;
    public int p1Fuel = 100;
    public int p2Fuel = 100;
    public Rigidbody2D p1Tank, p2Tank;
    public BaseTank activeTankBase = null;

    public Image test;

    [Header("UI Variables")]
    public Slider p1Accelerator;
    public Slider p2Accelerator;
    public Slider p1HealthBar, p2HealthBar;
    public Image p1FuelLevel, p2FuelLevel;

    // Start is called before the first frame update
    void Start()
    {
        activeTankBase = p1Tank.GetComponent<BaseTank>();
    }

    // Update is called once per frame
    void Update()
    {
        if (FindObjectOfType<SessionController>().sessionRunning) { HandleMovement(); }
    }

    private void LateUpdate() { prevState = currentState; }

    /// <summary>
    /// Apply input based on which phase the game is in and whether P2 is a bot person or bot.
    /// </summary>
    private void HandleMovement()
    {
        if (!isP2Turn && p1Tank.GetComponent<BaseTank>().isGrounded)
        {
            p1Accelerator.value = Input.GetAxis("P1Horizontal");
            if (p1Accelerator.value != 0)
            {
                p1Tank.AddForce(Vector2.right * p1Accelerator.value * p1Tank.GetComponent<BaseTank>().tankSpeed * Time.deltaTime);
                if (prevState != DriveState.driving && prevState != DriveState.rev)
                {
                    currentState = DriveState.rev;
                }
            }
            else
            {
                p1Accelerator.value = 0;
            }
        }
        else if (isP2Turn && isP2Human && p2Tank.GetComponent<BaseTank>().isGrounded)
        {
            p2Accelerator.value = -Input.GetAxis("P2Horizontal");
            if (p2Accelerator.value != 0)
            {
                p2Tank.AddForce(Vector2.right * -p2Accelerator.value * p2Tank.GetComponent<BaseTank>().tankSpeed * Time.deltaTime);
                if (prevState != DriveState.driving && prevState != DriveState.rev)
                {
                    currentState = DriveState.rev;
                }
            }
            else
            {
                p2Accelerator.value = 0;
            }
        }

        else if (isP2Turn && !isP2Human && p2Tank.GetComponent<BaseTank>().isGrounded)
        {
            BotTurn();
        }

        if (p1Accelerator.value == 0 && currentState != DriveState.breaking && currentState != DriveState.idle)
        {
            currentState = DriveState.breaking;
        }

        if (p2Accelerator.value == 0 && currentState != DriveState.breaking && currentState != DriveState.idle)
        {
            currentState = DriveState.breaking;
        }

        if (!activeTankBase.chassisAudioSource.loop && currentState == DriveState.idle) { activeTankBase.chassisAudioSource.loop = true; }

        Debug.Log("Drive State: " + currentState);
        HandleMovementAudio();
    }

    private void HandleMovementAudio()
    {
        // If the tank is accelerating and the current chassis clip is idle.
        if (currentState == DriveState.rev && activeTankBase.chassisAudioSource.clip == activeTankBase.driveAudioClips[2])
        {
            Debug.Log("Reving from idle.");
            activeTankBase.chassisAudioSource.clip = activeTankBase.driveAudioClips[1];
        }
        // If the tank is accelerating and the current chassis clip is break.
        else if (currentState == DriveState.rev && activeTankBase.chassisAudioSource == activeTankBase.driveAudioClips[3])
        {
            Debug.Log("Reving from breaking.");
            float startTime = activeTankBase.chassisAudioSource.time;
            activeTankBase.chassisAudioSource.clip = activeTankBase.driveAudioClips[1];
            activeTankBase.chassisAudioSource.time = startTime;
        }
        // If the tank is breaking and the cucrrent chassis clip is accelerate.
        else if (currentState == DriveState.breaking && activeTankBase.chassisAudioSource.clip == activeTankBase.driveAudioClips[1])
        {
            Debug.Log("Breaking from reving.");
            float startTime = activeTankBase.chassisAudioSource.time;
            activeTankBase.chassisAudioSource.clip = activeTankBase.driveAudioClips[3];
            activeTankBase.chassisAudioSource.time = startTime;
        }
        // If the tank is breaking and the current chassis clip is drive.
        else if (currentState == DriveState.breaking && activeTankBase.chassisAudioSource.clip == activeTankBase.driveAudioClips[2])
        {
            Debug.Log("Breaking from drive.");
            activeTankBase.chassisAudioSource.clip = activeTankBase.driveAudioClips[3];
        }
        // Any other condition.
        else
        {
            Debug.Log("Idling.");
            currentState = DriveState.idle;
            activeTankBase.chassisAudioSource.clip = activeTankBase.driveAudioClips[0]; 
        }
        Debug.Log("Playing: " + activeTankBase.chassisAudioSource.clip.name);
        if (!activeTankBase.chassisAudioSource.isPlaying) { activeTankBase.chassisAudioSource.Play(); }
    }

    /// <summary>
    /// Control bot turn.
    /// </summary>
    private void BotTurn()
    {
        throw new NotImplementedException("Bot turns have not been implemented!");
    }
}
