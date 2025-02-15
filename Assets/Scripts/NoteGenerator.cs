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
    foreach (var note in notes)
    {
        float spawnTime = (note.time - offset - (2500 / GameManager.Instance.speedMultiplier)) / 1000f;
        if (spawnTime > 0)
            yield return new WaitForSeconds(spawnTime);

        Transform spawnPoint = lanes[note.lane];

        if (note.type == 0) // 일반 노트
        {
            GameObject noteObj = NotePool.Instance.GetNote();
            noteObj.transform.position = spawnPoint.position;

            Note noteScript = noteObj.GetComponent<Note>();
            noteScript.UpdateSpeed();
        }
        else if (note.type == 1) // 롱노트
        {
            GameObject longNoteObj = LongNotePool.Instance.GetLongNote();
            longNoteObj.transform.position = spawnPoint.position;

            LongNote longNoteScript = longNoteObj.GetComponent<LongNote>();
            longNoteScript.Initialize(note.time, note.endTime);
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
