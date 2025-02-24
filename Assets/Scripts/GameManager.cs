using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject[] keyInputs = new GameObject[4];
    public GameObject[] gearKeyInputs = new GameObject[4];
    
    public static GameManager Instance;
    public float speedMultiplier = 3.0f;
    public float minSpeed = 1.0f;
    public float maxSpeed = 9.0f;
    public float speedStep = 0.1f;
    public int TotalNotes = 0;
    public int missNotes = 0;
    public TMP_Text ScoreText;
    public TMP_Text AccuracyText;
    public TMP_Text SpeedText;
    public bool musicEnd = false;
    public GameObject fadeOutUI;
    public Image ScoreIcon;
    public GameObject endScoreScreen;
    public Sprite[] scoreIcons;
    public TMP_Text[] resultNoteText;


    private int criticalNotes = 0;
    private int hitNotes = 0;
    private int blockNotes = 0;
    private float Score = 0f;
    private bool Faded = false;
    private Image fadeImage;
    private readonly List<Note> activeNotes = new List<Note>();
    private readonly List<LongNote> activeLongNotes = new List<LongNote>();
    public List<Note>[] laneNotes = new List<Note>[4];

    private readonly KeyCode[] inputKeys = { KeyCode.Z, KeyCode.X, KeyCode.Period, KeyCode.Slash };

    void Awake()
    {
        Instance = this;
        for (int i = 0; i < laneNotes.Length; i++)
            laneNotes[i] = new List<Note>();
    }

    void Start()
    {
        fadeImage = fadeOutUI.GetComponent<Image>();
        Color color = fadeImage.color;
        color.a = 0f;
        fadeImage.color = color;
        endScoreScreen.SetActive(false);
        ChangeSpeed(speedMultiplier);
        SetActiveAllKeyInputs(false);
        Invoke("GetTotalNote",1f);
    }

    void GetTotalNote()
    {
        TotalNotes = NoteGenerator.Instance.totalNotes;
    }

    void Update()
    {
        // 배속 조절
        if (Input.GetKeyDown(KeyCode.Alpha1)) ChangeSpeed(speedMultiplier - speedStep);
        if (Input.GetKeyDown(KeyCode.Alpha2)) ChangeSpeed(speedMultiplier + speedStep);

        // 키 입력 감지 및 처리
        for (int i = 0; i < inputKeys.Length; i++)
        {
            if (Input.GetKeyDown(inputKeys[i]))
            {
                CheckNoteInLane(i);
                SetKeyInputActive(i, true);
            }
            if (Input.GetKeyUp(inputKeys[i]))
            {
                SetKeyInputActive(i, false);
            }
        }
        if (musicEnd && !Faded) StartCoroutine(FadeOutAndInCoroutine());
    }

    private void SetActiveAllKeyInputs(bool active)
    {
        foreach (var input in keyInputs.Concat(gearKeyInputs))
        {
            if (input != null) input.SetActive(active);
        }
    }

    private void SetKeyInputActive(int index, bool active)
    {
        if (keyInputs[index] != null) keyInputs[index].SetActive(active);
        if (gearKeyInputs[index] != null) gearKeyInputs[index].SetActive(active);
    }

    public void ChangeSpeed(float newSpeed)
    {
        speedMultiplier = Mathf.Clamp(newSpeed, minSpeed, maxSpeed);
        SpeedText.text = speedMultiplier.ToString("F1");
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

    private void CheckNoteInLane(int laneIndex)
    {
        if (laneNotes[laneIndex].Count == 0) return;
        

        // Y 좌표 기준으로 노트를 가장 가까운 순서대로 정렬
        laneNotes[laneIndex].Sort((a, b) => a.transform.position.y.CompareTo(b.transform.position.y));

        Note closestNote = laneNotes[laneIndex][0];
        float currentTime = Time.time * 1000; // ms 단위

        float noteJudgeTime = closestNote.spawnTime + 2500f;
        float timeDifference = currentTime - noteJudgeTime;

        if (timeDifference <= 42 && timeDifference >= -42)
        {
            Debug.Log("Critical!");
            laneNotes[laneIndex].Remove(closestNote); // 노트 리스트에서 제거
            NotePool.Instance.ReturnNote(closestNote.gameObject); // 노트를 풀로 반환
            criticalNotes++;
            ScoreCal();
        }
        else if (timeDifference <= 120 && timeDifference >= -120)
        {
            Debug.Log("Hit!");
            laneNotes[laneIndex].Remove(closestNote); // 노트 리스트에서 제거
            NotePool.Instance.ReturnNote(closestNote.gameObject); // 노트를 풀로 반환
            hitNotes++;
            ScoreCal();
        }
        else if (timeDifference <= 180 && timeDifference >= -180)
        {
            Debug.Log("Block!");
            laneNotes[laneIndex].Remove(closestNote); // 노트 리스트에서 제거
            NotePool.Instance.ReturnNote(closestNote.gameObject); // 노트를 풀로 반환
            blockNotes++;
            ScoreCal();
        }
        else if(timeDifference <= 200 && timeDifference >= -200)
        {
            Debug.Log("Miss! Time Difference: " + timeDifference + "ms");
            laneNotes[laneIndex].Remove(closestNote); // 노트 리스트에서 제거
            NotePool.Instance.ReturnNote(closestNote.gameObject); // 노트를 풀로 반환
            missNotes++;
            ScoreCal();
        }
    }

    public void ScoreCal()
    {
        float Accuracy = 0f;
        float TotalHitNotes = (criticalNotes + hitNotes + blockNotes + missNotes);
        Accuracy = (criticalNotes*100 + hitNotes*66 + blockNotes*33)/(TotalHitNotes*100);
        Score = (TotalHitNotes/TotalNotes)*Accuracy*1000000;
        Accuracy = Mathf.Round(Accuracy*10000)/100;
        AccuracyText.text = Accuracy.ToString("F2");
        Score = Mathf.Round(Score);
        Debug.Log(Score);
        ScoreText.text = Score.ToString("N0");
    }

    private IEnumerator FadeOutAndInCoroutine()
    {
        Faded = true;
        yield return new WaitForSeconds(2f);

        // 1. 페이드 아웃
        yield return StartCoroutine(Fade(0f, 1f));
        Debug.Log("페이드아웃 완료!");

        // 2. 결과 화면 활성화
        endScoreScreen.SetActive(true);
        EndScreenControl();

        // 3. 페이드 인
        yield return StartCoroutine(Fade(1f, 0f));
        Debug.Log("페이드인 완료!");
    }

    void EndScreenControl()
    {
        resultNoteText[0].text = $"{criticalNotes}";
        resultNoteText[1].text = $"{hitNotes}";
        resultNoteText[2].text = $"{blockNotes}";
        resultNoteText[3].text = $"{missNotes}";
        resultNoteText[4].text = $"{Score}";

        if (Score >= 970000)
    {
        ScoreIcon.sprite = scoreIcons[0];
    }
    else if (Score >= 900000)
    {
        ScoreIcon.sprite = scoreIcons[1];
    }
    else if (Score >= 800000)
    {
        ScoreIcon.sprite = scoreIcons[2];
    }
    else if (Score > 0)
    {
        ScoreIcon.sprite = scoreIcons[3];
    }
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < 0.3f)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / 0.3f);
            fadeImage.color = color;
            yield return null;
        }

        color.a = endAlpha;
        fadeImage.color = color;
    }

    private void SetImageAlpha(float alpha)
    {
        Color color = fadeImage.color;
        color.a = alpha;
        fadeImage.color = color;
    }

    public void RegisterNote(Note note) => activeNotes.Add(note);
    public void UnregisterNote(Note note) => activeNotes.Remove(note);
    public void RegisterLongNote(LongNote longNote) => activeLongNotes.Add(longNote);
    public void UnregisterLongNote(LongNote longNote) => activeLongNotes.Remove(longNote);
}