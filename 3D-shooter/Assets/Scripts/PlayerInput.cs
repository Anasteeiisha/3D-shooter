using UnityEngine;

public class PlayerInput : MonoBehaviour 
{
    public float MoveX { get; private set; }
    public float MoveZ { get; private set; }
    public float MouseX { get; private set; }
    public float MouseY { get; private set; } 

    private void Start() 
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update() 
    {
        MoveX = Input.GetAxis("Horizontal");
        MoveZ = Input.GetAxis("Vertical");
        MouseX = Input.GetAxis("Mouse X");
        MouseY = Input.GetAxis("Mouse Y");
    }
}