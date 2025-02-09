using UnityEngine;

public class Note : MonoBehaviour
{
    public float spawnY = 5.0f;
    public float judgeY = -2.0f;
    public float baseFallTime = 2.5f;

    private float fallSpeed;

    void OnEnable()
    {
        if (GameManager.Instance != null)
    {
        GameManager.Instance.RegisterNote(this);
        UpdateSpeed();
    }
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
        fallSpeed = (spawnY - judgeY) / (baseFallTime / speedMultiplier);
    }

    void Update()
    {
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;

        if (transform.position.y <= judgeY - 1)
        {
            NotePool.Instance.ReturnNote(gameObject);
        }
    }
}


