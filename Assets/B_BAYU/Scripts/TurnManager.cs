using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager instance;
    public enum TurnPhase { Player1, Player2, Airborne, Transition }

    [Header("Pengaturan Kamera Dinamis")]
    public CinemachineVirtualCamera mainVCam;
    public GameObject worldVCam;

    [Header("Status Pemain saat ini")]
    public TurnPhase currentPhase = TurnPhase.Player1;
    private TurnPhase nextPlayerPhase = TurnPhase.Player2;

    [Header("Pengaturan Transisi")]
    public float delayBetweenTurns = 3f;

    public AnimalData[] player1AnimalPool;
    public AnimalData[] player2AnimalPool;
    public Transform slingshotP1;
    public Transform slingshotP2;

    private GameObject currentActiveAnimal;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        SpawnAnimalForCurrentTurn();
    }

    // Update is called once per frame
    void Update()
    {

    }

    /*public void SwitchTurn()
    {
        if (currentPhase == TurnPhase.Player1)
        {
            currentPhase = TurnPhase.Player2;
            Debug.Log("Giliran Player 2");
        }
        else if (currentPhase == TurnPhase.Player2)
        {
            currentPhase = TurnPhase.Player1;
            Debug.Log("Giliran Player 1");
        }
    }*/

    public void SetAirbornePhase()
    {
        nextPlayerPhase = (currentPhase == TurnPhase.Player1) ? TurnPhase.Player2 : TurnPhase.Player1;
        currentPhase = TurnPhase.Airborne;

        if(mainVCam != null && currentActiveAnimal != null)
        {
            mainVCam.Follow = currentActiveAnimal.transform;
        }

        Debug.Log("Proyektil Melayang! Kunci kontrol ketapel.");
    }

    public void FinishActionAndSwitchTurn()
    {
        if(currentPhase == TurnPhase.Transition)
        {
            return;
        }
        StartCoroutine(TransitionRoutine());
    }

    IEnumerator TransitionRoutine()
    {
        currentPhase = TurnPhase.Transition;

        if(worldVCam != null)
        {
            worldVCam.SetActive(true);
        }

        Debug.Log("Fase Transisi: Menunggu " + delayBetweenTurns + " detik untuk melihat kerusakan...");

        yield return new WaitForSeconds(delayBetweenTurns);

        if(currentActiveAnimal != null)
        {
            Destroy(currentActiveAnimal);
        }

        if(worldVCam != null)
        {
            worldVCam.SetActive(false);
        }
        currentPhase = nextPlayerPhase;
        Debug.Log("Giliran baru dimulai! Sekarang giliran: " + currentPhase.ToString());

        SpawnAnimalForCurrentTurn();
    }

    private void SpawnAnimalForCurrentTurn()
    {
        AnimalData[] currentPool = (currentPhase == TurnPhase.Player1) ? player1AnimalPool : player2AnimalPool;
        Transform currentSlingshot  = (currentPhase == TurnPhase.Player1) ? slingshotP1 : slingshotP2;

        if(currentPool == null || currentPool.Length == 0)
        {
            Debug.LogError("Kumpulan Hewan Belum diisi di Inspector");
            return;
        }

        if(mainVCam != null)
        {
            mainVCam.Follow = currentSlingshot;
        }

        int randomIndex = Random.Range(0, currentPool.Length);
        AnimalData selectedData = currentPool[randomIndex];

        currentActiveAnimal = Instantiate(selectedData.animalPrefab, currentSlingshot.position, Quaternion.identity);

        if(currentActiveAnimal.GetComponent<GamepadSlingshot>() != null)
        {
            currentActiveAnimal.GetComponent<GamepadSlingshot>().slingshotCenter = currentSlingshot;
        }
        if(currentActiveAnimal.GetComponent<AirComboManager>() != null)
        {
            currentActiveAnimal.GetComponent<AirComboManager>().myAnimalData = selectedData;
        }
        Debug.Log("Hewan terpilih untuk giliran ini: " + selectedData.animalName);
    }
}
