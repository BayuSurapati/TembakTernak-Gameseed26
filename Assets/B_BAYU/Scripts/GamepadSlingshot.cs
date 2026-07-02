using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using UnityEngine.InputSystem; -> DIHAPUS karena sudah diurus oleh PlayerInputHandler!

public class GamepadSlingshot : MonoBehaviour
{
    [Header("Pengaturan Visual Meriam")]
    public Sprite readySprite;
    public Sprite shootSprite;

    private SpriteRenderer cannonRenderer;
    private Sprite idleSprite;
    private Quaternion originalCannonRotation;

    [Header("Referensi Objek")]
    public Rigidbody2D animalRB;
    public Transform slingshotCenter;

    [Header("Pengaturan Ketapel")]
    public float powerMultiplier = 10f;
    public float maxDragDistance = 3f;

    [Header("Identitas Pemain")]
    public TurnManager.TurnPhase ownerPhase;

    private bool isPulling = false;
    private Vector2 startPoint;

    void Start()
    {
        startPoint = slingshotCenter.position;
        animalRB.isKinematic = true;

        if (slingshotCenter != null)
        {
            cannonRenderer = slingshotCenter.GetComponent<SpriteRenderer>();

            if (cannonRenderer != null)
            {
                idleSprite = cannonRenderer.sprite;
                originalCannonRotation = cannonRenderer.transform.rotation;
            }
        }
    }

    void Update()
    {
        // 1. PENGAMAN: Jika bukan giliran pemain ini, jangan lakukan apa-apa
        // (Catatan: Pastikan di TurnManager kamu menggunakan huruf I besar pada 'Instance')
        if (TurnManager.instance == null || TurnManager.instance.currentPhase != ownerPhase)
        {
            return;
        }

        // 2. Beritahu InputHandler posisi titik meriam kita saat ini agar kalkulasi Mouse akurat
        if (PlayerInputHandler.Instance != null && slingshotCenter != null)
        {
            PlayerInputHandler.Instance.currentSlingshotCenter = this.slingshotCenter;
        }

        // 3. Ambil status input dari Sang Penerjemah
        bool inputIsHolding = PlayerInputHandler.Instance.isHoldingAim;

        // Fase Mulai Ditarik (Klik/Trigger ditekan pertama kali)
        if (inputIsHolding && !isPulling)
        {
            StartPulling();
        }

        // Fase Sedang Ditarik (Klik/Trigger ditahan)
        if (inputIsHolding && isPulling)
        {
            ProcessAiming();
        }

        // Fase Dilepas / Menembak (Klik/Trigger dilepas)
        if (!inputIsHolding && isPulling)
        {
            ShootProjectile();
        }
    }

    private void StartPulling()
    {
        isPulling = true;
        animalRB.isKinematic = true;
        animalRB.velocity = Vector2.zero;
    }

    private void ProcessAiming()
    {
        // Kunci posisi hewan agar tetap di dalam laras meriam
        transform.position = slingshotCenter.position;

        // Ganti visual meriam bersiap
        if (cannonRenderer != null && readySprite != null)
        {
            cannonRenderer.sprite = readySprite;
        }

        // Mengambil arah tarikan dari InputHandler
        Vector2 pullDir = PlayerInputHandler.Instance.aimDirection;

        // Memutar arah laras meriam (arah kebalikan dari tarikan)
        Vector2 cannonAimDir = -pullDir;

        if (cannonAimDir.sqrMagnitude > 0.1f)
        {
            float angle = Mathf.Atan2(cannonAimDir.y, cannonAimDir.x) * Mathf.Rad2Deg;

            // Logika pembatasan rotasi meriam (Kiri dan Kanan)
            if (slingshotCenter.position.x < 0)
            {
                angle = Mathf.Clamp(angle, -15f, 85f);
            }
            else
            {
                if (angle < 0) angle += 360f;
                angle = Mathf.Clamp(angle, 95f, 195f);
            }
            cannonRenderer.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private IEnumerator ResetCannonVisual()
    {
        yield return new WaitForSeconds(0.5f); // Tahan gambar meledak selama 0.5 detik

        if (cannonRenderer != null && idleSprite != null)
        {
            cannonRenderer.sprite = idleSprite; // Kembalikan ke gambar normal
            cannonRenderer.transform.rotation = originalCannonRotation; // Luruskan meriam
        }
    }

    private void ShootProjectile()
    {
        isPulling = false;
        animalRB.isKinematic = false;

        // --- KALKULASI TEMBAKAN ---
        // Ambil seberapa jauh pemain menarik mouse/analog
        Vector2 pullVector = PlayerInputHandler.Instance.aimDirection;

        // Batasi maksimal kekuatan (max drag)
        if (pullVector.magnitude > maxDragDistance)
        {
            pullVector = pullVector.normalized * maxDragDistance;
        }

        // Tembakan adalah kebalikan dari arah tarik (pullVector)
        Vector2 shootDirection = -pullVector.normalized;
        float shootDistance = pullVector.magnitude;

        // Tembak!
        animalRB.AddForce(shootDirection * shootDistance * powerMultiplier, ForceMode2D.Impulse);

        // Efek Ledakan Meriam
        if (cannonRenderer != null && shootSprite != null)
        {
            cannonRenderer.sprite = shootSprite;
            StartCoroutine(ResetCannonVisual());
        }

        // Mulai QTE dan Pindah Fase ke Udara
        if (GetComponent<AirComboManager>() != null)
        {
            GetComponent<AirComboManager>().StartQTE();
        }

        TurnManager.instance.SetAirbornePhase();
    }
}