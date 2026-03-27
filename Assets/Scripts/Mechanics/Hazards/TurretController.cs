using FMOD.Studio;
using UnityEngine;

/// <summary>
/// Written by Matthew Glos
/// 
/// Controls turret behavior, including static and tracking variants.
/// - Static turrets fire on a fixed timer.
/// - Tracking turrets raycast to detect and rotate toward a player target.
/// </summary>
public class TurretController : MonoBehaviour
{
    /// <summary>
    /// The type of turret behavior.
    /// </summary>
    public enum TurretType
    {
        STATIC_TURRET,
        TRACKING_TURRET
    }

    [Header("Turret Info")]
    [SerializeField] private TurretType turretType = TurretType.STATIC_TURRET;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool debug = false;

    [Header("Tracking Turret Raycast Settings")]
    [SerializeField] private int rayCastCount = 7;          // Number of rays in a spread
    [SerializeField] private float rayCastSpread = 45f;     // Total spread angle (degrees)
    [SerializeField] private float rayCastDistance = 10f;   // Max distance of detection

    [Header("General Turret Settings")]
    [SerializeField] private float bulletTimer = 1.5f;      // Time between shots
    [SerializeField] private float bulletSpeed = 10f;       // Speed of fired bullets

    [Header("Tracking Turret Settings")]
    [SerializeField] private bool onlyShootWhenForward = false;  // Restrict shooting unless player is in front
    [SerializeField] private LayerMask layermask;                // Layers the turret can detect
    [SerializeField] private float trackSpeed = 5f;              // How quickly the turret tracks the player
    [SerializeField] private float returnSpeed = 2f;             // How quickly the turret returns to idle rotation
    [SerializeField] private float waitTime = 2f;                // Time before returning to idle rotation after losing sight

    // --- Private runtime variables ---
    private Vector3 initialRotation;     // The starting rotation of the turret (used to return to idle)
    private float shootTimer = 0f;       // Timer for bullet firing
    private float waitTimer = 0f;        // Timer for returning to idle rotation
    private bool ready = true;           // bool to only track once the turret resets

    private void Start()
    {
        // Store the turret's initial rotation for resetting
        initialRotation = transform.rotation.eulerAngles;
    }

    private void Update()
    {
        // Update shoot timer each frame
        shootTimer += Time.deltaTime;

        switch (turretType)
        {
            case TurretType.STATIC_TURRET:
                HandleStaticTurret();
                break;

            case TurretType.TRACKING_TURRET:
                HandleTrackingTurret();
                break;
        }
    }

    // -------------------------------
    // STATIC TURRET LOGIC
    // -------------------------------
    /// <summary>
    /// Static turret fires automatically on a fixed timer.
    /// </summary>
    private void HandleStaticTurret()
    {
        if (shootTimer > bulletTimer)
        {
            shootTimer = 0f;
            Shoot();
        }
    }

    // -------------------------------
    // TRACKING TURRET LOGIC
    // -------------------------------
    /// <summary>
    /// Tracking turret performs raycasts to detect a player,
    /// rotates toward them, and optionally fires when in range and facing them.
    /// </summary>
    private void HandleTrackingTurret()
    {
        bool foundPlayer = false;
        bool playerForward = false;

        // Find the player by tag
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null)
            return;

        // Perform a raycast sweep to check for the player
        for (int i = 0; i < rayCastCount; i++)
        {
            // Compute direction of this ray within the spread
            Vector2 direction = transform.up;
            direction = RotateVector(direction, ((rayCastCount / 2f) - i) * (rayCastSpread / rayCastCount));

            // Raycast using layermask
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, rayCastDistance, layermask);
            if (hit)
            {
                bool hitPlayer = hit.collider.CompareTag(playerTag) && hit.distance < rayCastDistance;
                if (hitPlayer)
                    foundPlayer = true;

                // Check if the *middle ray* (forward one) hit the player
                if (i == (int)(rayCastCount / 2f))
                    playerForward = hitPlayer;

                // Optional debug visualization
                if (debug)
                {
                    Color color = hitPlayer ? Color.red : Color.green;
                    Debug.DrawRay(transform.position, direction * Mathf.Min(hit.distance, rayCastDistance), color);
                }
            }
            else
            {
                //if there's not collision, just draw a line of the length of max raycast distance
                if (debug)
                {
                    Color color = Color.green;
                    Debug.DrawRay(transform.position, direction*rayCastDistance, color);
                }
            }
        }

        // --- ROTATION & SHOOTING LOGIC ---

        // Always rotate toward player position (even if not yet found)
        Vector2 toPlayer = player.transform.position - transform.position;
        float facingAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;

        // Shoot if timer expired and either:
        // - forward-only shooting is disabled, OR
        // - the player is in front
        if (shootTimer > bulletTimer && (!onlyShootWhenForward || playerForward))
        {
            shootTimer = 0f;
            Shoot();
        }

        if (foundPlayer && ready)
        {
            // Player detected — rotate toward them
            waitTimer = 0f;
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, facingAngle - 90f);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, trackSpeed * Time.deltaTime);
        }
        else
        {
            // Player not found — wait, then return to idle rotation
            waitTimer += Time.deltaTime;
            if (waitTimer > waitTime)
            {

                ready = false;
                Quaternion idleRotation = Quaternion.Euler(0f, 0f, initialRotation.z);
                transform.rotation = Quaternion.Lerp(transform.rotation, idleRotation, returnSpeed * Time.deltaTime);
            }

            if (Mathf.Abs(transform.localEulerAngles.z-initialRotation.z)<5f) ready = true;
        }
    }

    // -------------------------------
    // UTILITY METHODS
    // -------------------------------

    /// <summary>
    /// Rotates a 2D vector by a given number of degrees.
    /// </summary>
    public static Vector2 RotateVector(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(
            v.x * cos - v.y * sin,
            v.x * sin + v.y * cos
        );
    }

    /// <summary>
    /// Spawns and configures a bullet prefab.
    /// </summary>
    private void Shoot()
    {
        if (bulletPrefab == null) return;

        GameObject bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
        BulletController bulletController = bullet.GetComponent<BulletController>();
        bulletController.speed = bulletSpeed;
    }
}
