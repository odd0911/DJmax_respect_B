using UnityEngine;
using System.Collections.Generic;

public class NotePool : MonoBehaviour
{
    public static NotePool Instance { get; private set; }
    public GameObject notePrefab;
    public int poolSize = 50;

    private readonly Queue<Note> notePool = new Queue<Note>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            ExpandPool();
        }
    }

    private void ExpandPool()
    {
        GameObject noteObj = Instantiate(notePrefab);
        Note note = noteObj.GetComponent<Note>();

        noteObj.SetActive(false);
        notePool.Enqueue(note);
    }

    public GameObject GetNote()
    {
        if (notePool.Count == 0)
        {
            ExpandPool(); // 풀이 부족할 때 자동 확장
        }

        Note note = notePool.Dequeue();
        note.gameObject.SetActive(true);
        return note.gameObject;
    }

    public void ReturnNote(GameObject noteObj)
    {
        Note note = noteObj.GetComponent<Note>();
        if (note == null) return;

        noteObj.SetActive(false);
        notePool.Enqueue(note);
    }
}
