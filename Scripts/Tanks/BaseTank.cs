using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TankArena.Tanks
{
    public class BaseTank : MonoBehaviour
    {
        public AudioSource muzzleAudioSource;
        public AudioSource chassisAudioSource;
        private GameObject activeWeapon = null;
        protected Transform muzzleFlashTransform;
        private Vector3 fireDirection;

        protected GameObject muzzleObject = null;

        [Header("Common Tank Values")]
        public bool isGrounded = false;
        public bool isP1Tank = false;
        public float firePowerBase = 1.0f;
        public float firePowerModifier = 0.1f;
        public float tankSpeed = 5.0f;
        public Vector2 minMaxElevation = Vector2.zero;
        public AnimationCurve muzzleFlashIntensityCurve;
        [Tooltip("All fire effects.")]
        public AudioClip[] fireAudioClips = new AudioClip[3];
        [Tooltip("Idle, Rev Up, Drive, Rev Down.")]
        public AudioClip[] driveAudioClips = new AudioClip[2];
        [Tooltip("Turret Rotate Effect.")]
        public AudioClip turretRotateAudioClip;
        [Tooltip("Reload Sound Effect.")]
        public AudioClip turretReloadAudioClip;
        public CanvasGroup weaponGroup, controlGroup;
        public GameObject[] roundTypes = new GameObject[3];

        private void Awake()
        {
            GameObject.FindWithTag("Player 2").GetComponent<BasicTank>().ToggleTurn(false);
            GameObject.FindWithTag("Player").GetComponent<BasicTank>().ToggleTurn(true);
        }

        private void Update()
        {
            Debug.Log(gameObject.name);
        }

        /// <summary>
        /// Sets up the tank at the beginning of the play session.
        /// </summary>
        protected void InitializeTank()
        {
            muzzleFlashTransform = transform.GetChild(0).GetChild(0);
            muzzleFlashTransform.gameObject.GetComponent<Light>().intensity = 0;
            muzzleObject = muzzleFlashTransform.gameObject;

            chassisAudioSource = GetComponent<AudioSource>();
            muzzleAudioSource = muzzleObject.GetComponent<AudioSource>();
            muzzleAudioSource = muzzleObject.GetComponent<AudioSource>();
            SetWeapon(0);
        }

        /// <summary>
        /// Updates the selected weapon.
        /// </summary>
        /// <param name="index">Position in tank's weapon array.</param>
        public void SetWeapon(int index)
        {
            activeWeapon = roundTypes[index];
        }

        /// <summary>
        /// UI delegate for initiating fire.
        /// </summary>
        /// <param name="muzzleLocation">Endpoint of the tank's muzzle.</param>
        public void FireWeaponUI(Transform muzzleLocation)
        {
            ToggleTurn(false);
            StartCoroutine(FireWeapon(muzzleLocation));
        }

        /// <summary>
        /// Instantiates and applies force to the currently selected weapon before updating which tank is taking a turn.
        /// </summary>
        /// <param name="muzzleLocation">Endpoint of the tank's muzzle.</param>
        /// <returns>Null.</returns>
        public IEnumerator FireWeapon(Transform muzzleLocation)
        {
            if (activeWeapon == null)
            {
                SetWeapon(0);
            }
            float currentTime = 0.0f;
            float totalTime = 0.15f;

            while (currentTime < totalTime)
            {
                currentTime += Time.deltaTime;
                muzzleLocation.gameObject.GetComponent<Light>().intensity = muzzleFlashIntensityCurve.Evaluate(currentTime / totalTime);
                yield return null;
            }

            Vector3 fireDirection = (muzzleLocation.position - muzzleLocation.parent.position).normalized;
            GameObject firedRound = Instantiate(activeWeapon, new Vector3(muzzleLocation.transform.position.x, muzzleLocation.transform.position.y, 0), muzzleLocation.parent.rotation);
            firedRound.transform.rotation = Quaternion.AngleAxis(Mathf.Atan2(fireDirection.y, fireDirection.x) * Mathf.Rad2Deg, Vector3.forward);
            firedRound.GetComponent<Rigidbody2D>().AddForce(fireDirection * firePowerModifier * firePowerBase * activeWeapon.GetComponent<BasicRound>().weaponForceModifier, ForceMode2D.Impulse);

            muzzleAudioSource.PlayOneShot(fireAudioClips[fireAudioClips.Length - 1]);
            yield return new WaitForSeconds(1.0f);
            muzzleAudioSource.PlayOneShot(turretReloadAudioClip);
            yield return new WaitForSeconds(turretReloadAudioClip.length);

            if (FindObjectOfType<TwoPlayerController>().isP2Turn)
            {
                FindObjectOfType<TwoPlayerController>().isP2Turn = false;
                GameObject.FindWithTag("Player 2").GetComponent<BasicTank>().ToggleTurn(false);
                GameObject.FindWithTag("Player").GetComponent<BasicTank>().ToggleTurn(true);
            }
            else
            {
                FindObjectOfType<TwoPlayerController>().isP2Turn = true;
                GameObject.FindWithTag("Player 2").GetComponent<BasicTank>().ToggleTurn(true);
                GameObject.FindWithTag("Player").GetComponent<BasicTank>().ToggleTurn(false);
            }
        }

        /// <summary>
        /// Enables or disables control UI for this tank depending on turnActive value.
        /// If the value is true the main input controller will also be updated.
        /// </summary>
        /// <param name="turnActive">Value determining UI status.</param>
        public void ToggleTurn(bool turnActive)
        {
            if (turnActive)
            {
                controlGroup.interactable = true;
                weaponGroup.interactable = true;
                FindObjectOfType<TwoPlayerController>().activeTankBase = this;
                FindObjectOfType<TwoPlayerController>().UpdateBaseTankValues(this);
            }
            else
            {
                controlGroup.interactable = false;
                weaponGroup.interactable = false;
            }
        }

        /// <summary>
        /// Check if the tank gameObject has come in contact with the ground.
        /// </summary>
        /// <param name="collision">Detected collision.</param>
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.tag.ToLower().Equals("ground")) { isGrounded = true; }
        }

        /// <summary>
        /// Check if tank gameObject has lost contact with the ground.
        /// </summary>
        /// <param name="collision">Detected separation.</param>
        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.tag.ToLower().Equals("ground")) { isGrounded = false; }
        }
    }
}