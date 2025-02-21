using UnityEngine;
using System.Collections.Generic;

public class NotePool : MonoBehaviour
{
    public static NotePool Instance; // 싱글톤 패턴
    public GameObject notePrefab;
    public int poolSize = 50; // 초기 생성 개수

    private Queue<GameObject> notePool = new Queue<GameObject>();

    void Awake()
    {
        Instance = this;
        InitializePool();
    }

    void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject noteObj = Instantiate(notePrefab);
            noteObj.SetActive(false); // 비활성화 전에 GameManager 등록 방지
            noteObj.GetComponent<Note>().enabled = false; // Note 스크립트 비활성화
            notePool.Enqueue(noteObj);
        }
    }

    public GameObject GetNote()
    {
        if (notePool.Count > 0)
    {
        GameObject noteObj = notePool.Dequeue();
        noteObj.SetActive(true);
        noteObj.GetComponent<Note>().enabled = true; // Note 스크립트 다시 활성화
        return noteObj;
    }
    return Instantiate(notePrefab); // 풀이 부족하면 새로 생성
    }

    public void ReturnNote(GameObject note)
    {
        note.SetActive(false);
        notePool.Enqueue(note);
    }
}