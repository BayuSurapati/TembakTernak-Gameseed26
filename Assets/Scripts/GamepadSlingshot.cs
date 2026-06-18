using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GamepadSlingshot : MonoBehaviour
{
    [Header("Referensi Objek")]
    public Rigidbody2D animalRB;
    public Transform slingshotCenter;

    [Header("Pengaturan Ketapel")]
    public float powerMultiplier = 10f;
    public float maxDragDistance = 3f;

    [Header("Identitas Pemain")]
    public TurnManager.TurnPhase ownerPhase;

    private GameControls controls;
    private Vector2 aimInput;
    private bool isPulling = false;
    private Vector2 startPoint;
    private void Awake()
    {
        controls = new GameControls();

        controls.PlayerControls.Pull.started += ctx => StartPulling();
        controls.PlayerControls.Pull.canceled += ctx => ShootProjectile();

        controls.PlayerControls.Aim.performed += ctx => aimInput = ctx.ReadValue<Vector2>();
        controls.PlayerControls.Aim.canceled += ctx => aimInput = Vector2.zero;
    }

    // Start is called before the first frame update
    void Start()
    {
        startPoint = slingshotCenter.position;
        animalRB.isKinematic = true;

    }

    // Update is called once per frame
    void Update()
    {
        if(TurnManager.instance.currentPhase != ownerPhase)
        {
            return;
        }
        if (isPulling)
        {
            ProcessAiming();
        }
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void StartPulling()
    {
        if(TurnManager.instance.currentPhase == ownerPhase )
        {
            isPulling = true;
            animalRB.isKinematic = true;
            animalRB.velocity = Vector2.zero;
        }
    }

    private void ProcessAiming()
    {
        Vector2 pullVector = aimInput * maxDragDistance;

        animalRB.position = startPoint + pullVector;
    }

    private void ShootProjectile()
    {
        if(!isPulling || TurnManager.instance.currentPhase != ownerPhase)
        {
            return;
        }

        isPulling = false;
        animalRB.isKinematic= false;

        Vector2 releasePoint = animalRB.position;
        Vector2 shootDirection = (startPoint - releasePoint).normalized;
        float shootDistance = Vector2.Distance(startPoint, releasePoint);

        animalRB.AddForce(shootDirection * shootDistance * powerMultiplier, ForceMode2D.Impulse);
        TurnManager.instance.SetAirbornePhase();
    }
}
