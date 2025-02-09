using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    // Z키에 해당하는 스프라이트를 연결할 변수
    public GameObject keyInput1;
    public GameObject keyInput2;
    public GameObject keyInput3;
    public GameObject keyInput4;
    public GameObject gearKeyInput1;
    public GameObject gearKeyInput2;
    public GameObject gearKeyInput3;
    public GameObject gearKeyInput4;
    public static GameManager Instance;
    public float speedMultiplier = 3.0f; // 기본 배속 1배
    public float minSpeed = 1.0f;
    public float maxSpeed = 9.0f;
    public float speedStep = 0.1f;

    private List<Note> activeNotes = new List<Note>();
    private List<LongNote> activeLongNotes = new List<LongNote>();


    void Awake()
    {
        Instance = this;
    }

    // Start 함수에서 스프라이트 비활성화
    void Start()
    {
        if (keyInput1 != null)
        {
            keyInput1.SetActive(false); // 게임 시작 시 스프라이트 비활성화
        }
        if (keyInput2 != null)
        {
            keyInput2.SetActive(false); // 게임 시작 시 스프라이트 비활성화
        }
        if (keyInput3 != null)
        {
            keyInput3.SetActive(false); // 게임 시작 시 스프라이트 비활성화
        }
        if (keyInput4 != null)
        {
            keyInput4.SetActive(false); // 게임 시작 시 스프라이트 비활성화
        }
        if (gearKeyInput1 != null)
        {
            gearKeyInput1.SetActive(false); // 게임 시작 시 스프라이트 비활성화
        }
        if (gearKeyInput2 != null)
        {
            gearKeyInput2.SetActive(false); // 게임 시작 시 스프라이트 비활성화
        }
        if (gearKeyInput3 != null)
        {
            gearKeyInput3.SetActive(false); // 게임 시작 시 스프라이트 비활성화
        }
        if (gearKeyInput4 != null)
        {
            gearKeyInput4.SetActive(false); // 게임 시작 시 스프라이트 비활성화
        }
    }

    // Update 함수에서 키 입력 감지
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) // 배속 감소
        {
            ChangeSpeed(speedMultiplier - speedStep);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) // 배속 증가
        {
            ChangeSpeed(speedMultiplier + speedStep);
        }
        // Z키가 눌렸을 때
        if (Input.GetKeyDown(KeyCode.Z))
        {
            // 스프라이트를 활성화
            if (keyInput1 != null)
            {
                keyInput1.SetActive(true);
            }
            if (gearKeyInput1 != null)
            {
                gearKeyInput1.SetActive(true);
            }
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            // 스프라이트를 활성화
            if (keyInput2 != null)
            {
                keyInput2.SetActive(true);
            }
            if (gearKeyInput2 != null)
            {
                gearKeyInput2.SetActive(true);
            }
        }
        if (Input.GetKeyDown(KeyCode.Period))
        {
            // 스프라이트를 활성화
            if (keyInput3 != null)
            {
                keyInput3.SetActive(true);
            }
            if (gearKeyInput3 != null)
            {
                gearKeyInput3.SetActive(true);
            }
        }
        if (Input.GetKeyDown(KeyCode.Slash))
        {
            // 스프라이트를 활성화
            if (keyInput4 != null)
            {
                keyInput4.SetActive(true);
            }
            if (gearKeyInput4 != null)
            {
                gearKeyInput4.SetActive(true);
            }
        }

        // Z키가 떼어졌을 때 (원하는 경우 스프라이트를 다시 비활성화)
        if (Input.GetKeyUp(KeyCode.Z))
        {
            if (keyInput1 != null)
            {
                keyInput1.SetActive(false); // 스프라이트 비활성화
            }
            if (gearKeyInput1 != null)
            {
                gearKeyInput1.SetActive(false); // 스프라이트 비활성화
            }
        }
        if (Input.GetKeyUp(KeyCode.X))
        {
            if (keyInput2 != null)
            {
                keyInput2.SetActive(false); // 스프라이트 비활성화
            }
            if (gearKeyInput2 != null)
            {
                gearKeyInput2.SetActive(false); // 스프라이트 비활성화
            }
        }
        if (Input.GetKeyUp(KeyCode.Period))
        {
            if (keyInput3 != null)
            {
                keyInput3.SetActive(false); // 스프라이트 비활성화
            }
            if (gearKeyInput3 != null)
            {
                gearKeyInput3.SetActive(false); // 스프라이트 비활성화
            }
        }
        if (Input.GetKeyUp(KeyCode.Slash))
        {
            if (keyInput4 != null)
            {
                keyInput4.SetActive(false); // 스프라이트 비활성화
            }
            if (gearKeyInput4 != null)
            {
                gearKeyInput4.SetActive(false); // 스프라이트 비활성화
            }
        }
    }
    public void ChangeSpeed(float newSpeed)
    {
        speedMultiplier = Mathf.Clamp(newSpeed, minSpeed, maxSpeed);
        Debug.Log($"배속 변경: {speedMultiplier}x");

        // 현재 활성화된 모든 노트의 속도를 업데이트
        foreach (var note in activeNotes)
        {
            note.UpdateSpeed();
        }

        foreach (var longNote in activeLongNotes)
        {
            longNote.UpdateSpeed();
        }
    }

    public void RegisterNote(Note note)
    {
        activeNotes.Add(note);
    }

    public void UnregisterNote(Note note)
    {
        activeNotes.Remove(note);
    }

    public void RegisterLongNote(LongNote longNote)
    {
        activeLongNotes.Add(longNote);
    }

    public void UnregisterLongNote(LongNote longNote)
    {
        activeLongNotes.Remove(longNote);
    }
}
