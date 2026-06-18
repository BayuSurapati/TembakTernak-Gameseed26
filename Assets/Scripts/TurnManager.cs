using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager instance;

    public enum TurnPhase { Player1, Player2, Airborne }
    public TurnPhase currentPhase = TurnPhase.Player1;
    private void Awake()
    {
        if(instance == null)
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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SwitchTurn()
    {
        if(currentPhase == TurnPhase.Player1)
        {
            currentPhase = TurnPhase.Player2;
            Debug.Log("Giliran Player 2");
        }else if(currentPhase == TurnPhase.Player2)
        {
            currentPhase = TurnPhase.Player1;
            Debug.Log("Giliran Player 1");
        }
    }

    public void SetAirbornePhase()
    {
        currentPhase = TurnPhase.Airborne;
        Debug.Log("Proyektil Melayang! Kunci kontrol ketapel.");
    }
}
