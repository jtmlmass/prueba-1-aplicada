using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;



public class PlayerControllerV2 : MonoBehaviour
{
    public float speed = 1;
    public float jumpForce = 2.5f;
    public Transform groundCheck;
    public LayerMask groundedLayers;
    public float groundCheckBaridus;
    private Rigidbody2D _body;
    private Animator _animator;

    
    private Vector2 _movement;
    private bool facingRight = true;
    public bool _isGrounded = true;
    private float counter = 0;

    private bool atacking = false;
    private int colPicas;

    private int veces;
    public int jumpsWanted;
    private bool isJumping=false;

    public Transform pica;
    public Transform vacio;
    private GameObject _target;
    public float minX;
    public float maxX;
    public float waitingTime = 2f;
    private bool pushing;
    private bool pushingAnimation;
    private bool isGrounded;
    private bool enableKey;
    Scene currentScene;
    string sceneName;

    //Attack
    private bool isAttacking;
    public GameObject espada;


    void Awake()
    {
        _body = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        currentScene = SceneManager.GetActiveScene();
        sceneName = currentScene.name;
    }
    // Start is called before the first frame update
    void Start()
    {
        veces=0;
        colPicas=0;
        UpdateTarget();
        pushing=false;
        pushingAnimation=false;
        enableKey=true;     
    }
    private void UpdateTarget()
    {
        if (_target == null )
        {
            _target = new GameObject("Target");
            _target.transform.position = new Vector2(maxX, transform.position.y);
            return;
        }
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if(other.tag == "Door")
        { 
            if (Input.GetKeyDown(KeyCode.W))
            {
                _animator.SetTrigger("Enter");
            }
        }
    }
    public void enableKeys(bool enable){
        enableKey=enable;
        if(enable==false){
            _movement = Vector3.zero;
        }
        
    }
    // Update is called once per frame
    void Update()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        
        _isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckBaridus, groundedLayers);
        
        if((horizontalInput>0f || horizontalInput<0f) &&enableKey==true){
            if(pushing==true){
                pushingAnimation=true;
               
            }
            else{
                pushingAnimation=false;
               
            }
        }
        if (!atacking&&enableKey==true){
            _movement = new Vector2(horizontalInput, 0f);
        }
        if (horizontalInput < 0f && facingRight == true&&enableKey==true) {
            flip();
        }
        else if (horizontalInput > 0f && facingRight == false&&enableKey==true) {
            flip();
        }
        //Esta saltando?
        if(Input.GetButtonDown("Jump") &&  veces==0 && _isGrounded==true&&enableKey==true && !(sceneName.Equals("Sala") || sceneName.Equals("Pasillo"))){
            _body.AddForce(Vector2.up *jumpForce, ForceMode2D.Impulse);
            isJumping=true;
            veces++;
            
        }
        else if(Input.GetButtonDown("Jump") && veces<jumpsWanted &isJumping==true&&enableKey==true && !(sceneName.Equals("Sala") || sceneName.Equals("Pasillo")))
        {
            _body.AddForce(Vector2.up *jumpForce, ForceMode2D.Impulse);
           veces++;
        }
        if( veces==jumpsWanted){
            veces=0;
            isJumping=false;
        }    
        if (_movement == Vector2.zero && !atacking&&enableKey==true){
            counter += 1 * Time.deltaTime;
        }
        else{
            counter = 0;
        }
        if(transform.position.x>minX && sceneName=="Jungla"){
            enableKey=false;
            StartCoroutine("PatrolToTarget");
           if(transform.position.x>maxX){
               SceneManager.LoadScene("Castillo");
           }
        }

        // Wanna Attack?
        if (Input.GetButtonDown("Fire1") && _isGrounded == true && isAttacking == false && espada.activeSelf == false)
        {
            _movement = Vector2.zero;
            _body.velocity = Vector2.zero;
            _animator.SetBool("Idle", false);
            _animator.SetTrigger("Attack");
        }
        // Find out Time 
        if (GameManager.instance.timerRunning)
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                Debug.Log("T pressed");
                Debug.Log(GameManager.instance.elapsedTime);
            }
        }

    }
    private void FixedUpdate()
    {
        if (isAttacking == false)
        {
            float horizontalVelocity;
            horizontalVelocity = _movement.normalized.x * speed;
            _body.velocity = new Vector2(horizontalVelocity, _body.velocity.y);
        }
        
    }
    private void LateUpdate()
    {
        _animator.SetBool("isGrounded", _isGrounded);
        _animator.SetBool("isPushing", pushingAnimation);

        // Animator
        if (_animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
        {
            isAttacking = true;
        }
        else
        {
            isAttacking = false;
        }

        if (_isGrounded==false){
            _animator.SetBool("Idle", false);
            
        }
        else{
            _animator.SetBool("Idle", _movement == Vector2.zero);
        }
        _animator.SetFloat("VerticalVelocity",_body.velocity.y);
        
    }
    private void flip()
    {
        facingRight = !facingRight;
        float localScaleX = transform.localScale.x;
        localScaleX *= -1f;
        transform.localScale = new Vector3(localScaleX, transform.localScale.y, transform.localScale.z);

    }
    
    void OnCollisionEnter2D(Collision2D collisionInfo){
        
        if(collisionInfo.collider.gameObject.layer==9){
            pushing=true;
        }
        
       if(collisionInfo.collider.tag=="Die"){
            colPicas++;
            _movement = Vector3.zero;
            enableKeys(false);
        }
        if(colPicas==1){
          Physics2D.IgnoreCollision(pica.GetComponent<Collider2D>(), GetComponent<Collider2D>());
          Physics2D.IgnoreCollision(vacio.GetComponent<Collider2D>(), GetComponent<Collider2D>());
          colPicas=0;
          //FindObjectOfType<GameManager>().endGame();
          GameManager.instance.EndGame();
        }
    }
    void OnCollisionExit2D(Collision2D collisionInfo)
    {
        if(collisionInfo.collider.gameObject.layer==9){
            pushing=false;
        }
        
    }
private IEnumerator PatrolToTarget()
    {
        while (Vector2.Distance(transform.position, _target.transform.position) > 0.05f)
        {
            _animator.SetBool("Idle", false);
            Vector2 direction = _target.transform.position - transform.position;
            float xDirection = direction.x;

            transform.Translate(direction.normalized * 0.005f * Time.deltaTime);
            _animator.SetBool("isGrounded", _isGrounded);
            yield return null;
        }
        transform.position = new Vector2(_target.transform.position.x, transform.position.y);
        _animator.SetBool("Idle", true);
        yield return new WaitForSeconds(waitingTime);

    }
   
}