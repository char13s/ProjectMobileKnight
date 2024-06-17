using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMovement : StateMachineBehaviour
{
    CharacterController charCon;
    GameObject baseObject;
    [SerializeField] private Vector3 direction;
    [SerializeField] private bool isForward;
    [SerializeField] private float moveSpeed;
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        charCon=animator.GetComponent<CharacterController>();
        baseObject = animator.gameObject;
    }
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if(isForward)
            charCon.Move(baseObject.transform.forward  * moveSpeed * Time.deltaTime);
        else
            charCon.Move(direction * moveSpeed * Time.deltaTime);
    }
}
