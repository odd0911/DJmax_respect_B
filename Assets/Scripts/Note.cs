using UnityEngine;

public class Note : MonoBehaviour
{
    public int laneIndex;
    public float judgeY = -2.0f;
    public float baseFallTime = 2.5f;

    public float spawnTime;
    private float fallSpeed;
    private float spawnY = 5.0f;

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterNote(this);
            Initialize(spawnY, judgeY);
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterNote(this);
        }
    }

    public void Initialize(float spawnY, float hitY)
    {
        this.spawnTime = Time.time * 1000f; // ms 단위로 저장
        this.judgeY = hitY;
        UpdateSpeed();
    }

    public void UpdateSpeed()
    {
        fallSpeed = GameManager.Instance.speedMultiplier;
    }

    private void Update()
    {
        spawnY = GameManager.Instance.speedMultiplier*5;
        float elapsed = (Time.time * 1000f) - spawnTime; // ms 단위
        float newY = spawnY - (elapsed / 2500f) * (spawnY - judgeY);
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
