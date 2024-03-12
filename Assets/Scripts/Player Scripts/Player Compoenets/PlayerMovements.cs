using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovements : MonoBehaviour
{
    Player player;
    private CharacterController _controller;
    Animator _animator;
    [SerializeField] private Joystick _input;
    [SerializeField] private float DirectionSpeed;
    private float targetSpeed;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private float _animationBlend;
    private float _speed;
    // Start is called before the first frame update
    private void Awake() {
        player = GetComponent<Player>();
        _controller = GetComponent<CharacterController>();
    }
    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        print(_input.Direction.magnitude);
        Direction();
        UpdateSpeed();
        //_controller.Move(speed*DirectionSpeed*Time.deltaTime);
    }
    void UpdateSpeed() {
        if (_input.Direction.SqrMagnitude() > 0) {
            player.Anim.SetBool("Moving", true);
            //transform.TransformDirection(joystick.Direction.x,0, joystick.Direction.y);
            transform.rotation = Quaternion.Euler(0.0f, _input.Direction.magnitude, 0.0f);
        }
        else {
            player.Anim.SetBool("Moving", false);
        }
        //speed.x = _input.Direction.x;
        //speed.z = _input.Direction.y;
    }
    private void Direction() {
        // set target speed based on Direction speed, sprint speed and if sprint is pressed
        float targetSpeed = 7;

        // a simplistic acceleration and deceleration designed to be easy to reDirection, replace, or iterate upon

        // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is no input, set the target speed to 0
        if (_input.Direction == Vector2.zero) targetSpeed = 0.0f;

        // a reference to the players current horizontal velocity
        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        float speedOffset = 0.1f;
        //float inputMagnitude = _input.analogDirectionment ? _input.Direction.magnitude : 1f;

        // accelerate or decelerate to target speed
        if (currentHorizontalSpeed < targetSpeed - speedOffset ||
            currentHorizontalSpeed > targetSpeed + speedOffset) {
            // creates curved result rather than a linear one giving a more organic speed change
            // note T in Lerp is clamped, so we don't need to clamp our speed
            _speed = Mathf.Lerp(currentHorizontalSpeed, _input.Direction.magnitude,
                Time.deltaTime * 10);

            // round speed to 3 decimal places
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else {
            _speed = targetSpeed;
        }

       // _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
       // if (_animationBlend < 0.01f) _animationBlend = 0f;

        // normalise input direction
        Vector3 inputDirection = new Vector3(_input.Direction.x, 0.0f, _input.Direction.y).normalized;
        //_animator.SetFloat("XInput", _input.Direction.x);
        //_animator.SetFloat("YInput", _input.Direction.y);
        // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is a Direction input rotate player when the player is moving
        if (_input.Direction != Vector2.zero) {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                15);

            // rotate to face input direction relative to camera position
            //
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f); 
        }
        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

        // Direction the player
        //if (!player.Attacking ) {
        _controller.Move(targetDirection.normalized * _input.Direction.magnitude * (_speed * Time.deltaTime));
        

    }
}
