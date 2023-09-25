using UnityEngine;

public class Keys {
    public float x { get; set; }
    public float y { get; set; }
    public float mouseX { get; set; }
    public float mouseY { get; set; }
    public bool jumping { get; set; }
    public bool mouseLeft { get; set; }
    public bool mouseRight { get; set; }
    public bool shift { get; set; }

    public Quaternion playerRotation { get; set; }
    public Quaternion camRotation { get; set; }
    
    public int tick { get; set; }

    public void updateKeys(){
        x = Input.GetAxisRaw("Horizontal");
        y = Input.GetAxisRaw("Vertical");
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");
        mouseLeft = Input.GetMouseButton(0);
        mouseRight = Input.GetMouseButton(1);
        jumping = Input.GetButton("Jump");
        shift = Input.GetKey(KeyCode.LeftShift);
    }

    public void updateTick() {
        tick++;
    }

    public void updatePlayerRotation(Quaternion rotation) {
        playerRotation = rotation;
    }

    public void updateCamRotation(Quaternion rotation) {
        camRotation = rotation;
    }
    
    public Keys deepCopy() {
        Keys copyKeys = new Keys();
        copyKeys.x = this.x;
        copyKeys.y = this.y;
        copyKeys.mouseX = this.mouseX;
        copyKeys.mouseY = this.mouseY;
        copyKeys.jumping = this.jumping;
        copyKeys.mouseLeft = this.mouseLeft;
        copyKeys.mouseRight = this.mouseRight;
        copyKeys.shift = this.shift;
        copyKeys.playerRotation = this.playerRotation;
        copyKeys.camRotation = this.camRotation;
        copyKeys.tick = this.tick;

        return copyKeys;
    }
}