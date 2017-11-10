using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MobilePlayerBehaviour : MonoBehaviour
{
    public int rotationSpeed = 0;
    public float moveSpeed = 0.0f;
    public float bulletSpeed = 0.0f;
    public float shootCooldown = 0.0f;
    public GameObject bulletPrefab = null;
    public GameObject bulletSpawn = null;
    public LeftJoystick leftJoystick = null;
    public RightJoystick rightJoystick = null;
    public Transform rotationTarget = null;
    //public Animator animator = null;

    private bool isShootOnCooldown = false;
    private float shootCooldownTimer = 0.0f;
    private float leftJoystickX = 0.0f;
    private float leftJoystickY = 0.0f;
    private float rightJoystickX = 0.0f;
    private float rightJoytsickY = 0.0f;
    private Vector3 leftJoystickInput = Vector3.zero;
    private Vector3 rightJoystickInput = Vector3.zero;
    private Rigidbody rigidBody = null; 

    void Start()
    {
        StartStuff();
    }

    void FixedUpdate()
    {
        GetInputs();
        DoActions();
    }

    private void StartStuff()
    {
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
    }

    public void Fire()
    {
        if (!isShootOnCooldown)
        {
            GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.transform.position, bulletSpawn.transform.rotation) as GameObject;
            bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * bulletSpeed;
            isShootOnCooldown = true;
            Destroy(bullet, 2.0f);
        }
        else
        {
            shootCooldownTimer += Time.deltaTime;

            if(shootCooldownTimer > shootCooldown)
            {
                shootCooldownTimer = 0.0f;
                isShootOnCooldown = false;
            }
        }
    }
}