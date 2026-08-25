/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 玩家系统 / Player system
 * 功能 / Purpose: 管理玩家生命、资源、VR 控制、状态重置和战斗交互入口。
 * English: Manages player HP, resources, VR control, state reset, and combat interaction entry points.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityCharacterController;
using Oculus.Interaction.Input;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class VRplayer_controller : MonoBehaviour
{
    public enum RotationType
    {
        Smooth,
        Snap
    }

    [Header("Turning")]
    public RotationType rotationType;
    public CharacterController characterController;
    public Animator animator;

    private Vector3 velocity;
    public bool isGrounded=true;
    private float _previousTurnAxis;

     private bool _waitingForCameraMovement;
     [SerializeField] private float _actualVelocity;
      private Vector3 _previousLeftControllerPosition;
        private Vector3 _previousRightControllerPosition;
        private Vector3 PreviousPosition;
     //   public Transform LeftControllerTransform;
      //  public Transform RightControllerTransform;
        private Vector3 _cameraStartingPosition;
        [Header("Transforms")]
        public Transform Camera;
        [Header("Components")]
        public GameObject CameraRig;
        
         public Transform Neck;
         [Tooltip("Axis threshold to be considered valid for snap turning.")]
        public float SnapThreshold = .75f;
        public float SnapAmount = 45f;
          public float SmoothTurnThreshold = .1f;
          public float SmoothTurnSpeed = 90f;
         public float MoveSpeed = 1.5f;
         public float RunSpeed = 3.5f;
        public float Gravity = 9.81f;
        public bool Sprinting;
         public bool MovementEnabled =true;
        public bool RotationEnabled=true;
        public bool UseWASD;
        public bool CanJump = true;
        public bool CanSteerWhileJumping = true;
        public bool CanSprint = true;
        public bool CanCrouch = true;
        [Header("Locomotion")]
        public bool InstantAcceleration = true;
        private Vector3 xzVelocity;
        public float Deacceleration = 15f;
        public float Acceleration = 15;
        public float SprintAcceleration = 20f;
        [Header("Debugging")]
        public bool MouseTurning;
        public float MouseSensitivityX = 1f;

        private bool _isCameraCorrecting;
        [Tooltip("If true, limits the head distance from the body by MaxLean amount.")]
        public bool LimitHeadDistance = true;
        [Tooltip("If LimitHeadDistance is true, the max distance your head can be from your body.")]
        public float MaxLean = .5f;
         [Tooltip("Screen fades when leaning to far into something.")]
        public bool FadeFromLean = true;
        public float GroundedDistance = .02f;
        public LayerMask GroundedLayerMask;

        private bool _awaitingSecondClick;
        private float _timeSinceLastPress;
        [Tooltip("Double click timeout for sprinting.")]
        public float DoubleClickThreshold = .25f;
        private float yVelocity;
         public float JumpVelocity = 5f;
         public float MaxFallSpeed = 2f;
         float distanceToGround=0f;
         [Range(0,1)]
         public float adjust_height=0.07f;
         public bool is_backWarking;
         [Range(1,5)]
         public float BackWarkingSpeed=2f;
         Hand_and_controller hand_And_Controller;
         public Player_managerV2 player_ManagerV2;
         public TextMeshProUGUI textMeshProUGUI;
         
         public AudioClip 体力耗尽音效;

    
    void Update()
    {
            //CheckCameraCorrection();
             CheckSprinting();
            // if(OVRInput.Get(OVRInput.Button.One,OVRInput.Controller.LTouch) ) print("进入竖直按键为真");
            // else print("进入竖直按键为假");
            
          
            //CameraRig.PlayerControllerYOffset = _crouchOffset;
    }
    
    
      protected virtual void UpdateHeight()
        {
          //  if(!isGrounded) return;
               // 创建射线
                Ray ray = new Ray(Camera.position, Vector3.down);
               // 用来存储射线碰撞信息的结构体
                RaycastHit hitInfo;
                // 发射射线，并检测是否与地面（Layer为Ground的物体）发生碰撞
                if (Physics.Raycast(ray, out hitInfo, 5, LayerMask.GetMask("ground")))
                {
                    // 计算起始点到地面的距离
                     distanceToGround = hitInfo.distance;

                    // 输出距离信息
                  //  Debug.Log("Distance to ground: " + distanceToGround);
                }
                else
                {
                    // 如果没有与地面发生碰撞，可以在控制台输出错误信息
                 //   Debug.Log("Ray did not hit the ground.");
                }

            characterController.height = distanceToGround+adjust_height;
            characterController.center = new Vector3(0, characterController.height * .5f + characterController.skinWidth, 0f);
        }

    
     protected virtual void CheckSprinting()
        {
            if (!CanSprint)
                return;
            
            bool is_runButton = OVRInput.Get(OVRInput.Button.PrimaryThumbstick,OVRInput.Controller.LTouch);
         
           if(!player_ManagerV2.is_staminaZero())
           {
                if(is_runButton)
                {
                        Sprinting =true;
                        player_ManagerV2.start_SprintStamina();
                        player_ManagerV2.stop_recoverStamina();
                        //player_ManagerV2.stop_wait2Recover_Stamina();
                }else
                {
                        if(Sprinting)
                        {
                            Sprinting=false;
                            player_ManagerV2.stop_SprintStamina();
                            player_ManagerV2.start_wait2Recover_Stamina();
                        }
                    
                }
           }else 
            {
                if(Sprinting)
                    {
                        GetComponent<AudioSource>().clip = 体力耗尽音效;
                        GetComponent<AudioSource>().Play();
                         Sprinting=false;
                        player_ManagerV2.stop_SprintStamina();
                        player_ManagerV2.start_wait2Recover_Stamina();
                    }
            }
        }

     
    void FixedUpdate()
    {
         if (_waitingForCameraMovement)
                CheckCameraMovement();
            
            if(hand_And_Controller==null || hand_And_Controller.gameObject.activeSelf==false)
            {
                GameObject res = GameObject.FindWithTag("Hand_and_Controller");
                if(null!=res)
                hand_And_Controller = res.GetComponent<Hand_and_controller>();
            }
                

            if (characterController.enabled)
            {
                HandleMovement();

                if (CanRotate()&&!is_backWarking&&!hand_And_Controller.is_handtracing)
                {
                    HandleRotation();
                }
            }

         //   CheckLean();
            CheckGrounded();
              UpdateHeight();


            _actualVelocity = ((transform.position - PreviousPosition) / Time.deltaTime).magnitude;

          //  _previousLeftControllerPosition = LeftControllerTransform.position;
         //   _previousRightControllerPosition = RightControllerTransform.position;

            PreviousPosition = transform.position;
    }

    protected virtual void CheckCameraMovement()
        {
            if (Vector3.Distance(_cameraStartingPosition, Camera.transform.localPosition) < .05f)
            {
                return;
            }

            var delta = Camera.transform.position - characterController.transform.position;
            delta.y = 0f;
            CameraRig.transform.position -= delta;
            _waitingForCameraMovement = false;
            PreviousPosition = transform.position;
        }

        protected virtual void HandleMovement()
        {
           /* if (IsClimbing)
            {
                HandleClimbing();
                return;
            }*/

          
            if(is_backWarking)
            {backWardMovement(); }

           // print("进入处理移动");

            if (!_waitingForCameraMovement)
            {
                HandleHMDMovement();
            }
            HandleHorizontalMovement();
            HandleVerticalMovement();
           // AdjustHandAcceleration();
        }


        //这个函数是用来同步玩家自身头部移动时候，charactercontroller的位置以及camerarig的位置的
         protected virtual void HandleHMDMovement()
        {
            var originalCameraPosition = CameraRig.transform.position;
            var originalCameraRotation = CameraRig.transform.rotation;

            var delta = Neck.transform.position - characterController.transform.position;
            delta.y = 0f;
            if (delta.magnitude > 0.0f && characterController.enabled)
            {
                characterController.Move(delta);
            }

            transform.rotation = Quaternion.Euler(0.0f, Neck.rotation.eulerAngles.y, 0.0f);

            CameraRig.transform.position = originalCameraPosition;
            var local = CameraRig.transform.localPosition;
            local.y = 0f;
            CameraRig.transform.localPosition = local;
            CameraRig.transform.rotation = originalCameraRotation;
        }

        private void End_backing()
        {
            is_backWarking=false;
        }

        public void Pull_playerBackWard(Vector3 PULL_direction,float PullBackTime=1)
        {
                is_backWarking=true;
                backWardMoveTime = PullBackTime;
                pull_dir =  new Vector3(PULL_direction.x,0,PULL_direction.z);
                pull_dir.Normalize();
                Invoke("End_backing",PullBackTime);
        }

        private float backWardMoveTime=1;
        private Vector3 pull_dir;
        private void backWardMovement()
        {
                characterController.Move(pull_dir *BackWarkingSpeed* Time.deltaTime);
        }


        protected virtual void HandleHorizontalMovement()
        {
            if(is_backWarking) return;
              if(hand_And_Controller.is_handtracing) return;
          //  print("进入处理水平移动");
            var speed = MoveSpeed;
            var runSpeed = RunSpeed;

            if (Sprinting)
                speed = runSpeed;

            textMeshProUGUI.text = speed.ToString();

            var movement = GetMovementAxis();

            if (!MovementEnabled)
            {
                movement = Vector2.zero;
            }

            GetMovementDirection(out var forward, out var right);
            var direction = (forward * movement.y + right * movement.x);

            if (isGrounded || CanSteerWhileJumping)
            {
                if (InstantAcceleration)
                {
                    xzVelocity = speed * direction;
                }
                else
                {
                    var noMovement = Mathf.Abs(movement.x) < .1f && Mathf.Abs(movement.y) < .1f;
                    if (noMovement)
                    {
                        var dir = xzVelocity.normalized;
                        var deacceleration = Deacceleration * Time.deltaTime;
                        if (deacceleration > xzVelocity.magnitude)
                        {
                            xzVelocity = Vector3.zero;
                        }
                        else
                        {
                            xzVelocity -= dir * deacceleration;
                        }
                    }
                    else
                    {
                        var acceleration = (Sprinting ? SprintAcceleration : Acceleration) * Time.deltaTime;
                        xzVelocity += acceleration * direction;
                        xzVelocity = Vector3.ClampMagnitude(xzVelocity, speed);
                    }
                }
            }
        }


        protected virtual void HandleVerticalMovement()
        {
            if(is_backWarking) return;
            if(hand_And_Controller.is_handtracing) return;
            Vector3 velocity = xzVelocity;
          //  print("进入竖直运动1");

            if (isGrounded)
            {
               //  print("进入竖直运动2");
                 bool button_jump = OVRInput.Get(OVRInput.Button.One,OVRInput.Controller.LTouch) ;
               //   print("进入竖直按键结果为："+button_jump);
                if (button_jump && CanJump && MovementEnabled)
                {
                   //  print("进入竖直运动3");
                   if(player_ManagerV2.Minus_Jump())
                   {
        
                        yVelocity = JumpVelocity;
                   }
                    
                }
                else
                {
        
                    yVelocity += -Gravity * Time.deltaTime;
                }


                yVelocity = Mathf.Clamp(yVelocity, -Gravity * Time.deltaTime, yVelocity);
            }
            else
            {
                // print("进入竖直运动5");
                yVelocity += -Gravity * Time.deltaTime;
                yVelocity = Mathf.Clamp(yVelocity, -MaxFallSpeed, yVelocity);
            }

            velocity.y = yVelocity;

            characterController.Move(velocity * Time.deltaTime);
        }

        protected virtual Vector2 GetTurnAxis()
        {
            // 获取右控制器摇杆的输入
          //  Vector2 rightJoystick = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick, OVRInput.Controller.RTouch);
            Vector2 rightJoystick=OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch);
          //  print("右手柄遥感输入2维量为："+ rightJoystick);
            // 获取右控制器摇杆的X轴输入
           // float rightJoystickX = rightJoystick.x;
            return rightJoystick;
        }

        Vector2 GetMovementAxis()
        {
            if (UseWASD)
            {
             //   print("进入获取WASD");
                var wasd = CheckWASD();
            //    print("进入获取的WASD为: "+wasd);
                if (wasd.sqrMagnitude > 0f)
                  { //print("进入获取的WASD成功返回"); 
                  return wasd;}  
            }
            return  OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.LTouch);
        }




        private Vector2 CheckWASD()
        {
            var x = 0f;
            var y = 0f;

#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKey(KeyCode.W))
                y += 1f;
            if (Input.GetKey(KeyCode.S))
                y -= 1f;
            if (Input.GetKey(KeyCode.A))
                x += -1f;
            if (Input.GetKey(KeyCode.D))
                x += 1f;
#elif ENABLE_INPUT_SYSTEM
            if (Keyboard.current[Key.W].isPressed)
                y += 1f;
            if (Keyboard.current[Key.S].isPressed)
                y -= 1f;
            if (Keyboard.current[Key.A].isPressed)
                x += -1f;
            if (Keyboard.current[Key.D].isPressed)
                x += 1f;
#endif

            return new Vector2(x, y);
        }


        protected virtual void GetMovementDirection(out Vector3 forwards, out Vector3 right)
        {
            var t = transform;

          /*  switch (DirectionStyle)
            {
                case PlayerDirectionMode.Camera:
                    if (Camera)
                        t = Camera;
                    break;
                case PlayerDirectionMode.LeftController:
                    if (LeftControllerTransform)
                        t = LeftControllerTransform;
                    break;
                case PlayerDirectionMode.RightController:
                    if (RightControllerTransform)
                        t = RightControllerTransform;
                    break;
            }*/

             if (Camera)
                        t = Camera;

            forwards = t.forward;
            right = t.right;
            forwards.y = 0;
            forwards.Normalize();
            right.y = 0;
            right.Normalize();
        }

        protected virtual bool CanRotate()
        {
            if (!RotationEnabled)
                return false;

           // print("进入成功转向检测");
            return true;
        }

        protected virtual void HandleRotation()
        {
            if (rotationType == RotationType.Smooth)
            {
                HandleSmoothRotation();
            }
            else if (rotationType == RotationType.Snap)
            {
                HandleSnapRotation();
            }

            HandlMouseRotation();

            _previousTurnAxis = GetTurnAxis().x;
        }

       /*  protected virtual void CheckLean()
        {
            if (_isCameraCorrecting || !LimitHeadDistance)
                return;

            var delta = Neck.transform.position - characterController.transform.position;
            delta.y = 0;

            if (delta.sqrMagnitude < .01f || delta.magnitude < MaxLean) return;
            
            if (FadeFromLean)
            {
                StartCoroutine(CorrectCamera());
                return;
            }

            var allowedPosition = characterController.transform.position + delta.normalized * MaxLean;
            var difference = allowedPosition - Neck.transform.position;
            difference.y = 0f;
            CameraRig.transform.position += difference;
        }*/

        protected virtual void CheckGrounded()
        {
            var origin = characterController.center - Vector3.up * (.5f * characterController.height - characterController.radius);
            isGrounded = Physics.SphereCast(
                transform.TransformPoint(origin) + Vector3.up * characterController.contactOffset, 
                characterController.radius,
                Vector3.down,
                out var hit,
                GroundedDistance + characterController.contactOffset,
                GroundedLayerMask, QueryTriggerInteraction.Ignore);

          //  print("地面检测结果为："+isGrounded);
        }

        protected virtual void HandleSmoothRotation()
        {
            var input = GetTurnAxis().x;
            if (Math.Abs(input) < SmoothTurnThreshold)
                return;

            var rotation = input * SmoothTurnSpeed * Time.deltaTime;
            var rotationVector = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + rotation, transform.eulerAngles.z);
            transform.rotation = Quaternion.Euler(rotationVector);
        }

         protected virtual void HandleSnapRotation()
        {
          //  print("进入Snap转向");
            var input = GetTurnAxis().x;
          //  print("转向X轴绝对值为："+Math.Abs(input)+"  前一次轴绝对值为："+_previousTurnAxis+" 阈值为："+SnapThreshold);
            if (Math.Abs(input) < SnapThreshold || Mathf.Abs(_previousTurnAxis) > SnapThreshold)
                return;
          //  print("进入Snap转向判断成功");
            var rotation = Quaternion.Euler(0, Mathf.Sign(input) * SnapAmount, 0);
            transform.rotation *= rotation;
        }

         protected virtual void HandlMouseRotation()
        {
            if (MouseTurning)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    var offset = Quaternion.Euler(new Vector3(0, Input.GetAxis("Mouse X") * MouseSensitivityX, 0));
                    transform.rotation *= offset;

                    Cursor.lockState = CursorLockMode.Locked;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.None;
                }
            }
        }

      /*   private IEnumerator CorrectCamera()
        {
           _isCameraCorrecting = true;

            var delta = transform.position - Neck.position;
            delta.y = 0f;

            if (!ScreenFader)
            {
                CameraRig.transform.position += delta;
                _isCameraCorrecting = false;
                yield break;
            }

            ScreenFader.Fade(1, HeadCollisionFadeSpeed);

            while (ScreenFader.CurrentFade < .9)
            {
                yield return null;
            }

            delta = transform.position - Neck.position;
            delta.y = 0f;
            CameraRig.transform.position += delta;

            ScreenFader.Fade(0, HeadCollisionFadeSpeed);

            while (ScreenFader.CurrentFade > .1)
            {
                yield return null;
            }

            _isCameraCorrecting = false;
        }*/

    
}
