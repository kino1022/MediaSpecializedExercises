using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControl : MonoBehaviour
{
    //プレイヤーキャラのアニメーター登録
    public Animator playerAnimator;
    ///プレイヤーの移動速度
    public float moveSpeed = 1.0f;
    public float moveSpeedMax = 5.0f;
    //初期ジャンプの高さ
    public float initialJumpForce = 5f;
    //ジャンプボタン押しっぱなし時に加わる力
    public float holdJumpForce = 20f;
    //ジャンプボタン押しっぱなし時に力が増加する時間
    public float maxHoldTime = 0.5f;
    //ジャンプボタン押しっぱなし処理用フラグと時間格納変数
    private bool isHoldingJump = false;
    private float holdTime = 0f;
    //ジャンプ処理実行用フラグ
    private bool doJump = false;
    //プレイヤー操作可能かのフラグ
    private bool playable = true;
    //プレイヤーのリジッドボディ格納用変数
    private Rigidbody rb;
    private float rbVelocity;
    //接地チェック用変数群
    //　接地チェック、設置していたら真
    private bool isGrounded = true;
    //　レイの半径
    [SerializeField]private float groundCheckRadius = 0.4f;
    //　レイの距離
    private float groundCheckDistance = 0.2f;
    //　レイの開始地点調整用変数
    private float groundCheckOffsetY;
    //　レイの衝突情報格納
    private RaycastHit hit;
    
    [SerializeField]
    private float sprintSpeed = 2.0f;

    private float sprintSpeedForce = 1.0f;

    private float defaultSprintSpeedForce;

    //プレイヤー入力の格納変数
    private InputAction moveInput;
    private InputAction jumpInput;
    private InputAction sprintInput;
    private Vector2 inputMoveAxis;

    //Startよりも前に1回だけ実行される
    void Awake() {
        defaultSprintSpeedForce = sprintSpeedForce;
        //接地チェック用レイの距離の計算
        groundCheckDistance = groundCheckRadius / 2;
        //接地チェック用レイの開始地点調整用変数の計算
        groundCheckOffsetY = groundCheckRadius + groundCheckDistance / 4;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //リジッドボディの参照
        rb = this.GetComponent<Rigidbody>();
        // InputSystemから"Move" と "Jump" の値を参照
        moveInput = InputSystem.actions.FindAction("Move");
        jumpInput = InputSystem.actions.FindAction("Jump");
        sprintInput = InputSystem.actions.FindAction("Sprint");
        //ボタンを押した時の処理の登録、以降ボタンが押されると登録したメソッドが実行される
        jumpInput.started += OnJumpStarted;
        jumpInput.canceled += OnJumpReleased;
        sprintInput.started += OnSprintStarted;
        sprintInput.canceled += OnSprintReleased; 
    }

    //FixedUpdateはフレームレートに関係なく物理演算の計算前に一定間隔で呼び出される
    void FixedUpdate()
    {
        //接地チェック
        isGrounded = checkGrounded();
        //updateで取得したプレイヤーの入力処理を移動速度として格納
        Vector3 movement = new Vector3(inputMoveAxis.x, 0.0f, 0.0f);
        //空中は地面の摩擦がなくなるので、移動速度を3分の1にする
        if (!isGrounded) movement /= 3;
        //プレイヤーの最大速度の計算、速度が上がるにつれてspeedLimitは0になる
        float speedLimit = moveSpeedMax * sprintSpeedForce - Mathf.Abs(rb.linearVelocity.x);
        
        if (speedLimit < 0) speedLimit = 0;
        
        //移動速度をリジッドボディにAddForceに反映、speedLimitを乗算して最大速度を制限¥¥
        rb.AddForce(movement * moveSpeed * speedLimit * sprintSpeedForce, ForceMode.Force);
        //ジャンプフラグがTrueの時、ジャンプ処理
        if(doJump){
            if (playable && isGrounded)
            {
                //リジッドボディに上方向の力を加える
                rb.AddForce(Vector3.up * initialJumpForce, ForceMode.Impulse);
                isHoldingJump = true;
                holdTime = 0f;
            }
            doJump = false;

        }
        
        if (isHoldingJump && holdTime < maxHoldTime)
        {
            float forcePerFrame = holdJumpForce * Time.fixedDeltaTime;
                
            rb.AddForce(Vector3.up * forcePerFrame, ForceMode.Acceleration);
                
            holdTime += Time.fixedDeltaTime;
        }

        //リジッドボディの速度の取得、アニメーション制御用
        rbVelocity = rb.linearVelocity.x;
    }

    // Update is called once per frame
    void Update()
    {
        //操作可能な時実行
        if (playable)
        {
            //上下左右の入力を取得
            inputMoveAxis = moveInput.ReadValue<Vector2>();
            //左右の入力に応じてプレイヤーの向きを変更
            if (inputMoveAxis.x > 0) transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            else if (inputMoveAxis.x < 0) transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
            //アニメーション制御メソッドの呼び出し
            setAnimation();
        }
    }

    //ジャンプボタンを押した時の処理
    private void OnJumpStarted(InputAction.CallbackContext context)
    {
        //ジャンプフラグをtrueにする
        doJump = true;
    }
    //ジャンプボタンを離したときの処理
    private void OnJumpReleased(InputAction.CallbackContext context)
    {
        isHoldingJump = false;
    }
    //Bダッシュボタン処理
    private void OnSprintStarted(InputAction.CallbackContext context)
    {
        sprintSpeedForce = sprintSpeed;
    }
    //Bダッシュボタン離した処理
    private void OnSprintReleased(InputAction.CallbackContext context)
    {
        sprintSpeedForce = defaultSprintSpeedForce;
    }
    //接地チェック
    bool checkGrounded()
    {
        //球体レイキャストをプレイヤー足元方向に発射して接触があればTrueを返す
        //接触情報はhitに格納
        return Physics.SphereCast(transform.position + groundCheckOffsetY * Vector3.up, groundCheckRadius, Vector3.down, out hit, groundCheckDistance, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore);
    }
    //可視化
    void OnDrawGizmos()
    {
        //接地チェックのレイキャストを可視化
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + Vector3.down * (groundCheckRadius / 2), groundCheckRadius);
    }

    void setAnimation(){
        //リジッドボディの速度をspeedに入力、速度に応じてアイドル、歩き、走りの切り替え
        playerAnimator.SetFloat("speed",Mathf.Abs(rbVelocity/2));
        //接地していなかったらジャンプモーションに変更
        playerAnimator.SetBool("jump",!isGrounded);
        //下ボタンの入力があったらしゃがみフラグをオン、なければオフ
        if (inputMoveAxis.y < -0.3) playerAnimator.SetBool("squat", true);
        else playerAnimator.SetBool("squat", false);
        //急ブレーキモーション、リジッドボディの速度ベクトルとキー入力方向が逆だったら（マイナスになったら）フラグオン
        if (Mathf.Abs(rbVelocity) > 3.0f && rbVelocity * inputMoveAxis.x < 0) playerAnimator.SetBool("recoil", true);
        else playerAnimator.SetBool("recoil", false);
    }
}
