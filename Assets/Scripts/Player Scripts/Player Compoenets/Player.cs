using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    Animator anim;
    bool moving;
    public Animator Anim { get => anim; set => anim = value; }
    public bool Moving { get => moving; set { moving = value;anim.SetBool("Moving", value); } }

    private void Awake() {
        Anim = GetComponent<Animator>();
    }

}
