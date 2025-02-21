using UnityEngine;

public class Note : MonoBehaviour
{
    
    public float judgeY = -2.0f;
    public float baseFallTime = 2.5f;

    private float fallSpeed;
    private float spawnTime;
    private float baseSpawnY = 5.0f;
    private float spawnY = 5.0f;

    void OnEnable()
    {
        if (GameManager.Instance != null)
    {
        GameManager.Instance.RegisterNote(this);
        UpdateSpeed();
    }
    }

    public void Initialize(float spawnY, float hitY)
    {
        this.spawnTime = Time.time * 1000; // ms 단위로 저장
        this.spawnY = spawnY;
        this.judgeY = hitY;
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
        {
        GameManager.Instance.UnregisterNote(this);
        }
    }

    public void UpdateSpeed()
    {
        float speedMultiplier = GameManager.Instance.speedMultiplier;
        spawnY = baseSpawnY * speedMultiplier;
    }

    void Update()
    {
        float speedMultiplier = GameManager.Instance.speedMultiplier;
        spawnY = baseSpawnY * speedMultiplier;
        float currentTime = Time.time * 1000; // ms 단위
        float elapsed = currentTime - spawnTime;

        // 새로운 위치 계산 (ms 단위 / 기준 이동 시간)
        float newY = spawnY - (elapsed / 2500f) * (spawnY - judgeY);
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}



