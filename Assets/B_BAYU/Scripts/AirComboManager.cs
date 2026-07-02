using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AirComboManager : MonoBehaviour
{
    [HideInInspector]
    public AnimalData myAnimalData;

    [Header("Pengaturan Waktu dan Efek")]
    public float qteDuration = 5f;
    public float slowMotionScale = 0.2f;

    public enum ComboButton { A, B, X, Y }

    [Header("UrutanCombo")]
    private ComboButton[] comboSequence;

    [Header("Pengaturan UI Combo")]
    public Canvas qteCanvas;
    public Transform uiContainer;
    public GameObject iconPrefab;

    [Header("Pengaturan Animasi Gaya Hewan")]
    public SpriteRenderer animalSpriteRenderer;

    [Header("Pengaturan Kamera")]
    public GameObject zoomCamera;

    [Header("Sprite Ikon Tombol Gamepad")]
    public Sprite spriteA, spriteB, spriteX, spriteY;

    private int currentComboIndex = 0;
    private int maxComboScore;
    private int currentScore;

    private float safeFlightTimer = 0f;

    private bool isQteActive = false;
    private float timer = 0f;

    private List<Image> spawnedIcons = new List<Image>();

    void Start()
    {

    }

    void Update()
    {
        if (isQteActive)
        {
            safeFlightTimer += Time.unscaledDeltaTime;
        }

        if (!isQteActive) return;

        timer -= Time.unscaledDeltaTime;

        if (timer <= 0)
        {
            EndQTE();
            return;
        }

        if (currentComboIndex < comboSequence.Length)
        {
            CheckQTEInput();
        }
        else
        {
            EndQTE();
        }
    }

    public void StartQTE()
    {
        isQteActive = true;
        timer = qteDuration;
        safeFlightTimer = 0f;
        currentComboIndex = 0;

        if (zoomCamera != null)
        {
            zoomCamera.SetActive(true);
        }

        int comboLength = myAnimalData.comboLength;
        comboSequence = new ComboButton[comboLength];
        for (int i = 0; i < comboLength; i++)
        {
            comboSequence[i] = (ComboButton)Random.Range(0, 4);
        }

        maxComboScore = comboSequence.Length;
        currentScore = maxComboScore;

        Time.timeScale = slowMotionScale;
        Time.fixedDeltaTime = 0.02f * slowMotionScale;

        if (animalSpriteRenderer != null && myAnimalData.defaultSprite != null)
        {
            animalSpriteRenderer.sprite = myAnimalData.defaultSprite;
        }

        qteCanvas.gameObject.SetActive(true);
        GenerateUI();
        UpdateUIHighlight();

        Debug.Log("QTE Dimulai!");
    }

    private void CheckQTEInput()
    {
        // PENGAMAN: Pastikan InputHandler sudah ada di Scene
        if (PlayerInputHandler.Instance == null) return;

        // Mengecek apakah ada tombol QTE APA SAJA yang ditekan frame ini
        if (!PlayerInputHandler.Instance.IsAnyQTEKeyPressed()) return;

        // --- LOGIKA BARU: Menerjemahkan Input dari PlayerInputHandler ---
        // Asumsi tata letak tombol standar (Keyboard / Gamepad):
        // Tombol A (Bawah) = S / Button South
        // Tombol B (Kanan) = D / Button East
        // Tombol X (Kiri)  = A / Button West
        // Tombol Y (Atas)  = W / Button North

        bool pressA = PlayerInputHandler.Instance.IsQTEDownPressed();
        bool pressB = PlayerInputHandler.Instance.IsQTERightPressed();
        bool pressX = PlayerInputHandler.Instance.IsQTELeftPressed();
        bool pressY = PlayerInputHandler.Instance.IsQTEUpPressed();

        ComboButton expectedButton = comboSequence[currentComboIndex];
        bool isCorrect = false;

        if (expectedButton == ComboButton.A && pressA) isCorrect = true;
        else if (expectedButton == ComboButton.B && pressB) isCorrect = true;
        else if (expectedButton == ComboButton.X && pressX) isCorrect = true;
        else if (expectedButton == ComboButton.Y && pressY) isCorrect = true;

        if (isCorrect)
        {
            Debug.Log("BENAR! Lanjut ke tombol berikutnya.");
            spawnedIcons[currentComboIndex].color = new Color(0.3f, 1f, 0.3f, 0.8f);

            if (animalSpriteRenderer != null && currentComboIndex < myAnimalData.comboPoses.Length)
            {
                if (myAnimalData.comboPoses[currentComboIndex] != null)
                {
                    animalSpriteRenderer.sprite = myAnimalData.comboPoses[currentComboIndex];
                }
            }
        }
        else
        {
            Debug.Log("SALAH TEKAN! Hangus, lanjut ke tombol berikutnya.");
            if (currentScore > 0) currentScore--;
            spawnedIcons[currentComboIndex].color = new Color(1f, 0.3f, 0.3f, 0.8f);
        }

        currentComboIndex++;
        UpdateUIHighlight();
    }

    private void EndQTE()
    {
        if (!isQteActive)
        {
            return;
        }
        isQteActive = false;

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        if (qteCanvas != null)
        {
            qteCanvas.gameObject.SetActive(false);
        }

        if (zoomCamera != null)
        {
            zoomCamera.SetActive(false);
        }

        Debug.Log("QTE Selesai, Skor: " + currentScore);

        if (TurnManager.instance != null)
        {
            TurnManager.instance.FinishActionAndSwitchTurn();
        }
    }

    private void GenerateUI()
    {
        foreach (Image icon in spawnedIcons)
        {
            Destroy(icon.gameObject);
        }
        spawnedIcons.Clear();

        foreach (ComboButton btn in comboSequence)
        {
            GameObject newIcon = Instantiate(iconPrefab, uiContainer);
            Image img = newIcon.GetComponent<Image>();

            switch (btn)
            {
                case ComboButton.A: img.sprite = spriteA; break;
                case ComboButton.B: img.sprite = spriteB; break;
                case ComboButton.X: img.sprite = spriteX; break;
                case ComboButton.Y: img.sprite = spriteY; break;
            }
            spawnedIcons.Add(img);
        }
    }

    private void UpdateUIHighlight()
    {
        for (int i = 0; i < spawnedIcons.Count; i++)
        {
            Image img = spawnedIcons[i];

            if (i == currentComboIndex)
            {
                img.color = Color.white;
                img.transform.localScale = Vector3.one * 1.3f;
            }
            else if (i > currentComboIndex)
            {
                img.color = new Color(1f, 1f, 1f, 0.7f);
                img.transform.localScale = Vector3.one;
            }
            else
            {
                img.transform.localScale = Vector3.one * 0.8f;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isQteActive && safeFlightTimer < 0.2f)
        {
            return;
        }

        TowerHealth targetTower = collision.gameObject.GetComponent<TowerHealth>();

        if (targetTower != null)
        {
            if (TurnManager.instance != null && targetTower.towerOwner == TurnManager.instance.currentPhase)
            {
                Debug.Log("Ups! Terkena markas sendiri. Diabaikan!");
                return;
            }

            float comboMultiplier = 1f + (currentScore * 0.5f);

            // PERBAIKAN: Mengganti (+) menjadi (*) agar multiplier berfungsi dengan benar
            float finalDamage = myAnimalData.baseDamage * comboMultiplier;

            Debug.Log("BAM! " + myAnimalData.animalName + " menabrak Tower!");
            Debug.Log("Base Damage: " + myAnimalData.baseDamage + " | Multiplier: x" + comboMultiplier + " | Final Damage: " + finalDamage);

            targetTower.TakeDamage(finalDamage);
        }
        else
        {
            Debug.Log("Meleset! Hewan hanya menabrak tanah/rintangan.");
        }

        if (isQteActive)
        {
            Debug.Log("Hewan mendarat! Waktu lambat otomatis dihentikan.");
            EndQTE();
        }
    }
}