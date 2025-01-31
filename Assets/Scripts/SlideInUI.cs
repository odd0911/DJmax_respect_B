using UnityEngine;
using System;
using DG.Tweening;
using TMPro;
using System.IO; // 파일 입출력
using System.Collections.Generic; // Dictionary 사용
using Newtonsoft.Json; // JSON 직렬화/역직렬화

public class SlideInUI : MonoBehaviour
{
    public RectTransform uiElement; // 이동할 UI 요소
    private Vector2 offScreenPosition = new Vector2(-325, 0); // 화면 밖 위치
    private Vector2 onScreenPosition = new Vector2(0, 0); // 화면 안 위치
    public float slideDuration = 0.5f; // 이동 시간
    public GameObject[] menuItems; // 탐색할 UI 항목들 (각 항목은 Highlight 이미지를 자식으로 포함)
    public float growDuration = 0.3f; // Highlight 늘어나는 시간
    private float maxHighlightWidth = 270f; // Highlight의 최대 너비
    private int currentIndex = 0; // 현재 선택된 항목 인덱스
    private RectTransform currentHighlight; // 현재 Highlight RectTransform
    private string saveFilePath; // 저장 파일 경로

    private bool isOnScreen = false; // 현재 UI 상태

    // 조정할 변수들
    private float noteSpeed = 5.0f;
    private string mode = "블청년";
    private float gearTransparency = 100f;
    private bool rate = true;
    private bool flsl = true;
    private string gearPosition = "LEFT";

    private void Start()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "Settings.json");

        // 시작 위치를 화면 밖으로 설정
        if (uiElement != null)
        {
            uiElement.anchoredPosition = offScreenPosition;
        }
        LoadSettings(); // 저장된 설정 불러오기
        UpdateSelection(); // 초기 선택 상태 업데이트
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleUI();
        }
        if (isOnScreen == true)
        {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Navigate(-1); // 위로 이동
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Navigate(1); // 아래로 이동
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            AdjustValue(-1); // 값 감소
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            AdjustValue(1); // 값 증가
        }
        }
    }

    private void Navigate(int direction)
    {
        // 현재 항목의 Highlight 줄이기
        ShrinkHighlight(currentIndex);

        // 새로운 인덱스 계산
        currentIndex = Mathf.Clamp(currentIndex + direction, 0, menuItems.Length - 1);

        // 새로운 항목의 Highlight 늘리기
        GrowHighlight(currentIndex);
    }

    private void AdjustValue(int direction)
    {
        // 현재 선택된 UI 이름 가져오기
        string currentName = menuItems[currentIndex].name;

        // 이름에 따라 변수 조정
        switch (currentName)
        {
            case "NoteSpeed":
                noteSpeed = Mathf.Clamp(noteSpeed + direction * 0.1f, 1.0f, 10.0f);
                UpdateChildText(currentIndex, noteSpeed.ToString("F1"));
                break;

            case "Mode":
                string[] difficulties = { "블청년", "할배" };
                int currentDifficultyIndex = System.Array.IndexOf(difficulties, mode);
                currentDifficultyIndex = (currentDifficultyIndex + direction + difficulties.Length) % difficulties.Length;
                mode = difficulties[currentDifficultyIndex];
                UpdateChildText(currentIndex, mode);
                break;

            case "Gear":
                gearTransparency = Mathf.Clamp(gearTransparency + direction * 10f, 0f, 100f);
                UpdateChildText(currentIndex, $"{gearTransparency}%");
                break;

            case "GearLocation":
                string[] positions = { "LEFT", "CENTER", "RIGHT" };
                int currentPositionIndex = System.Array.IndexOf(positions, gearPosition);
                currentPositionIndex = (currentPositionIndex + direction + positions.Length) % positions.Length;
                gearPosition = positions[currentPositionIndex];
                UpdateChildText(currentIndex, gearPosition);
                break;

            case "Rate":
                rate = !rate; // 토글
                UpdateChildText(currentIndex, rate ? "ON" : "OFF");
                break;

            case "FastSlow":
                flsl = !flsl; // 토글
                UpdateChildText(currentIndex, flsl ? "ON" : "OFF");
                break;

            default:
                Debug.LogWarning($"Unknown menu item: {currentName}");
                break;
        }
    }

    private void UpdateChildText(int index, string value)
    {
        // 선택된 항목의 자식 텍스트(TMP)를 업데이트
        TextMeshProUGUI valueText = menuItems[index].transform.Find("ValueText").GetComponent<TextMeshProUGUI>();
        if (valueText != null)
        {
            valueText.text = value;
        }
    }

    private void UpdateSelection()
    {
        for (int i = 0; i < menuItems.Length; i++)
        {
            if (i == currentIndex)
            {
                GrowHighlight(i); // 현재 선택된 항목 Highlight 활성화
            }
            else
            {
                ShrinkHighlight(i); // 나머지 항목 Highlight 비활성화
            }
        }
    }

    private void GrowHighlight(int index)
    {
        Transform highlight = menuItems[index].transform.Find("Highlight");
        Transform arrowBox = menuItems[index].transform.Find("ArrowBox");
        if (highlight != null)
        {
            currentHighlight = highlight.GetComponent<RectTransform>();
            currentHighlight.DOSizeDelta(new Vector2(maxHighlightWidth, currentHighlight.sizeDelta.y), growDuration)
                            .SetEase(Ease.OutQuad);
        }
        if (arrowBox != null)
        {
            arrowBox.gameObject.SetActive(true); // ArrowBox 활성화
        }
    }

    private void ShrinkHighlight(int index)
    {
        Transform highlight = menuItems[index].transform.Find("Highlight");
        Transform arrowBox = menuItems[index].transform.Find("ArrowBox");
        if (highlight != null)
        {
            RectTransform highlightRect = highlight.GetComponent<RectTransform>();
            highlightRect.DOSizeDelta(new Vector2(0, highlightRect.sizeDelta.y), growDuration)
                         .SetEase(Ease.OutQuad);
        }
        if (arrowBox != null)
        {
            arrowBox.gameObject.SetActive(false); // ArrowBox 비활성화
        }
    }

    private void ToggleUI()
    {
        if (uiElement == null) return;

        if (isOnScreen)
        {
            SaveSettings(); // 설정 저장
            // 화면 밖으로 슬라이드
            uiElement.DOAnchorPos(offScreenPosition, slideDuration).SetEase(Ease.InOutQuad);
        }
        else
        {
            // 화면 안으로 슬라이드
            uiElement.DOAnchorPos(onScreenPosition, slideDuration).SetEase(Ease.InOutQuad);
        }

        isOnScreen = !isOnScreen; // 상태 전환
    }

    private void SaveSettings()
    {
        var settings = new Dictionary<string, object>
        {
            { "NoteSpeed", noteSpeed },
            { "Mode", mode },
            { "Gear", gearTransparency },
            { "GearLocation", gearPosition },
            { "Rate", rate },
            { "FastSlow", flsl }
        };

        string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
        File.WriteAllText(saveFilePath, json);
        Debug.Log("Settings saved to " + saveFilePath);
    }

    private void LoadSettings()
    {
        if (!File.Exists(saveFilePath)) return;

        string json = File.ReadAllText(saveFilePath);
        var settings = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

        noteSpeed = Convert.ToSingle(settings["NoteSpeed"]);
        mode = settings["Mode"].ToString();
        gearTransparency = Convert.ToSingle(settings["Gear"]);
        gearPosition = settings["GearLocation"].ToString();
        rate = Convert.ToBoolean(settings["Rate"]);
        flsl = Convert.ToBoolean(settings["FastSlow"]);

        // UI 갱신
        for (int i = 0; i < menuItems.Length; i++)
        {
            string currentName = menuItems[i].name;

            switch (currentName)
            {
                case "NoteSpeed":
                    UpdateChildText(i, noteSpeed.ToString("F1"));
                    break;
                case "Mode":
                    UpdateChildText(i, mode);
                    break;
                case "Gear":
                    UpdateChildText(i, $"{gearTransparency}%");
                    break;
                case "GearLocation":
                    UpdateChildText(i, gearPosition);
                    break;
                case "Rate":
                    UpdateChildText(i, rate ? "ON" : "OFF");
                    break;
                case "FastSlow":
                    UpdateChildText(i, flsl ? "ON" : "OFF");
                    break;
            }
        }
    }
}