using UnityEngine;

public class PlayerState {
    public Vector3 position;
    public Quaternion rotation;
    public Quaternion camRotation;
    public Vector3 velocity;
    public Vector3 angularVelocity;

    public PlayerState() {
        position = Vector3.zero;
        velocity = Vector3.zero;
        angularVelocity = Vector3.zero;
    }
}