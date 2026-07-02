using UnityEngine;
using UnityEngine.InputSystem; // Wajib untuk New Input System

public class PlayerInputHandler : MonoBehaviour
{
    public static PlayerInputHandler Instance;

    private GameControls controls; // Mengambil dari C# Class yang di-generate tadi

    [Header("Data Output Tembakan")]
    public bool isHoldingAim;
    public Vector2 aimDirection;

    // Titik pusat meriam yang sedang aktif (akan diisi otomatis oleh meriam)
    [HideInInspector] public Transform currentSlingshotCenter;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        controls = new GameControls();
    }

    // Wajib menyalakan dan mematikan deteksi input
    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void Update()
    {
        // 1. Membaca apakah pemain sedang menahan tombol tembak (Klik Kiri / Trigger R2)
        isHoldingAim = controls.PlayerControls.Pull.IsPressed();

        // 2. Kalkulasi Arah Tarikan secara otomatis
        if (isHoldingAim && currentSlingshotCenter != null)
        {
            // Cek Gamepad dulu (Apakah analog kiri sedang digerakkan?)
            Vector2 gamepadInput = controls.PlayerControls.GamepadAim.ReadValue<Vector2>();

            if (gamepadInput.sqrMagnitude > 0.1f)
            {
                // Gunakan arah dari Controller
                aimDirection = gamepadInput;
            }
            else
            {
                // Jika tidak ada input gamepad, gunakan kalkulasi Mouse (Gaya Tarik Belakang)
                Vector2 mouseScreenPos = controls.PlayerControls.MouseAim.ReadValue<Vector2>();
                if (Camera.main != null)
                {
                    // Ubah koordinat layar pixel ke koordinat dunia 2D
                    Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, 0));
                    mouseWorldPos.z = 0;

                    // Arah ditarik dari titik tengah meriam ke kursor mouse
                    aimDirection = (mouseWorldPos - currentSlingshotCenter.position);
                }
            }
        }
    }

    // --- FUNGSI PUBLIK UNTUK SISTEM QTE (Bisa dipanggil dari luar) ---
    public bool IsQTEUpPressed() => controls.PlayerControls.QTE_Up.triggered;
    public bool IsQTEDownPressed() => controls.PlayerControls.QTE_Down.triggered;
    public bool IsQTELeftPressed() => controls.PlayerControls.QTE_Left.triggered;
    public bool IsQTERightPressed() => controls.PlayerControls.QTE_Right.triggered;

    // Mendeteksi apakah pemain memencet salah satu tombol QTE (Bisa untuk deteksi salah pencet)
    public bool IsAnyQTEKeyPressed()
    {
        return IsQTEUpPressed() || IsQTEDownPressed() || IsQTELeftPressed() || IsQTERightPressed();
    }
}