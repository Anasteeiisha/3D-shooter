using System.Collections;
using UnityEngine;

public class PlayerShooting : MonoBehaviour {
    [Header("Настройки оружия")]
    [SerializeField] private int _damage = 34;
    [SerializeField] private float _fireRange = 100f;

    [Header("Линия стрельбы")]
    [SerializeField] private Transform _gunMuzzle; // Точка вылета пули
    [SerializeField] private LineRenderer _lineRenderer; // Линия выстрела

    [Header("Звуковые эффекты")]
    [SerializeField] private AudioSource _audioSource; // Сюда перетащим пистолет с Audio Source
    [SerializeField] private AudioClip _shootSound; // Сюда закинем аудиофайл звука выстрела

    private Camera _mainCamera;

    private void Start() {
        _mainCamera = Camera.main; // Находим главную камеру
        if (_lineRenderer != null) _lineRenderer.enabled = false; // Прячем линию при старте
    }

    private void Update() {
        if (Time.timeScale == 0f) return; // Защита от стрельбы в паузе

        if (Input.GetMouseButtonDown(0)) { // Если нажата левая кнопка мыши
            Shoot(); // Стреляем
        }
    }

    private void Shoot() {
        //  Играет звук выстрела без прерывания предыдущего
        if (_audioSource != null && _shootSound != null) {
            _audioSource.PlayOneShot(_shootSound);
        }

        // Создаем луч из центра экрана (0.5, 0.5) в направлении взгляда камеры
        Ray ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit, _fireRange)) { // Если луч во что-то попал
            targetPoint = hit.point; // Запоминаем точку попадания

            EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>(); // Ищем скрипт врага
            if (enemy != null) {
                enemy.TakeDamage(_damage); // Наносим урон
            }
        }
        else {
            // Если никуда не попали, рисуем линию в пустоту на максимальную дистанцию
            targetPoint = ray.origin + ray.direction * _fireRange;
        }

        if (_lineRenderer != null && _gunMuzzle != null) {
            // Запускаем отрисовку линии от ствола до точки попадания
            StartCoroutine(RenderLineCoroutine(_gunMuzzle.position, targetPoint));
        }
    }

    private IEnumerator RenderLineCoroutine(Vector3 start, Vector3 end) {
        _lineRenderer.enabled = true; // Включаем линию
        _lineRenderer.SetPosition(0, start); // Начало линии (ствол)
        _lineRenderer.SetPosition(1, end); // Конец линии (точка попадания)
        yield return new WaitForSeconds(0.05f); // Ждем 0.05 сек
        _lineRenderer.enabled = false; // Выключаем линию (эффект мигания)
    }
}