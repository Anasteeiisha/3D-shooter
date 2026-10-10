using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour {
    [Header("Настройки перемещения")]
    [SerializeField] private float moveSpeed = 5.0f;

    [Header("Настройки обзора")]
    [SerializeField] private float mouseSensitivity = 2.0f;

    private CharacterController _characterController;

    private void Start() {
        _characterController = GetComponent<CharacterController>();

        // Блокируем курсор мыши в центре экрана, чтобы он не мешал во время игры
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update() {
        // 1. Поворот персонажа мышью влево/вправо (по горизонтали)
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);

        // 2. Перемещение персонажа (клавиши WASD)
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Рассчитываем направление движения относительно взгляда персонажа
        Vector3 moveDirection = transform.right * moveX + transform.forward * moveZ;

        // Двигаем игрока через компонент CharacterController
        _characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
    }
}