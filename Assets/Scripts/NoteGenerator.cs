using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class NoteGenerator : MonoBehaviour
{
    public TextAsset sheetFile;
    public Transform[] lanes;
    public float speedMultiplier = 1.0f;
    public int totalNotes;
    public static NoteGenerator Instance;

    private List<NoteData> notes = new List<NoteData>();
    private int offset;
    private bool musicPlaying = false;
    private const float BaseSpawnHeight = 5f;
    private const float HitPositionY = -2f;
    private EventInstance musicEventInstance; // FMOD 이벤트 인스턴스

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (sheetFile != null)
        {
            ParseSheet(sheetFile.text);
            StartCoroutine(SpawnNotes());
            Invoke("PlayMusic", 2.2f);
            // FMOD 이벤트 인스턴스를 생성하고, 해당 경로로 이벤트 로드
            musicEventInstance = FMODUnity.RuntimeManager.CreateInstance("event:/Usagi_Flap");
        }
    }
    void PlayMusic()
    {   
        // 음악 이벤트 재생
        musicEventInstance.start();
        musicPlaying = true;
    }

    void Update()
    {
        // 음악이 이미 종료되었으면 더 이상 검사를 하지 않도록
    if (!GameManager.Instance.musicEnd && musicPlaying)
    {
        PLAYBACK_STATE playbackState;
        
        // 음악 재생 상태를 가져옴
        if (musicEventInstance.getPlaybackState(out playbackState) == FMOD.RESULT.OK)
        {
            // 음악이 멈춘 상태(STOPPED)라면
            if (playbackState == PLAYBACK_STATE.STOPPED)
            {
                Debug.Log("음악이 종료되었습니다!");
                GameManager.Instance.musicEnd = true; // 종료 상태를 true로 변경
            }
        }
        else
        {
            Debug.LogWarning("음악 상태를 가져오는 데 실패했습니다.");
        }
    }
    }

    private void ParseSheet(string sheetContent)
    {
        bool noteSection = false;
        foreach (var line in sheetContent.Split('\n'))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;

            if (trimmed.StartsWith("[Note]"))
            {
                noteSection = true;
                continue;
            }

            if (noteSection)
            {
                var data = trimmed.Split(',');
                if (data.Length >= 3 &&
                    int.TryParse(data[0], out int time) &&
                    int.TryParse(data[1], out int type) &&
                    int.TryParse(data[2], out int lane))
                {
                    lane = Mathf.Clamp(lane - 1, 0, lanes.Length - 1);
                    int endTime = (type == 1 && data.Length == 4 && int.TryParse(data[3], out int parsedEndTime)) 
                                    ? parsedEndTime 
                                    : 0;

                    notes.Add(new NoteData(time, type, lane, endTime));
                }
            }
            else if (trimmed.StartsWith("Offset:") && 
                     int.TryParse(trimmed.Split(':')[1].Trim(), out int parsedOffset))
            {
                offset = parsedOffset;
            }
            
        }
        totalNotes = notes.Count;
    }

    private IEnumerator SpawnNotes()
    {
        float startTime = Time.time;
        int noteIndex = 0;
        Vector3 spawnPosition = Vector3.zero;

        while (noteIndex < notes.Count)
        {
            var note = notes[noteIndex];
            float noteTime = (note.time - offset) / 1000f;
            float elapsed = Time.time - startTime;

            if (elapsed >= noteTime)
            {
                spawnPosition = lanes[note.lane].position;
                spawnPosition.y = BaseSpawnHeight;

                var noteObj = NotePool.Instance.GetNote();
                noteObj.transform.position = spawnPosition;

                var noteScript = noteObj.GetComponent<Note>();
                noteScript.Initialize(BaseSpawnHeight, HitPositionY);
                noteScript.laneIndex = note.lane;

                GameManager.Instance.laneNotes[note.lane].Add(noteScript);
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

