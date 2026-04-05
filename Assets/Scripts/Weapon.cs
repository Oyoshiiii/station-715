using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Weapon : MonoBehaviour
{
    [SerializeField] private float damage = 25f;
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private float range = 100f;

    [SerializeField] private int maxAmmo = 12;
    [SerializeField] private int currentAmmo;
    [SerializeField] private float reloadTime = 1.5f;

    [SerializeField] private LineRenderer laserLine;
    [SerializeField] private Transform gunTip;

    private float nextFireTime;

    private Camera mainCamera;

    public event EventHandler OnAmmoChanged;
    public event EventHandler OnReloadStarted;
    public event EventHandler OnReloadCompleted;

    public int CurrentAmmo {get { return currentAmmo;}}
    public int MaxAmmo { get { return maxAmmo;}}
    public bool IsReloading;
    public bool IsAiming;

    private void Awake()
    {
        mainCamera = Camera.main;
        currentAmmo = maxAmmo;

        SetupLaser();

        if (laserLine != null)
            laserLine.enabled = false;
    }

    private void Update()
    {
        if (IsReloading) return;

        HandleLaser();
    }

    private void HandleLaser()
    {
        if (IsAiming && mainCamera != null && gunTip != null)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, range))
            {
                Vector3 aimPoint = hit.point;

                if (laserLine != null)
                {
                    laserLine.enabled = true;
                    laserLine.SetPosition(0, gunTip.position);
                    laserLine.SetPosition(1, aimPoint);
                }
            }
            else
            {
                Vector3 farPoint = ray.GetPoint(range);
                if (laserLine != null)
                {
                    laserLine.enabled = true;
                    laserLine.SetPosition(0, gunTip.position);
                    laserLine.SetPosition(1, farPoint);
                }
            }
        }
        else
        {
            if (laserLine != null && laserLine.enabled)
                laserLine.enabled = false;
        }
    }

    private void SetupLaser()
    {
        if (laserLine == null && gunTip != null)
        {
            GameObject laserObj = new GameObject("Laser");
            laserObj.transform.SetParent(gunTip);

            laserLine = laserObj.AddComponent<LineRenderer>();
        }

        if (laserLine != null)
        {
            laserLine.startWidth = 0.03f;
            laserLine.endWidth = 0.03f;
            laserLine.positionCount = 2;
            laserLine.material = new Material(Shader.Find("Sprites/Default"));
            laserLine.startColor = Color.red;
            laserLine.endColor = Color.red;
            laserLine.useWorldSpace = true;
        }
    }

    public void Shoot()
    {
        if (IsReloading) return;
        if (!IsAiming) return;
        if (currentAmmo <= 0)
        {
            currentAmmo = 0;
            Debug.Log("Патроны кончились");
            return;
        }
        if (Time.time < nextFireTime) return;

        nextFireTime = Time.time + fireRate;

        currentAmmo--;

        Debug.Log("Осталось патронов " + currentAmmo);

        OnAmmoChanged?.Invoke(this, EventArgs.Empty);

        if (mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            Enemy enemy = hit.collider.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                Vector3 hitDirection = (hit.point - gunTip.position).normalized;
                enemy.TakeDamage(damage, hitDirection);
            }
        }
    }

    public void StartAiming(bool aiming)
    {
        IsAiming = aiming;
    }

    public void StartReload()
    {
        if (!IsReloading && currentAmmo < maxAmmo)
            StartCoroutine(ReloadCoroutine());
    }

    private IEnumerator ReloadCoroutine()
    {
        IsReloading = true;

        OnReloadStarted?.Invoke(this, EventArgs.Empty);

        Debug.Log("Перезарядка");

        yield return new WaitForSeconds(reloadTime);

        Debug.Log("Конец перезарядки");

        currentAmmo = maxAmmo;
        IsReloading = false;

        OnReloadCompleted?.Invoke(this, EventArgs.Empty);
        OnAmmoChanged?.Invoke(this, EventArgs.Empty);
    }

    public void AddAmmo(int amount)
    {
        currentAmmo = Mathf.Min(currentAmmo + amount, maxAmmo);
        OnAmmoChanged?.Invoke(this, EventArgs.Empty);
    }
}