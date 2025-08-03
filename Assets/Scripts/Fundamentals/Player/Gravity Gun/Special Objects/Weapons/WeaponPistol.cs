using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/* -----------------------------------------------------------
 * Author:
 * Ian Fletcher
 * 
 * Modified By:
 * 
 */// --------------------------------------------------------

/// <summary>
/// Class that handles the pistol weapon of the game
/// </summary>
public class WeaponPistol : GravSpecialObject
{
    [Header("References")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _projectileSpawnPoint;
    [SerializeField] private TextMeshProUGUI _projectileAmmoText;
    
    [Header("Settings")]
    [SerializeField] private int _projectileMaxAmmoCount = 15;
    [SerializeField] private float _cooldownTime = 0.5f;
    
    [Header("Debugging")]
    [SerializeField] private bool _doDebugLog;
    
    [Header("Readouts")]
    [SerializeField, InspectorReadOnly] private bool _isOnCooldown;
    [SerializeField, InspectorReadOnly] private int _currentAmmoCount = 0;
    
    void Start()
    {
        // Add Listener to event
        gravEvents.onGravityGunSpecialTriggered.AddListener(FireProjectile);
        
        // Set max ammo
        _currentAmmoCount = _projectileMaxAmmoCount;
        
        // Set ammo text
        _projectileAmmoText.text = _currentAmmoCount.ToString();
    }

    /// <summary>
    /// Function that fires the gun
    /// </summary>
    private void FireProjectile()
    {
        // Dont fire if on cooldown
        if (_isOnCooldown) return;
        
        // Dont fire if there's no ammo
        if (_currentAmmoCount <= 0) return;
        
        // Else, spawn projectile
        Quaternion rotation = _projectileSpawnPoint.rotation * Quaternion.Euler(0f, 0f, -90f);
        GameObject projectile = Instantiate(_projectilePrefab, _projectileSpawnPoint.position, rotation);
        
        // Make sure our collider doesnt hit the projectile collider
        Collider2D projectileCollider = projectile.GetComponent<Collider2D>();
        Physics2D.IgnoreCollision(collider, projectileCollider, true);
        
        // Lower ammo
        _currentAmmoCount--;
        
        // Set ammo text
        _projectileAmmoText.text = _currentAmmoCount.ToString();
        
        // Trigger cooldown
        StartCoroutine(CooldownRoutine());
    }
    
    /// <summary>
    /// Coroutine that handles the in-between shooting cooldown
    /// </summary>
    /// <returns></returns>
    private IEnumerator CooldownRoutine()
    {
        _isOnCooldown = true;
        yield return new WaitForSeconds(_cooldownTime);
        _isOnCooldown = false;
    }
}
