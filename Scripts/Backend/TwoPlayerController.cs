using System;
using System.Collections;
using System.Collections.Generic;
using TankArena.Tanks;
using UnityEngine;
using UnityEngine.UI;

public class TwoPlayerController : MonoBehaviour
{
    private bool audioFadingIn = false;
    private bool audioFadingOut = false;
    private IEnumerator fadeInRoutine, fadeOutRoutine = null;

    [Header("Gameplay and GameObject Variables")]
    public bool isP2Human = false;
    public bool isP2Turn = false;
    public float reorientationSpeed = 5.0f;
    public Rigidbody2D p1Tank, p2Tank;
    public BaseTank activeTankBase = null;

    [Header("UI Variables")]
    public Slider p1Accelerator;
    public Slider p2Accelerator;
    public Slider p1HealthBar, p2HealthBar;
    public Image p1FuelLevel, p2FuelLevel;

    private void Start()
    {
        activeTankBase = p1Tank.GetComponent<BaseTank>();
        activeTankBase.chassisAudioSource.clip = activeTankBase.driveAudioClips[0];
        activeTankBase.chassisAudioSource.Play();

        FindObjectOfType<AnalogStick>().onValueChanged.AddListener(ReceiveTargetingData);
    }

    private void Update()
    {
        if (FindObjectOfType<SessionController>().sessionRunning) { HandleMovement(); }

        // Reorient tanks to upward position if not grounded.
        if (!p1Tank.GetComponent<BaseTank>().isGrounded)
        {
            p1Tank.gameObject.transform.rotation = Quaternion.RotateTowards(p1Tank.gameObject.transform.rotation, Quaternion.identity, reorientationSpeed * Time.deltaTime);
        }
        if (!p2Tank.GetComponent<BaseTank>().isGrounded)
        {
            p2Tank.gameObject.transform.rotation = Quaternion.RotateTowards(p2Tank.gameObject.transform.rotation, Quaternion.identity, reorientationSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Apply input based on which phase the game is in and whether P2 is a bot person or bot.
    /// Fade Audio if breaking or accelerating, loop idle if no input, loop drive at full volume if at full speed.
    /// </summary>
    private void HandleMovement()
    {
        float timeRemainderFraction = 1.0f;
        if (activeTankBase.chassisAudioSource.volume < 1.0f)
        {
            timeRemainderFraction = 1.0f - activeTankBase.chassisAudioSource.volume;
        }

        if (!isP2Turn && p1Tank.GetComponent<BaseTank>().isGrounded && p1FuelLevel.fillAmount > 0)
        {
            p1Accelerator.value = Input.GetAxis("P1Horizontal");
            if (p1Accelerator.value != 0)
            {
                p1Tank.AddForce(Vector2.right * p1Accelerator.value * p1Tank.GetComponent<BaseTank>().tankSpeed * Time.deltaTime);
                if (fadeOutRoutine != null) { StopCoroutine(fadeOutRoutine); audioFadingOut = false; ; }
                if (!audioFadingIn && activeTankBase.chassisAudioSource.volume < 1.0f)
                {
                    p1FuelLevel.fillAmount -= Time.deltaTime / 4.0f;
                    fadeInRoutine = FadeInAudio(activeTankBase.chassisAudioSource, activeTankBase.driveAudioClips[1], activeTankBase.chassisAudioSource.volume, 1.0f, timeRemainderFraction);
                    StartCoroutine(fadeInRoutine);
                }
            }
            else
            {
                p1Accelerator.value = 0;
                if (fadeInRoutine != null) { StopCoroutine(fadeInRoutine); audioFadingIn = false; }
                if (!audioFadingOut && activeTankBase.chassisAudioSource.volume > 0.1f)
                {
                    fadeOutRoutine = FadeOutAudio(activeTankBase.chassisAudioSource, activeTankBase.driveAudioClips[0], activeTankBase.chassisAudioSource.volume, 0.1f, timeRemainderFraction);
                    StartCoroutine(fadeOutRoutine);
                }
            }
        }
        else if (isP2Turn && isP2Human && p2Tank.GetComponent<BaseTank>().isGrounded && p2FuelLevel.fillAmount > 0)
        {
            p2Accelerator.value = -Input.GetAxis("P2Horizontal");
            if (p2Accelerator.value != 0)
            {
                p2Tank.AddForce(Vector2.right * -p2Accelerator.value * p2Tank.GetComponent<BaseTank>().tankSpeed * Time.deltaTime);
                if (fadeOutRoutine != null) { StopCoroutine(fadeOutRoutine); audioFadingOut = false; }
                if (!audioFadingIn && activeTankBase.chassisAudioSource.volume < 1.0f)
                {
                    p2FuelLevel.fillAmount -= Time.deltaTime / 4.0f;
                    fadeInRoutine = FadeInAudio(activeTankBase.chassisAudioSource, activeTankBase.driveAudioClips[1], activeTankBase.chassisAudioSource.volume, 1.0f, timeRemainderFraction);
                    StartCoroutine(fadeInRoutine);
                }
            }
            else
            {
                p2Accelerator.value = 0;
                if (fadeInRoutine != null) { StopCoroutine(fadeInRoutine); audioFadingIn = false; }
                if (!audioFadingOut && activeTankBase.chassisAudioSource.volume > 0.1f)
                {
                    fadeOutRoutine = FadeOutAudio(activeTankBase.chassisAudioSource, activeTankBase.driveAudioClips[0], activeTankBase.chassisAudioSource.volume, 0.1f, timeRemainderFraction);
                    StartCoroutine(fadeOutRoutine);
                }
            }
        }

        else if (isP2Turn && !isP2Human && p2Tank.GetComponent<BaseTank>().isGrounded)
        {
            BotTurn();
        }
    }

    /// <summary>
    /// Assign an AudioClip to an AudioSource and fade it in to full volume over fadeTime seconds.
    /// </summary>
    /// <param name="audioSource">Target AudioSource.</param>
    /// <param name="driveClip">AudioClip to fade in.</param>
    /// <param name="startVolume">Initial point of the fade in (0-1).</param>
    /// <param name="targetVolume">Target volume of the fade in (1-0).</param>
    /// <param name="fadeTime">How long the fade in should last.</param>
    /// <returns>Null.</returns>
    private IEnumerator FadeInAudio(AudioSource audioSource, AudioClip driveClip, float startVolume, float targetVolume, float fadeTime)
    {
        audioFadingIn = true;
        audioSource.volume = startVolume;
        audioSource.clip = driveClip;
        audioSource.Play();

        while (audioSource.volume <= targetVolume)
        {
            audioSource.volume += Time.deltaTime / fadeTime;
            if (audioSource.volume >= targetVolume) { break; }
            yield return null;
        }

        fadeInRoutine = null;
        audioFadingIn = false;
    }

    /// <summary>
    /// Assign an AudioClip to an AudioSource and fade it out to a specified volume over fadeTime seconds.
    /// </summary>
    /// <param name="audioSource">Target AudioSource.</param>
    /// <param name="idleClip">AudioClip to fade in.</param>
    /// <param name="startVolume">Initial point for the fade out (1-0).</param>
    /// <param name="targetVolume">Target volume of the fade out (0-1).</param>
    /// <param name="fadeTime">How long the fade out should last.</param>
    /// <returns>Null.</returns>
    private IEnumerator FadeOutAudio(AudioSource audioSource, AudioClip idleClip, float startVolume, float targetVolume, float fadeTime)
    {
        audioFadingOut = true;
        while (audioSource.volume >= targetVolume)
        {
            audioSource.volume -= startVolume * Time.deltaTime / fadeTime;
            if (audioSource.volume <= targetVolume) { break; }
            yield return null;
        }

        audioSource.clip = idleClip;
        audioSource.Play();
        fadeOutRoutine = null;
        audioFadingOut = false;
    }

    /// <summary>
    /// Stop continuous functions of previous activeBaseTank and intitialize new activeBaseTank.
    /// </summary>
    /// <param name="baseTank">Current BaseTank class.</param>
    public void UpdateBaseTankValues(BaseTank baseTank)
    {
        activeTankBase.chassisAudioSource.Stop();
        activeTankBase.muzzleAudioSource.Stop();
        activeTankBase = baseTank;
        activeTankBase.chassisAudioSource.clip = activeTankBase.driveAudioClips[0];
        activeTankBase.chassisAudioSource.Play();
        if (!isP2Turn) { p1FuelLevel.fillAmount = 1.0f; }
        else { p2FuelLevel.fillAmount = 1.0f; }
    }

    /// <summary>
    /// Control bot turn.
    /// </summary>
    private void BotTurn()
    {
        throw new NotImplementedException("Bot turns have not been implemented!");
    }

    /// <summary>
    /// Listens for analog stick input.
    /// </summary>
    /// <param name="value"></param>
    public void ReceiveTargetingData(Vector2 value)
    {
        Debug.Log("Receiving Targeting value of: " + value.ToString());
    }
}
