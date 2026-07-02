using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerHealth : MonoBehaviour
{
    [Header("Data Tower")]
    public TowerData myTowerData;
    public float currentHealth;

    [Header("Komponen Visual")]
    public SpriteRenderer towerSpriteRenderer;

    [Header("Identitas Tower")]
    public TurnManager.TurnPhase towerOwner;
    // Start is called before the first frame update
    void Start()
    {
        if(myTowerData == null)
        {
            Debug.LogError("Tower Data belum diisi di Markas " + towerOwner.ToString());
            return;
        }

        currentHealth = myTowerData.maxHealth;

        if(towerSpriteRenderer != null)
        {
            towerSpriteRenderer.sprite = myTowerData.normalSprite;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(float damageAmount)
    {
        if(currentHealth <= 0)
        {
            return;
        }

        currentHealth -= damageAmount;
        Debug.Log("Markas " + towerOwner.ToString() + " terkena serangan! Sisa HP: " + currentHealth);

        UpdateTowerVisual();

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            DestroyTower();
        }
        else
        {

        }



        currentHealth -= damageAmount;
        Debug.Log("Markas " + towerOwner.ToString() + " terkena Serangan! Damage:  " + damageAmount + " Sisa HP: " + currentHealth);
    }

    private void UpdateTowerVisual()
    {
        if(towerSpriteRenderer == null || myTowerData == null)
        {
            return;
        }

        float healthPercentage = currentHealth / myTowerData.maxHealth;

        if(healthPercentage <= 0)
        {
            towerSpriteRenderer.sprite = myTowerData.destroyedSprite;
        }
        else if(healthPercentage <= .5f)
        {
            towerSpriteRenderer.sprite = myTowerData.halfDamageSprite;
        }
        else
        {
            towerSpriteRenderer.sprite = myTowerData.normalSprite;
        }
    }

    private void DestroyTower()
    {
        Debug.Log("MARKAS " + towerOwner.ToString() + " HANCUR! PERMAINAN SELESAI!");

        if(GameOverManager.Instance != null)
        {
            GameOverManager.Instance.TriggerGameOver(towerOwner);
        }
    }
}
