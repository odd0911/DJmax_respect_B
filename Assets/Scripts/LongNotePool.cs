using UnityEngine;
using System.Collections.Generic;

public class LongNotePool : MonoBehaviour
{
    public static LongNotePool Instance;
    public GameObject longNotePrefab;
    public int poolSize = 20; // 초기 생성 개수

    private Queue<GameObject> longNotePool = new Queue<GameObject>();

    void Awake()
    {
        Instance = this;
        InitializePool();
    }

    void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject longNote = Instantiate(longNotePrefab);
            longNote.SetActive(false);
            longNotePool.Enqueue(longNote);
        }
    }

    public GameObject GetLongNote()
    {
        if (longNotePool.Count > 0)
        {
            GameObject longNote = longNotePool.Dequeue();
            longNote.SetActive(true);
            return longNote;
        }
        else
        {
            return Instantiate(longNotePrefab);
        }
    }

    public void ReturnLongNote(GameObject longNote)
    {
        longNote.SetActive(false);
        longNotePool.Enqueue(longNote);
    }
}