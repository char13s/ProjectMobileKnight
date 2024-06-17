using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [SerializeField] private float attackPower;
    //[SerializeField] private  
    public abstract void Attack();
}
