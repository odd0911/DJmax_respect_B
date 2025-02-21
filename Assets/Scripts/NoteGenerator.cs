using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class NoteGenerator : MonoBehaviour
{
    public TextAsset sheetFile;
    public Transform[] lanes;
    public float speedMultiplier = 1.0f;

    private List<NoteData> notes = new List<NoteData>();
    private int offset;
    private float baseSpawnHeight = 5f; // 스폰 위치의 Y 좌표
    private float hitPositionY = -2f; // 판정선 Y 좌표

    void Start()
    {
        if (sheetFile != null)
        {
            ParseSheet(sheetFile.text);
            StartCoroutine(SpawnNotes());
        }
    }

    void ParseSheet(string sheetContent)
    {
        string[] lines = sheetContent.Split('\n');
        bool noteSection = false;

        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;

            if (trimmed.StartsWith("[Note]"))
            {
                noteSection = true;
                continue;
            }

            if (noteSection)
            {
                string[] data = trimmed.Split(',');
                if (data.Length >= 3)
                {
                    int time = int.Parse(data[0]);
                    int type = int.Parse(data[1]);
                    int lane = int.Parse(data[2]) - 1;
                    int endTime = type == 1 && data.Length == 4 ? int.Parse(data[3]) : 0;

                    notes.Add(new NoteData(time, type, lane, endTime));
                }
            }
            else if (trimmed.StartsWith("Offset:"))
            {
                offset = int.Parse(trimmed.Split(':')[1].Trim());
            }
        }
    }

    System.Collections.IEnumerator SpawnNotes()
{
    float startTime = Time.time; // 코루틴 시작 시간

    int noteIndex = 0;
    while (noteIndex < notes.Count)
        {
            NoteData note = notes[noteIndex];
            float noteTime = (note.time - offset) / 1000f;

            if (Time.time - startTime >= noteTime)
            {
                Transform spawnPoint = lanes[note.lane];
                Vector3 spawnPosition = spawnPoint.position;
                spawnPosition.y = baseSpawnHeight;

                GameObject noteObj = NotePool.Instance.GetNote();
                noteObj.transform.position = spawnPosition;

                Note noteScript = noteObj.GetComponent<Note>();
                noteScript.Initialize(baseSpawnHeight, hitPositionY);

                noteIndex++;
            }
            else
            {
                yield return null;
            }
        }
}
}


[Serializable]
public class NoteData
{
    public int time;
    public int type;
    public int lane;
    public int endTime;

    public NoteData(int time, int type, int lane, int endTime)
    {
        this.time = time;
        this.type = type;
        this.lane = lane;
        this.endTime = endTime;
    }
}
