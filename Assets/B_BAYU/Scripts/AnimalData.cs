using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data Hewan Baru", menuName = "Game Data/Animal Data")]
public class AnimalData : ScriptableObject
{
    [Header("Identitas Hewan")]
    public string animalName;
    public GameObject animalPrefab;

    [Header("Data visual & kombo")]
    public Sprite defaultSprite;
    public Sprite[] comboPoses;
    public int comboLength = 4;

    [Header("Data status tempur hewan")]
    public float baseDamage = 10f;
    public float weight = 1f;
    
}
