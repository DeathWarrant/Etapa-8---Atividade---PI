using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
public class MobilePlayerBehaviour : MonoBehaviour
{
    public int health = 0;
    public int rotationSpeed = 0;
    public int maxAmmoOnWeapon = 0;
    public int maxCarryingAmmo = 0;
    public float moveSpeed = 0.0f;
    public float shootCooldown = 0.0f;
    public float reloadTime = 0.0f;
    public float walkCooldownTime = 0.0f;
    public AudioClip[] damageSounds;
    public AudioClip[] walkSounds;
    public AudioClip[] weaponSounds;
    public AudioClip reloadSound = null;
    public GameObject bulletPrefab = null;
    public GameObject bulletSpawn = null;
    public LeftJoystick leftJoystick = null;
    public RightJoystick rightJoystick = null;
    public Transform rotationTarget = null;
    //public Animator animator = null;

    private bool isShootOnCooldown = false;
    private bool isWalkOnCooldown = false;
    private bool isRightStep = true;
    private bool playReloadSound = true;
    private int ammo = 0;
    private int bulletPoolCounter = 0;
    private int maxAmmo = 0;
    private float shootCooldownTimer = 0.0f;
    private float walkCooldownTimer = 0.0f;
    private float leftJoystickX = 0.0f;
    private float leftJoystickY = 0.0f;
    private float rightJoystickX = 0.0f;
    private float rightJoytsickY = 0.0f;
    private AudioSource audioSource = null;
    private GameObject[] bulletPool = new GameObject[10];
    private Rigidbody rigidBody = null;
    private Vector3 leftJoystickInput = Vector3.zero;
    private Vector3 rightJoystickInput = Vector3.zero;

    private void Start()
    {
        StartComponents();
    }

    private void FixedUpdate()
    {
        GetInputs();
        DoActions();
    }

    private void StartComponents()
    {
        maxAmmo = maxCarryingAmmo;
        ammo = maxAmmoOnWeapon;
        bulletPoolCounter = 0;

        audioSource = GetComponent<AudioSource>();

        for (int i = 0; i < bulletPool.Length; i++)
        {
            bulletPool[i] = Instantiate(bulletPrefab, bulletSpawn.transform.position, bulletSpawn.transform.rotation);
            bulletPool[i].SetActive(false);
        }

        if (transform.GetComponent<Rigidbody>() == null)
        {
            Debug.LogError("A RigidBody component is required on this game object.");
        }
        else
        {
            rigidBody = transform.GetComponent<Rigidbody>();
        }

        if (leftJoystick == null)
        {
            Debug.LogError("The left joystick is not attached.");
        }

        if (rightJoystick == null)
        {
            Debug.LogError("The right joystick is not attached.");
        }

        if (rotationTarget == null)
        {
            Debug.LogError("The target rotation game object is not attached.");
        }
    }

    private void GetInputs()
    {
        leftJoystickInput = leftJoystick.GetInputDirection();
        rightJoystickInput = rightJoystick.GetInputDirection();

        leftJoystickX = leftJoystickInput.x;
        leftJoystickY = leftJoystickInput.y;

        rightJoystickX = rightJoystickInput.x;
        rightJoytsickY = rightJoystickInput.y;
    }

    private void DoActions()
    {
        /*if (leftJoystickInput == Vector3.zero)
        {
            animator.SetBool("isRunning", false);
        }

        if (rightJoystickInput == Vector3.zero)
        {
            animator.SetBool("isAttacking", false);
        }*/

        // LEFT JOYSTICK INPUT ONLY
        if (leftJoystickInput != Vector3.zero && rightJoystickInput == Vector3.zero)
        {
            // calculate the player's direction based on angle
            float tempAngle = Mathf.Atan2(leftJoystickY, leftJoystickX);
            leftJoystickX *= Mathf.Abs(Mathf.Cos(tempAngle));
            leftJoystickY *= Mathf.Abs(Mathf.Sin(tempAngle));

            leftJoystickInput = new Vector3(leftJoystickX, 0, leftJoystickY);
            leftJoystickInput = transform.TransformDirection(leftJoystickInput);
            leftJoystickInput *= moveSpeed;

            // rotate the player to face the direction of input
            Vector3 temp = transform.position;
            temp.x += leftJoystickX;
            temp.z += leftJoystickY;
            Vector3 lookDirection = temp - transform.position;

            if (lookDirection != Vector3.zero)
            {
                rotationTarget.localRotation = Quaternion.Slerp(rotationTarget.localRotation, Quaternion.LookRotation(lookDirection), rotationSpeed * Time.deltaTime);
            }
            /*if (animator != null)
            {
                animator.SetBool("isRunning", true);
            }*/

            // move the player
            rigidBody.transform.Translate(leftJoystickInput * Time.fixedDeltaTime);
        }

        // RIGHT JOYSTICK INPUT ONLY
        if (leftJoystickInput == Vector3.zero && rightJoystickInput != Vector3.zero)
        {
            // calculate the player's direction based on angle
            float tempAngle = Mathf.Atan2(rightJoytsickY, rightJoystickX);
            rightJoystickX *= Mathf.Abs(Mathf.Cos(tempAngle));
            rightJoytsickY *= Mathf.Abs(Mathf.Sin(tempAngle));

            // rotate the player to face the direction of input
            Vector3 temp = transform.position;
            temp.x += rightJoystickX;
            temp.z += rightJoytsickY;
            Vector3 lookDirection = temp - transform.position;

            if (lookDirection != Vector3.zero)
            {
                rotationTarget.localRotation = Quaternion.Slerp(rotationTarget.localRotation, Quaternion.LookRotation(lookDirection) * Quaternion.Euler(0, 45f, 0), rotationSpeed * Time.deltaTime);
            }

            Fire();

            //animator.SetBool("isAttacking", true);
        }

        // INPUT FROM BOTH JOYSTICKS
        if (leftJoystickInput != Vector3.zero && rightJoystickInput != Vector3.zero)
        {
            // calculate the player's direction based on angle
            float tempAngleInputRightJoystick = Mathf.Atan2(rightJoytsickY, rightJoystickX);
            rightJoystickX *= Mathf.Abs(Mathf.Cos(tempAngleInputRightJoystick));
            rightJoytsickY *= Mathf.Abs(Mathf.Sin(tempAngleInputRightJoystick));

            // rotate the player to face the direction of input
            Vector3 temp = transform.position;
            temp.x += rightJoystickX;
            temp.z += rightJoytsickY;
            Vector3 lookDirection = temp - transform.position;
            if (lookDirection != Vector3.zero)
            {
                rotationTarget.localRotation = Quaternion.Slerp(rotationTarget.localRotation, Quaternion.LookRotation(lookDirection) * Quaternion.Euler(0, 45f, 0), rotationSpeed * Time.deltaTime);
            }

            //animator.SetBool("isAttacking", true);

            // calculate the player's direction based on angle
            float tempAngleLeftJoystick = Mathf.Atan2(leftJoystickY, leftJoystickX);
            leftJoystickX *= Mathf.Abs(Mathf.Cos(tempAngleLeftJoystick));
            leftJoystickY *= Mathf.Abs(Mathf.Sin(tempAngleLeftJoystick));

            leftJoystickInput = new Vector3(leftJoystickX, 0, leftJoystickY);
            leftJoystickInput = transform.TransformDirection(leftJoystickInput);
            leftJoystickInput *= moveSpeed;

            /*if (animator != null)
            {
                animator.SetBool("isRunning", true);
            }*/

            rigidBody.transform.Translate(leftJoystickInput * Time.fixedDeltaTime);

            Fire();
        }

        if(leftJoystickX != 0.0f || leftJoystickY != 0.0f)
        {
            WalkSound();
        }

        if (isShootOnCooldown)
        {
            shootCooldownTimer += Time.deltaTime;

            if (shootCooldownTimer > shootCooldown)
            {
                shootCooldownTimer = 0.0f;
                isShootOnCooldown = false;
            }
        }

        if (isWalkOnCooldown)
        {
            walkCooldownTimer += Time.deltaTime;

            if (walkCooldownTimer > walkCooldownTime)
            {
                isWalkOnCooldown = false;
                walkCooldownTimer = 0.0f;
            }
        }

        Reload();
    }

    public int GetAmmo()
    {
        return ammo;
    }

    public int GetMaxAmmo()
    {
        return maxAmmo;
    }

    public int GetHealth()
    {
        return health;
    }

    public void RestartVariables()
    {
        health = 100;
        maxAmmo = maxCarryingAmmo;
        ammo = maxAmmoOnWeapon;
        rigidBody.velocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;
    }

    private void WalkSound()
    {
        if(!isWalkOnCooldown)
        {
            if (isRightStep)
            {
                audioSource.PlayOneShot(walkSounds[0]);
                isRightStep = false;
            }
            else
            {
                audioSource.PlayOneShot(walkSounds[1]);
                isRightStep = true;
            }

            isWalkOnCooldown = true;
        }

    }

    private void Fire()
    {
        if (!isShootOnCooldown && ammo > 0)
        {
            if (bulletPoolCounter >= bulletPool.Length)
            {
                bulletPoolCounter = 0;
            }

            audioSource.PlayOneShot(weaponSounds[0], 0.15f);
            bulletPool[bulletPoolCounter].transform.position = bulletSpawn.transform.position;
            bulletPool[bulletPoolCounter].transform.rotation = bulletSpawn.transform.rotation;
            bulletPool[bulletPoolCounter].SetActive(true);
            bulletPoolCounter++;
            ammo--;

            isShootOnCooldown = true;
        }
    }

    private void Reload()
    {
        if (!isShootOnCooldown && ammo <= 0 && maxAmmo > 0)
        {
            if(playReloadSound)
            {
                audioSource.PlayOneShot(reloadSound, 0.5f);
                playReloadSound = false;
            }

            shootCooldownTimer += Time.deltaTime;

            if (shootCooldownTimer > reloadTime)
            {
                if (maxAmmo - maxAmmoOnWeapon >= 0)
                {
                    maxAmmo -= maxAmmoOnWeapon;
                    ammo = maxAmmoOnWeapon;
                }
                else
                {
                    ammo = maxAmmo;
                    maxAmmo -= maxAmmo;
                }

                shootCooldownTimer = 0.0f;
                playReloadSound = true;
            }
        }
    }

    public void DoDamage(int p_damage)
    {
        StartCoroutine(PlayDamageSoundWithDelay());
        health -= p_damage;

        if (health <= 0)
        {
            health = 0;
            GameControllerBehaviour.gameControllerInstance.ShowEndScreen();
            gameObject.SetActive(false);
        }
    }

    public void AddAmmo(int p_ammo)
    {
        maxAmmo += p_ammo;
    }

    private IEnumerator PlayDamageSoundWithDelay()
    {
        yield return new WaitForSeconds(0.2f);
        audioSource.PlayOneShot(damageSounds[Random.Range(0, damageSounds.Length)], 0.5f);
    }
}