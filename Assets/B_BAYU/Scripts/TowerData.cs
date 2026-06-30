using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data Tower Baru" , menuName = "Game Data/Tower Data")]
public class TowerData : ScriptableObject
{

    [Header("Status Markas")]
    public string towerName;
    public float maxHealth = 100f;

    [Header("Visual Tower")]
    public Sprite normalSprite;
    public Sprite halfDamageSprite;
    public Sprite destroyedSprite;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
