using System.Collections;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("Настройки оружия")]
    [SerializeField] private int _damage = 34; 
    [SerializeField] private float _fireRange = 100f;

    [Header("Линия стрельбы")]
    [SerializeField] private Transform _gunMuzzle;       
    [SerializeField] private LineRenderer _lineRenderer;   

    [Header("Звуковые эффекты")]
    [SerializeField] private AudioSource _audioSource;     // Сюда перетащим пистолет с Audio Source
    [SerializeField] private AudioClip _shootSound;        // Сюда закинем аудиофайл звука выстрела

    private Camera _mainCamera;

    private void Start()
    {
        _mainCamera = Camera.main;
        if (_lineRenderer != null) _lineRenderer.enabled = false;
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;

        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        // ВОСПРОИЗВЕДЕНИЕ ЗВУКА: Играет звук выстрела без прерывания предыдущего
        if (_audioSource != null && _shootSound != null)
        {
            _audioSource.PlayOneShot(_shootSound);
        }

        Ray ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit, _fireRange))
        {
            targetPoint = hit.point;

            EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(_damage);
            }
        }
        else
        {
            targetPoint = ray.origin + ray.direction * _fireRange;
        }

        if (_lineRenderer != null && _gunMuzzle != null)
        {
            StartCoroutine(RenderLineCoroutine(_gunMuzzle.position, targetPoint));
        }
    }

    private IEnumerator RenderLineCoroutine(Vector3 start, Vector3 end)
    {
        _lineRenderer.enabled = true;
        _lineRenderer.SetPosition(0, start); 
        _lineRenderer.SetPosition(1, end);   
        yield return new WaitForSeconds(0.05f); 
        _lineRenderer.enabled = false;
    }
}