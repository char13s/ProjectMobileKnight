using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovements : MonoBehaviour
{
    private CharacterController charCon;
    [SerializeField] private Joystick joystick;
    [SerializeField] private float moveSpeed;
    private Vector3 speed;
    // Start is called before the first frame update
    void Start()
    {
        charCon = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateSpeed();
        charCon.Move(speed*moveSpeed*Time.deltaTime);
    }
    void UpdateSpeed() {
        speed.x = joystick.Direction.x;
        speed.z = joystick.Direction.y;
    }
}
