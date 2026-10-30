using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    [Header("Покачивание камеры")]
    [SerializeField] private float _bobFrequency = 5.0f; // Скорость шагов
    [SerializeField] private float _bobAmount = 0.05f; // Высота покачивания
    private float _bobTimer = 0f;
    private float _defaultCameraY = 0f;

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

    private void Awake() {
        if (_characterController == null) _characterController = GetComponent<CharacterController>();
        if (_playerInput == null) _playerInput = GetComponent<PlayerInput>();
        if (_cameraTransform == null) _cameraTransform = GetComponentInChildren<Camera>().transform;

        if (_cameraTransform != null) _defaultCameraY = _cameraTransform.localPosition.y;
    }

    private void Update() {
        if (Time.timeScale == 0f) return; // Защита от движения в паузе

        // 1. Поворот тела персонажа влево/вправо
        float horizontalRotation = _playerInput.MouseX * _mouseSensitivity;
        transform.Rotate(Vector3.up * horizontalRotation);

        // 2. Наклон камеры вверх/вниз
        _verticalRotation -= _playerInput.MouseY * _mouseSensitivity;
        _verticalRotation = Mathf.Clamp(_verticalRotation, -85f, 85f); // Ограничиваем угол обзора, чтобы не сломать шею
        _cameraTransform.localRotation = Quaternion.Euler(_verticalRotation, 0f, 0f);

        // 3. Расчет направления движения по горизонтали (WASD)
        Vector3 moveDirection = transform.right * _playerInput.MoveX + transform.forward * _playerInput.MoveZ;
        Vector3 finalMovement = moveDirection * _walkSpeed;

        // 4. Логика гравитации (прижим к земле)
        if (_characterController.isGrounded) {
            _verticalVelocity = -2f;
        }
        else {
            // Если мы оказались в воздухе, плавно увеличиваем скорость падения
            _verticalVelocity += _gravity * Time.deltaTime;
        }

        // Добавляем вертикальную скорость к общему движению
        finalMovement.y = _verticalVelocity;

        // Покачивание камеры по синусоиде
        Vector3 speedXZ = new Vector3(finalMovement.x, 0, finalMovement.z);
        if (_characterController.isGrounded && speedXZ.magnitude > 0.1f) {
            _bobTimer += Time.deltaTime * _bobFrequency;
            _cameraTransform.localPosition = new Vector3(
                _cameraTransform.localPosition.x,
                _defaultCameraY + Mathf.Sin(_bobTimer) * _bobAmount, // Синус создает плавную волну
                _cameraTransform.localPosition.z
            );
        }
        else {
            // Игрок стоит - плавно возвращаем камеру в центр через Lerp
            _bobTimer = 0f;
            _cameraTransform.localPosition = new Vector3(
                _cameraTransform.localPosition.x,
                Mathf.Lerp(_cameraTransform.localPosition.y, _defaultCameraY, Time.deltaTime * 5f),
                _cameraTransform.localPosition.z
            );
        }
        // 5. Финальное перемещение персонажа
        _characterController.Move(finalMovement * Time.deltaTime); // Двигаем контроллер
    }
}