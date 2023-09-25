using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    
    public GameObject playerModel;
    public Weapon playerGun;
    public GameObject player;
    public Transform orientation;
    public Rigidbody rb;
    public Transform playerCam;
    public Transform head;
    public Animator animations;
    public TextMesh displayName;
    public Keys keys { get; set; }

    //PlayerStats
    public float health = 100f;
    public float block = 80f;
    public string username = "";
    public int id = -1;
    public bool enemy = false;
    private bool alive = true;

    //PS
    public ParticleSystem dashPS;
    public GameObject dashGO;
    public GameObject impactPS;
    public GameObject bloodPS;
    public GameObject jumpPS;
    public GameObject landPS;

    //Shoot
    private float damage = 10f;
    private float range = 100f;
    private float fireRate = 2f;
    private float nextTimeToFire = 0f;

    //Dash
    private float dashExecutionTimeDelay = 0.1f;
    private float dashForce = 2000f;
    private float dashDelay = 0.8f;
    private bool finishDashDelay = true;
    private bool isDashing = false;

    //Jump
    private float jumpHeight = 1000f;
    private float jumpDelay = 0.6f;
    private bool readyToJump = true;
    
    //Movement
    private float maxSpeed = 10f;
    private float moveSpeed = 2500f;
    private float counterMovement = 0.175f;
    private float threshold = 0.001f;

    //Look
    // private float mouseSensitivity = 70f;
    private float mouseSensitivity = 700f;
    private float xRotation = 0f;
    
    //GroundCheck
    private bool isGrounded;
    private float groundCheckRadius = 0.1f;
    public Transform groundCheck;
    public LayerMask whatIsGround;

    //BufferPrediction
    private bool reconciliation = false;
    private static int maxBufferSize = 256;
    private Keys[] playerKeysBuffer = new Keys[maxBufferSize];
    private PlayerState[] playerStateBuffer = new PlayerState[maxBufferSize];

    private void Awake() {
        for (int i = 0; i < playerStateBuffer.Length; i++) playerStateBuffer[i] = new PlayerState();
    }

    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        keys = new Keys();
    }

    private IEnumerator coroutine;
    void FixedUpdate() {
        if(alive && !reconciliation) {
            keys.updatePlayerRotation(orientation.localRotation);
            keys.updateCamRotation(playerCam.localRotation);
            
            storeBufferKeysAndState();
            Keys prekeys = keys.deepCopy();

            if(!enemy) keys.updateKeys();

            movement();
            look();
            jump();
            dash();
            shoot();

            if(!enemy) Physics.Simulate(Time.fixedDeltaTime);
            
            keys.updateTick();
            sendPlayerKeys(prekeys);
        }
    }

    private void storeBufferKeysAndState() {
        int bufferSlot = keys.tick % maxBufferSize;

        playerKeysBuffer[bufferSlot] = keys.deepCopy();

        playerStateBuffer[bufferSlot].position = rb.transform.position;
        playerStateBuffer[bufferSlot].rotation = orientation.localRotation;
        playerStateBuffer[bufferSlot].camRotation = playerCam.localRotation;
        playerStateBuffer[bufferSlot].velocity = rb.velocity;
        playerStateBuffer[bufferSlot].angularVelocity = rb.angularVelocity;
    }

    private void look(){
        if(enemy) return;

        if(Camera.main != null) displayName.transform.LookAt(Camera.main.transform);
        displayName.transform.Rotate(0, 180, 0);

        float mX = keys.mouseX * mouseSensitivity * Time.deltaTime;
        float mY = keys.mouseY * mouseSensitivity * Time.deltaTime;

        Vector3 rot = playerCam.localRotation.eulerAngles;
        float desiredX = rot.y + mX;

        xRotation -= mY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCam.localRotation = Quaternion.Euler(xRotation, desiredX, 0);
        orientation.localRotation = Quaternion.Euler(0, desiredX, 0);
    }

    private void movement() {
        isGrounded = Physics.OverlapSphere(groundCheck.position, groundCheckRadius, whatIsGround).Length > 0;

        runningAnimation();

        if(enemy) return;

        Vector2 mag = FindVelRelativeToLook();
        CounterMovement(keys.x, keys.y, mag);

        if (keys.x > 0 && mag.x > maxSpeed) keys.x = 0;
        if (keys.x < 0 && mag.x < -maxSpeed) keys.x = 0;
        if (keys.y > 0 && mag.y > maxSpeed) keys.y = 0;
        if (keys.y < 0 && mag.y < -maxSpeed) keys.y = 0;

        float multiplier = 1f;
        
        if (!isGrounded) multiplier = 0.5f;

        rb.AddForce(orientation.forward * keys.y * moveSpeed * rb.mass * Time.deltaTime * multiplier);
        rb.AddForce(orientation.right * keys.x * moveSpeed * rb.mass * Time.deltaTime * multiplier);
    }

    private void CounterMovement(float x, float y, Vector2 mag) {
        if (!isGrounded) return;
        
        if (Mathf.Abs(mag.x) > threshold && Mathf.Abs(x) < 0.05f || (mag.x < -threshold && x > 0) || (mag.x > threshold && x < 0)) {
            rb.AddForce(moveSpeed * orientation.right  * rb.mass * Time.deltaTime * -mag.x * counterMovement);
        }
        if (Mathf.Abs(mag.y) > threshold && Mathf.Abs(y) < 0.05f || (mag.y < -threshold && y > 0) || (mag.y > threshold && y < 0)) {
            rb.AddForce(moveSpeed * orientation.forward  * rb.mass * Time.deltaTime * -mag.y * counterMovement);
        }
        
        //Limit diagonal running
        if (Mathf.Sqrt((Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2))) > maxSpeed) {
            float fallspeed = rb.velocity.y;
            Vector3 n = rb.velocity.normalized * maxSpeed;
            rb.velocity = new Vector3(n.x, fallspeed, n.z);
        }
    }

    private Vector2 FindVelRelativeToLook() {
        float lookAngle = orientation.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = rb.velocity.magnitude;
        float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);
        
        return new Vector2(xMag, yMag);
    }

    private void runningAnimation() {
        if(!isGrounded && animations.GetBool("Running")){
            animations.SetBool("Running", false);
        } 

        if((keys.x == 0 && keys.y == 0) && isGrounded && animations.GetBool("Running")){
            animations.SetBool("Running", false);
        } 

        if((keys.x != 0 || keys.y != 0) && isGrounded && !animations.GetBool("Running")) {
            animations.SetBool("Running", true);
        }
    }

    private void restartJump() {
        readyToJump = true;
    }

    private void restartDash() {
        isDashing = false;
    }

    private void resetFinishDashDelay() {
        finishDashDelay = true;
    }
    
    void LateUpdate() {
        if(alive) head.localRotation = playerCam.localRotation;
    }

    private void updateHealth() {
        GameController.instance.setHealth(health);
        GameController.instance.setBlock(block);
    }

    private void shoot() {
        if(keys.mouseLeft && Time.time >= nextTimeToFire) {
            nextTimeToFire = Time.time + 2f / fireRate;
            playerGun.shoot();
            SoundManager.PlaySound("shoot");
        }
    }

    private void jump() {
        if(isGrounded && animations.GetBool("Jumping") && readyToJump){
            animations.SetBool("Jumping", false);
            Destroy(Instantiate(landPS, new Vector3(rb.transform.position.x, rb.transform.position.y-1, rb.transform.position.z), Quaternion.identity), 1);
            SoundManager.PlaySound("land");
        }

        if(!isGrounded && !animations.GetBool("Jumping") && !animations.GetBool("Running")) {
            animations.SetBool("Jumping", true);
        }

        if(keys.jumping && isGrounded && readyToJump && !animations.GetBool("Jumping")){
            animations.SetBool("Jumping", true);
            Destroy(Instantiate(jumpPS, new Vector3(rb.transform.position.x, rb.transform.position.y-1, rb.transform.position.z), Quaternion.identity), 1);
            SoundManager.PlaySound("jump");

            if(!enemy){
                rb.AddForce(Vector3.up * jumpHeight * rb.mass);

                Vector3 vel = rb.velocity;
                if (rb.velocity.y < 0.5f)
                    rb.velocity = new Vector3(vel.x, 0, vel.z);
                else if (rb.velocity.y > 0) 
                    rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);
            }

            Invoke("restartJump", jumpDelay);
            readyToJump = false;
        }
    }

    private void dash() {
        if(keys.mouseRight && !isGrounded && !isDashing && finishDashDelay){
            isDashing = true;
            finishDashDelay = false;

            if (keys.y != 0) {
                dashGO.transform.localPosition = new Vector3(0, 0, keys.y * 1.2f);
                dashGO.transform.localRotation = Quaternion.Euler(0, keys.y >= 0 ? 180 : 0, 0);
                dashPS.Play();
                SoundManager.PlaySound("dash");
            } else if (keys.x != 0) {
                dashGO.transform.localPosition = new Vector3(keys.x * 1.2f, 0, 0);
                dashGO.transform.localRotation = Quaternion.Euler(0, keys.x * -90, 0);
                dashPS.Play();
                SoundManager.PlaySound("dash");
            }

            Invoke("resetFinishDashDelay", dashDelay);
            Invoke("restartDash", dashExecutionTimeDelay);
        }

        if(isDashing && !enemy) {
            if (keys.y != 0) rb.AddForce(rb.transform.forward * keys.y * dashForce, ForceMode.Acceleration);
            if (keys.x != 0) rb.AddForce(rb.transform.right * keys.x * dashForce, ForceMode.Acceleration);
        }
    }

    public void sendPlayerKeys(Keys sendKeys) {
        if(enemy) return;

        Packet packet = new Packet();
        packet.Write("playerKeysTS");
        packet.Write(id);
        packet.Write(sendKeys);
        Client.instance.sendUdpData(packet);
    }

    public void playerPosition(Packet packet) {
        Vector3 position = packet.ReadVector3();
        Quaternion rotation = packet.ReadQuaternion();
        Quaternion camRotation = packet.ReadQuaternion();
        Vector3 velocity = packet.ReadVector3();
        Vector3 angularVelocity = packet.ReadVector3();

        isGrounded = packet.ReadBool();
        health = packet.ReadFloat();
        block = packet.ReadFloat();
        Keys auxKeys = packet.ReadKeys();
        if(enemy) keys = auxKeys;

        ThreadManager.ExecuteOnMainThread(() => {
            updateHealth();
            playerPositionUpdate(id, position, rotation, camRotation, velocity, angularVelocity, auxKeys.tick);
        });
    }

    float serverTickExecuted = 0;
    public float maxErrorDistance = 0;

    public void playerPositionUpdate(int id, Vector3 position, Quaternion rotation, Quaternion camRotation, Vector3 velocity, Vector3 angularVelocity, int serverTick) {
        if(player == null || serverTick <= serverTickExecuted || serverTick > keys.tick) return;
        serverTickExecuted = serverTick;

        Vector3 prePosition = rb.transform.position;
        int bufferSlot = serverTick % maxBufferSize;
        Vector3 positionError = position - playerStateBuffer[bufferSlot].position;

        Debug.DrawLine(playerStateBuffer[bufferSlot].position, position, Color.red, 1f);

        if(positionError.sqrMagnitude > maxErrorDistance) {
            rb.transform.position = position;
            rb.velocity = velocity;
            rb.angularVelocity = angularVelocity;

            if(enemy) {
                float vel = 10000;
                orientation.localRotation = Quaternion.Lerp(orientation.localRotation, rotation, Time.deltaTime * vel);
                playerCam.localRotation = Quaternion.Lerp(playerCam.localRotation, camRotation, Time.deltaTime * vel);
            }

            int lastTick = keys.tick;
            reconciliation = true;

            for(int rewindTick = serverTick; rewindTick < lastTick; rewindTick++) {
                int bufferSlotRewind = rewindTick % maxBufferSize;

                playerStateBuffer[bufferSlotRewind].position = rb.transform.position;
                playerStateBuffer[bufferSlotRewind].rotation = orientation.localRotation;
                playerStateBuffer[bufferSlotRewind].camRotation = playerCam.localRotation;
                playerStateBuffer[bufferSlotRewind].velocity = rb.velocity;
                playerStateBuffer[bufferSlotRewind].angularVelocity = rb.angularVelocity;

                keys = playerKeysBuffer[bufferSlotRewind];

                movement();
                look();
                jump();
                dash();
                shoot();

                if(!enemy) Physics.Simulate(Time.fixedDeltaTime);
            }

        // //     Vector3 offsetPosition = rb.transform.position - prePosition;
        // //     // MoveCamera.instance.desyncOffeset = -offsetPosition;
        // //     Debug.Log(offsetPosition);

            reconciliation = false;
        }

        // Debug.DrawLine(prePosition, rb.transform.position, Color.green, 1f);
    }

    public void removePlayer() {
        ThreadManager.ExecuteOnMainThread(() => {
            Client.instance.removeFromPlayers(id);
            GameController.instance.setLog("Player [id: "+id+"] has disconnect!");
            Destroy(player);
        });
    }

    public void creatShootImpact(Packet packet) {
        ThreadManager.ExecuteOnMainThread(() => {
            Vector3 hitPoint = packet.ReadVector3();
            Quaternion rotationPoint = packet.ReadQuaternion();
            bool isPlayer = packet.ReadBool();
            Destroy(Instantiate(impactPS, hitPoint, rotationPoint), 1);
            if(isPlayer) {
                Destroy(Instantiate(bloodPS, hitPoint, rotationPoint), 1);
                SoundManager.PlaySound("hit");
                if(enemy) GameController.instance.enableDamage();
            }
        });
    }

    public void killPlayer(Packet packet) {
        alive = false;
        playerModel.SetActive(false);
        playerGun.setVisible(false);
    }

    public void respawnPlayer(Packet packet) {
        alive = true;
        playerModel.SetActive(true);
        playerGun.setVisible(true);
    }

    public void setId(int id) {
        this.id = id;
    }

    public void setUsername(string username) {
        this.username = username;
    }

    public void makeItEnemy() {
        enemy = true;
        // camera.SetActive(false);
        // camera.tag = "Enemy";
        displayName.text = username;
        setLayerRecursively(playerModel, LayerMask.NameToLayer("Enemy"));
    }

    private void setLayerRecursively(GameObject obj, LayerMask newLayer) {
        if(null == obj) return;
       
        obj.layer = newLayer;
       
        foreach(Transform child in obj.transform) {
            if (null == child) continue;
            setLayerRecursively(child.gameObject, newLayer);
        }
    }
}
