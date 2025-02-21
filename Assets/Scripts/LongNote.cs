using UnityEngine;

public class LongNote : MonoBehaviour
{
    public float judgeY = -2.0f;
    public float baseFallTime = 2.5f;

    private float fallSpeed;
    private float longNoteLength;
    private int startTime;
    private int endTime;
    private float spawnTime;
    private float baseSpawnY = 5.0f;
    private float spawnY = 5.0f;

    void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterLongNote(this);
            UpdateSpeed();
        }
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterLongNote(this);
        }
    }

    public void Initialize(int startTime, int endTime)
    {
        this.startTime = startTime;
        this.endTime = endTime;
        UpdateSpeed();
    }

    public void UpdateSpeed()
    {
        float speedMultiplier = GameManager.Instance.speedMultiplier;
        spawnY = baseSpawnY * speedMultiplier;

    }

    void Update()
    {
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;

        if (transform.position.y + longNoteLength <= judgeY - 1)
        {
            LongNotePool.Instance.ReturnLongNote(gameObject);
        }
    }
}
