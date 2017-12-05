using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
public class MobilePlayerBehaviour : MonoBehaviour
{
    public int health = 0;
    public int rotationSpeed = 0;
    public int maxAmmoOnNormalWeapon = 0;
    public int maxCarryingAmmo = 0;
    public float moveSpeed = 0.0f;
    public float shootCooldown = 0.0f;
    public float shotgunShootCooldown = 0.0f;
    public float reloadTime = 0.0f;
    public float walkCooldownTime = 0.0f;
    public AudioClip[] damageSounds;
    public AudioClip[] walkSounds;
    public AudioClip[] weaponSounds;
    public AudioClip reloadSound = null;
    public GameObject bulletPrefabShotgun = null;
    public GameObject bulletPrefab = null;
    public GameObject bulletSpawn = null;
    public GameObject rifle;
    public GameObject shotgun;
    public LeftJoystick leftJoystick = null;
    public RightJoystick rightJoystick = null;
    public Transform rotationTarget = null;
    //public Animator animator = null;

    private enum Weapon { NormalWeapon, Shotgun }
    private Weapon playerWeapon = 0;
    private bool isDead = false;
    private bool isShootOnCooldown = false;
    private bool isWalkOnCooldown = false;
    private bool isRightStep = true;
    private bool playReloadSound = true;
    private int[] ammo = null;
    private int[] bulletPoolCounter = new int[2];
    private int[] maxAmmo = null;
    private float shootCooldownTimer = 0.0f;
    private float walkCooldownTimer = 0.0f;
    private float leftJoystickX = 0.0f;
    private float leftJoystickY = 0.0f;
    private float rightJoystickX = 0.0f;
    private float rightJoystickY = 0.0f;
    private Animator animator = null;
    private AudioSource audioSource = null;
    private GameObject[] bulletPool = new GameObject[10];
    private GameObject[] bulletPoolShotgun = new GameObject[3];
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
        isDead = false;
        maxAmmo = new int[] { maxCarryingAmmo, 0 };
        ammo = new int[] { maxAmmoOnNormalWeapon, 0 };
        bulletPoolCounter = new int[2];
        bulletPool = new GameObject[10];
        bulletPoolShotgun = new GameObject[3];
        shotgun.SetActive(false);
        rifle.SetActive(true);

        audioSource = GetComponent<AudioSource>();
        animator = GetComponentInChildren<Animator>();

        for(int i = 0; i < bulletPoolCounter.Length; i++)
        {
            bulletPoolCounter[i] = 0;
        }

        for (int i = 0; i < bulletPool.Length; i++)
        {
            bulletPool[i] = Instantiate(bulletPrefab, bulletSpawn.transform.position, bulletSpawn.transform.rotation);
            bulletPool[i].SetActive(false);
        }

        for (int i = 0; i < bulletPoolShotgun.Length; i++)
        {
            bulletPoolShotgun[i] = Instantiate(bulletPrefabShotgun, bulletSpawn.transform.position, bulletSpawn.transform.rotation);
            bulletPoolShotgun[i].SetActive(false);
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
        if (!isDead)
        {
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                ChangeWeapon(1, 100);
                GameControllerBehaviour.gameControllerInstance.PlayCrateSound(2);
            }

            leftJoystickInput = leftJoystick.GetInputDirection();
            rightJoystickInput = rightJoystick.GetInputDirection();

            leftJoystickX = leftJoystickInput.x;
            leftJoystickY = leftJoystickInput.y;

            rightJoystickX = rightJoystickInput.x;
            rightJoystickY = rightJoystickInput.y;
        }
        else
        {
            leftJoystickX = 0.0f;
            leftJoystickY = 0.0f;

            rightJoystickX = 0.0f;
            rightJoystickY = 0.0f;
        }
    }

    private void DoActions()
    {
        if (GameControllerBehaviour.gameControllerInstance.GetGameState() == 0 || GameControllerBehaviour.gameControllerInstance.GetGameState() == 2)
            rigidBody.isKinematic = true;
        else
            rigidBody.isKinematic = false;

        if (rightJoystickX == 0.0f && rightJoystickY == 0.0f)
        {
            if (leftJoystickX != 0.0f && leftJoystickY != 0.0f)
            {
                float tempX = leftJoystickX < 0.0f ? leftJoystickX * -1 : leftJoystickX;
                float tempY = leftJoystickY < 0.0f ? leftJoystickY * -1 : leftJoystickY;
                float temp = tempX + tempY;
                animator.SetFloat("VelocityZ", temp);
            }
            else if (leftJoystickX != 0.0f && leftJoystickY == 0.0f)
            {
                float tempX = leftJoystickX < 0.0f ? leftJoystickX * -1 : leftJoystickX;
                animator.SetFloat("VelocityZ", tempX);
            }
            else if (leftJoystickX == 0.0f && leftJoystickY != 0.0f)
            {
                float tempY = leftJoystickY < 0.0f ? leftJoystickY * -1 : leftJoystickY;
                animator.SetFloat("VelocityZ", tempY);
            }
            else
            {
                animator.SetFloat("VelocityZ", leftJoystickY);
                animator.SetFloat("VelocityX", leftJoystickY);
            }
        }
        else if (rightJoystickX != 0.0f && rightJoystickY == 0.0f || rightJoystickX == 0.0f && rightJoystickY != 0.0f || rightJoystickX != 0.0f && rightJoystickY != 0.0f)
        {
            if (leftJoystickX != 0.0f && leftJoystickY != 0.0f)
            {
                float angleBetween = Mathf.Atan2(rightJoystickY, rightJoystickX) - Mathf.Atan2(leftJoystickY, leftJoystickX);
                animator.SetFloat("VelocityX", Mathf.Sin(angleBetween));
                animator.SetFloat("VelocityZ", Mathf.Cos(angleBetween));
            }
            else
            {
                animator.SetFloat("VelocityZ", leftJoystickY);
                animator.SetFloat("VelocityX", leftJoystickY);
            }
        }

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
                rotationTarget.localRotation = Quaternion.Slerp(rotationTarget.localRotation, Quaternion.LookRotation(lookDirection) * Quaternion.Euler(0.0f, 55.0f, 0.0f), rotationSpeed * Time.deltaTime);
            }

            // move the player
            rigidBody.transform.Translate(leftJoystickInput * Time.fixedDeltaTime);
        }

        // RIGHT JOYSTICK INPUT ONLY
        if (leftJoystickInput == Vector3.zero && rightJoystickInput != Vector3.zero)
        {
            // calculate the player's direction based on angle
            float tempAngle = Mathf.Atan2(rightJoystickY, rightJoystickX);
            rightJoystickX *= Mathf.Abs(Mathf.Cos(tempAngle));
            rightJoystickY *= Mathf.Abs(Mathf.Sin(tempAngle));

            // rotate the player to face the direction of input
            Vector3 temp = transform.position;
            temp.x += rightJoystickX;
            temp.z += rightJoystickY;
            Vector3 lookDirection = temp - transform.position;

            if (lookDirection != Vector3.zero)
            {
                rotationTarget.localRotation = Quaternion.Slerp(rotationTarget.localRotation, Quaternion.LookRotation(lookDirection) * Quaternion.Euler(0.0f, 55.0f, 0.0f), rotationSpeed * Time.deltaTime);
            }

            float tempX = rightJoystickX * -1;
            float tempY = rightJoystickY * -1;

            if (rightJoystickX + rightJoystickY > 0.9f || tempX + tempY > 0.9f || rightJoystickX + tempY > 0.9f || tempX + rightJoystickY > 0.9f)
                Fire();
        }

        // INPUT FROM BOTH JOYSTICKS
        if (leftJoystickInput != Vector3.zero && rightJoystickInput != Vector3.zero)
        {
            // calculate the player's direction based on angle
            float tempAngleInputRightJoystick = Mathf.Atan2(rightJoystickY, rightJoystickX);
            rightJoystickX *= Mathf.Abs(Mathf.Cos(tempAngleInputRightJoystick));
            rightJoystickY *= Mathf.Abs(Mathf.Sin(tempAngleInputRightJoystick));

            // rotate the player to face the direction of input
            Vector3 temp = transform.position;
            temp.x += rightJoystickX;
            temp.z += rightJoystickY;
            Vector3 lookDirection = temp - transform.position;

            if (lookDirection != Vector3.zero)
            {
                rotationTarget.localRotation = Quaternion.Slerp(rotationTarget.localRotation, Quaternion.LookRotation(lookDirection) * Quaternion.Euler(0.0f, 55.0f, 0.0f), rotationSpeed * Time.deltaTime);
            }

            // calculate the player's direction based on angle
            float tempAngleLeftJoystick = Mathf.Atan2(leftJoystickY, leftJoystickX);
            leftJoystickX *= Mathf.Abs(Mathf.Cos(tempAngleLeftJoystick));
            leftJoystickY *= Mathf.Abs(Mathf.Sin(tempAngleLeftJoystick));

            leftJoystickInput = new Vector3(leftJoystickX, 0, leftJoystickY);
            leftJoystickInput = transform.TransformDirection(leftJoystickInput);
            leftJoystickInput *= moveSpeed;

            rigidBody.transform.Translate(leftJoystickInput * Time.fixedDeltaTime);

            float tempX = rightJoystickX * -1;
            float tempY = rightJoystickY * -1;

            if (rightJoystickX + rightJoystickY > 0.9f || tempX + tempY > 0.9f || rightJoystickX + tempY > 0.9f || tempX + rightJoystickY > 0.9f)
                Fire();
        }

        if (leftJoystickX != 0.0f || leftJoystickY != 0.0f)
        {
            WalkSound();
        }

        if (isShootOnCooldown)
        {
            shootCooldownTimer += Time.deltaTime;

            if (playerWeapon == Weapon.NormalWeapon)
            {
                if (shootCooldownTimer > shootCooldown)
                {
                    shootCooldownTimer = 0.0f;
                    isShootOnCooldown = false;
                }
            }
            else
            {
                if (shootCooldownTimer > shotgunShootCooldown)
                {
                    shootCooldownTimer = 0.0f;
                    isShootOnCooldown = false;
                }
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
        return ammo[(int)playerWeapon];
    }

    public int GetMaxAmmo()
    {
        return maxAmmo[(int)playerWeapon];
    }

    public int GetHealth()
    {
        return health;
    }

    public void RestartVariables()
    {
        isDead = false;
        animator.SetBool("Dead", isDead);
        health = 100;

        ammo = new int[] { maxAmmoOnNormalWeapon, 0 };
        maxAmmo = new int[] { maxCarryingAmmo, 0 };

        rigidBody.velocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;
        rotationTarget.transform.localPosition = Vector3.zero;
    }

    private void WalkSound()
    {
        if (!isWalkOnCooldown)
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
        if (playerWeapon == Weapon.NormalWeapon)
        {
            if (!isShootOnCooldown && ammo[(int)playerWeapon] > 0)
            {
                if (bulletPoolCounter[(int)playerWeapon] >= bulletPool.Length)
                {
                    bulletPoolCounter[(int)playerWeapon] = 0;
                }

                audioSource.PlayOneShot(weaponSounds[(int)playerWeapon], 0.2f);
                bulletPool[bulletPoolCounter[(int)playerWeapon]].transform.position = bulletSpawn.transform.position;
                bulletPool[bulletPoolCounter[(int)playerWeapon]].transform.rotation = bulletSpawn.transform.rotation;
                bulletPool[bulletPoolCounter[(int)playerWeapon]].SetActive(true);
                bulletPoolCounter[(int)playerWeapon]++;
                ammo[(int)playerWeapon]--;

                isShootOnCooldown = true;
            }
        }
        else
        {
            if (!isShootOnCooldown && ammo[(int)playerWeapon] > 0)
            {
                if (bulletPoolCounter[(int)playerWeapon] >= bulletPoolShotgun.Length)
                {
                    bulletPoolCounter[(int)playerWeapon] = 0;
                }

                audioSource.PlayOneShot(weaponSounds[(int)playerWeapon], 0.25f);
                bulletPoolShotgun[bulletPoolCounter[(int)playerWeapon]].transform.position = bulletSpawn.transform.position;
                bulletPoolShotgun[bulletPoolCounter[(int)playerWeapon]].transform.rotation = bulletSpawn.transform.rotation;
                bulletPoolShotgun[bulletPoolCounter[(int)playerWeapon]].SetActive(true);
                bulletPoolCounter[(int)playerWeapon]++;
                ammo[(int)playerWeapon]--;

                isShootOnCooldown = true;
            }
        }
    }

    private void Reload()
    {
        if (playerWeapon == Weapon.NormalWeapon)
        {
            if (!isShootOnCooldown && ammo[(int)playerWeapon] <= 0 && maxAmmo[(int)playerWeapon] > 0)
            {
                if (playReloadSound)
                {
                    audioSource.PlayOneShot(reloadSound, 0.5f);
                    playReloadSound = false;
                }

                shootCooldownTimer += Time.deltaTime;

                if (shootCooldownTimer > reloadTime)
                {
                    if (maxAmmo[(int)playerWeapon] - maxAmmoOnNormalWeapon >= 0)
                    {
                        maxAmmo[(int)playerWeapon] -= maxAmmoOnNormalWeapon;
                        ammo[(int)playerWeapon] = maxAmmoOnNormalWeapon;
                    }
                    else
                    {
                        ammo[(int)playerWeapon] = maxAmmo[(int)playerWeapon];
                        maxAmmo[(int)playerWeapon] -= maxAmmo[(int)playerWeapon];
                    }

                    shootCooldownTimer = 0.0f;
                    playReloadSound = true;
                }
            }
        }
        else
        {
            if (ammo[(int)playerWeapon] <= 0)
            {
                shotgun.SetActive(false);
                rifle.SetActive(true);
                playerWeapon = Weapon.NormalWeapon;
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
            isDead = true;
            animator.SetBool("Dead", isDead);
            GameControllerBehaviour.gameControllerInstance.ShowEndScreen();
        }
    }

    public void AddAmmo(int p_ammo)
    {
        maxAmmo[0] += p_ammo;
    }

    public void AddHealth(int p_health)
    {
        health += p_health;
    }

    public void ChangeWeapon(int p_weaponID, int p_specialAmmo)
    {
        if (p_weaponID == 1)
        {
            playerWeapon = Weapon.Shotgun;
            ammo[p_weaponID] = p_specialAmmo;
            rifle.SetActive(false);
            shotgun.SetActive(true);
        }
    }

    private IEnumerator PlayDamageSoundWithDelay()
    {
        yield return new WaitForSeconds(0.2f);
        audioSource.PlayOneShot(damageSounds[Random.Range(0, damageSounds.Length)], 0.7f);
    }

    private void OnTriggerEnter(Collider collider)
    {
        if(collider.CompareTag("Death Zone") && GameControllerBehaviour.gameControllerInstance.GetGameState() == 1)
        {
            health = 0;
            isDead = true;
            animator.SetBool("Dead", isDead);
            GameControllerBehaviour.gameControllerInstance.ShowEndScreen();
        }
    }
}