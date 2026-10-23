using UnityEngine;

public class PlayerMovement : MonoBehaviour 
{
    [Header("Movement Settings")]
    [SerializeField] private float _walkSpeed = 5.0f;
    [SerializeField] private float _mouseSensitivity = 2.0f;
    [SerializeField] private float _gravity = -9.81f; // Сила, которая будет тянуть нас вниз

    [Header("Refs")]
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Transform _cameraTransform; 

    private float _verticalRotation = 0f; 
    private float _verticalVelocity = 0f; // Переменная для хранения текущей скорости падения

    private void Awake() 
    {
        if (_characterController == null) _characterController = GetComponent<CharacterController>();
        if (_playerInput == null) _playerInput = GetComponent<PlayerInput>();
        if (_cameraTransform == null) _cameraTransform = GetComponentInChildren<Camera>()?.transform;
    }

    private void Update() 
    {
        // 1. Поворот ТЕЛА персонажа влево/вправо
        float horizontalRotation = _playerInput.MouseX * _mouseSensitivity;
        transform.Rotate(Vector3.up * horizontalRotation);

        // 2. Наклон КАМЕРЫ вверх/вниз
        _verticalRotation -= _playerInput.MouseY * _mouseSensitivity;
        _verticalRotation = Mathf.Clamp(_verticalRotation, -85f, 85f); 
        _cameraTransform.localRotation = Quaternion.Euler(_verticalRotation, 0f, 0f);

        // 3. Расчет направления движения по горизонтали (WASD)
        Vector3 moveDirection = transform.right * _playerInput.MoveX + transform.forward * _playerInput.MoveZ;
        Vector3 finalMovement = moveDirection * _walkSpeed;

        // 4. ЛОГИКА ГРАВИТАЦИИ (Прижим к земле)
        if (_characterController.isGrounded)
        {
            // Если мы твердо стоим на земле, слегка прижимаем игрока вниз (-2), чтобы физика не теряла пол
            _verticalVelocity = -2f; 
        }
        else
        {
            // Если мы оказались в воздухе (соскочили с коробки), плавно увеличиваем скорость падения
            _verticalVelocity += _gravity * Time.deltaTime;
        }

        // Добавляем вертикальную скорость к общему движению
        finalMovement.y = _verticalVelocity;

        // 5. Финальное перемещение персонажа
        _characterController.Move(finalMovement * Time.deltaTime);
    }
}