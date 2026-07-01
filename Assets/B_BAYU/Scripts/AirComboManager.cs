using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class AirComboManager : MonoBehaviour
{

    [HideInInspector]
    public AnimalData myAnimalData;

    [Header("Pengaturan Waktu dan Efek")]
    public float qteDuration = 5f;
    public float slowMotionScale = 0.2f;

    public enum ComboButton { A, B, X, Y }

    [Header("UrutanCombo")]
    //public int comboLength = 4;
    private ComboButton[] comboSequence;

    [Header("Pengaturan UI Combo")]
    public Canvas qteCanvas;
    public Transform uiContainer;
    public GameObject iconPrefab;

    [Header("Pengaturan Animasi Gaya Hewan")]
    public SpriteRenderer animalSpriteRenderer; // Komponen visual hewan
    //public Sprite defaultSprite;                // Gambar normal hewan saat ditarik/terbang biasa
    //public Sprite[] comboPoses;                 // Daftar gambar pose lucu yang berurutan

    [Header("Pengaturan Kamera")]
    public GameObject zoomCamera; // Objek Cinemachine Virtual Camera untuk Zoom

    [Header("Sprite Ikon Tombol Gamepad")]
    public Sprite spriteA, spriteB, spriteX, spriteY;

    private int currentComboIndex = 0;
    private int maxComboScore;
    private int currentScore;

    private bool isQteActive = false;
    private float timer = 0f;

    private List<Image> spawnedIcons = new List<Image>();

    // Start is called before the first frame update
    void Start()
    {

    }
    // Update is called once per frame
    void Update()
    {
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
        if (Gamepad.current == null) return;

        bool pressA = Gamepad.current.buttonSouth.wasPressedThisFrame;
        bool pressB = Gamepad.current.buttonEast.wasPressedThisFrame;
        bool pressX = Gamepad.current.buttonWest.wasPressedThisFrame;
        bool pressY = Gamepad.current.buttonNorth.wasPressedThisFrame;

        if (!pressA && !pressB && !pressX && !pressY) return;

        ComboButton expectedButton = comboSequence[currentComboIndex];
        bool isCorrect = false;

        if (expectedButton == ComboButton.A && pressA) isCorrect = true;
        else if (expectedButton == ComboButton.B && pressB) isCorrect = true;
        else if (expectedButton == ComboButton.X && pressX) isCorrect = true;
        else if (expectedButton == ComboButton.Y && pressY) isCorrect = true;

        if (isCorrect)
        {
            Debug.Log("BENAR! Lanjut ke tombol berikutnya.");
            // Ubah warna ikon yang berhasil menjadi Hijau
            spawnedIcons[currentComboIndex].color = new Color(0.3f, 1f, 0.3f, 0.8f);

            if(animalSpriteRenderer != null && currentComboIndex < myAnimalData.comboPoses.Length)
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
            // Ubah warna ikon yang gagal menjadi Merah
            spawnedIcons[currentComboIndex].color = new Color(1f, 0.3f, 0.3f, 0.8f);
        }

        // KUNCI PERUBAHAN: Index SELALU bertambah, baik saat benar maupun salah
        currentComboIndex++;

        // Perbarui highlight (pembesaran ukuran) untuk tombol target selanjutnya
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

        qteCanvas.gameObject.SetActive(false);

        if (zoomCamera != null)
        {
            zoomCamera.SetActive(false);
        }
        if(qteCanvas != null)
        {
            qteCanvas.gameObject.SetActive(false);
        }

        Debug.Log("QTE Selesai, Skor: " + currentScore);

        TurnManager.instance.FinishActionAndSwitchTurn();
    }


    //LOGIKA & PEMBUATAN UI

    private void GenerateUI()
    {
        foreach(Image icon in spawnedIcons)
        {
            Destroy(icon.gameObject);
        }
        spawnedIcons.Clear();

        foreach(ComboButton btn in comboSequence)
        {
            GameObject newIcon = Instantiate(iconPrefab, uiContainer);
            Image img = newIcon.GetComponent<Image>();

            switch(btn)
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

            // Status 1: Tombol TARGET SAAT INI (Di-highlight / dibesarkan)
            if (i == currentComboIndex)
            {
                img.color = Color.white;
                img.transform.localScale = Vector3.one * 1.3f;
            }
            // Status 2: Tombol BERIKUTNYA (Belum gilirannya)
            else if (i > currentComboIndex)
            {
                img.color = new Color(1f, 1f, 1f, 0.7f);
                img.transform.localScale = Vector3.one;
            }
            // Status 3: Tombol yang SUDAH DITEKAN (Ukurannya dikecilkan kembali)
            // (Warnanya tidak perlu diubah lagi karena sudah diatur jadi hijau/merah di CheckQTEInput)
            else
            {
                img.transform.localScale = Vector3.one * 0.8f;
            }
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        TowerHealth targetTower = collision.gameObject.GetComponent<TowerHealth>();

        if (targetTower != null)
        {
            float comboMultiplier = 1f + (currentScore * 0.5f);
            float finalDamage = myAnimalData.baseDamage + comboMultiplier;

            Debug.Log("BAM! " + myAnimalData.animalName + " menabrak Tower!");
            Debug.Log("Base Damage: " + myAnimalData.baseDamage + " | Multiplier: x" + comboMultiplier + " | Final Damage: " + finalDamage);

            targetTower.TakeDamage(finalDamage);
        }

        // Jika hewan menabrak objek apa pun (tanah, tower, hewan lain) saat QTE masih berjalan
        if (isQteActive)
        {
            Debug.Log("Hewan mendarat! Waktu lambat otomatis dihentikan.");
            EndQTE(); // Panggil fungsi ini untuk mengembalikan waktu menjadi normal seketika
        }
    }
}
