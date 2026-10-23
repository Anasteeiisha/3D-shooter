using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("Настройки оружия")]
    [SerializeField] private int _damage = 25;       // Урон от одного выстрела
    [SerializeField] private float _fireRange = 100f; // Дистанция полета пули

    private Camera _mainCamera;

    private void Start()
    {
        // Находим главную камеру, чтобы пускать луч ровно из её центра
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        // Проверяем нажатие Левой Кнопки Мыши (0 — левая, 1 — правая)
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        Debug.Log("[Игрок] Сделал выстрел!");

        // Создаем луч, который идет из центра экрана вперед
        Ray ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        // Пускаем невидимый физический луч во все стороны
        if (Physics.Raycast(ray, out hit, _fireRange))
        {
            Debug.Log($"[Пуля] Попала в объект: {hit.collider.name}");

            // Проверяем, есть ли на объекте, в который мы попали, скрипт здоровья врага
            EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(_damage); // Наносим ему урон!
            }
        }
    }
}