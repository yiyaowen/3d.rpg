using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Asset", menuName = "Game Data/Property Asset/Character Stats")]
public class SO_CharacterStats : ScriptableObject
{
    [Header("Basic Properties")]
    public int maxHealth;
    public int currentHealth;
    public int baseDefence;
    public int currentDefence;

    [Header("Attack")]
    public float attackRange;
    public float skillRange;
    public float attackCD;
    public float minDamage;
    public float maxDamage;
    public float criticalChance;
    public float criticalMultiplier;
}
