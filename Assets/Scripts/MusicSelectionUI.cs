using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.IO;
using DG.Tweening;
using FMODUnity;
using FMOD.Studio;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class SongSelectionUI : MonoBehaviour
{
    [Header("UI References")]
    public ScrollRect scrollRect; // ScrollRect 컴포넌트
    public RectTransform content; // ScrollView Content
    public GameObject songItemPrefab; // 곡 아이템 프리팹
    public Image albumCover; // 선택된 곡 앨범 커버
    public TMP_Text songTitle; // 곡 제목 텍스트
    public TMP_Text bpmText; // BPM 텍스트
    public TMP_Text leftPanelArtistName; // Left Panel 아티스트 이름 텍스트
    public Image backGround;
    public GameObject clearIconsParent; // 클리어 아이콘 부모 오브젝트
    public TMP_Text scoreText; // 점수를 표시할 TMP_Text
    private string currentMode;
    public Sprite[] scoreIcon;
    public Image ScoreIconUI;
    public RectTransform ScoreBox;
    public RectTransform SongInfo;
    public SelectedSongInfo selectedSong;

    [Header("Song Data")]
    public List<SongData> songs = new List<SongData>(); // 곡 데이터 리스트
    public List<DifficultyData> difficulties = new List<DifficultyData>(); // 난이도 데이터 리스트
    private int currentIndex = 0; // 현재 선택된 곡 인덱스

    [Header("FMOD Events")]
    [SerializeField] private EventReference navigationSound; // 화살표 이동 시 효과음 이벤트 참조

    [Header("Difficulty UI")]
    public GameObject nmlUI; // NM 난이도 UI
    public GameObject hrdUI; // HD 난이도 UI
    public GameObject insUI; // IS 난이도 UI
    public GameObject tmtUI; // TM 난이도 UI

    [Header("Scaling UI")]
    public RectTransform scalingUI; // 크기 조정 UI 오브젝트

    [Header("UI References")]
    public TMP_Text noteSpeedText; // 노트 속도 표시
    public TMP_Text modeText; // 모드 표시

    [Header("Clear Icons")]
    public GameObject starIconPrefab; // 별 아이콘 프리팹
    public GameObject blueIconPrefab; // 달 모양 아이콘 프리팹

    [System.Serializable]
    public class SelectedSongInfo
    {
        public string title;
        public string difficulty;
        public string mode;
        public string sheetFilePath;
        public string fmodEventPath;
    }

    private int currentDifficultyIndex = 0; // 현재 선택된 난이도 (0: nml, 1: hrd, 2: ins, 3: tmt)
    private const float ContentStartPosition = -125f; // Content 초기 위치
    private const float ContentMoveStep = 40f; // Content 이동 간격
    private const float MoveDuration = 0.3f; // 이동 애니메이션 지속 시간
    private int level = 0; // 현재 난이도 레벨
    private bool stop = false;
    // 연속 입력 처리용 변수
    private float keyHoldDelay = 0.13f; // 키 입력 간격 (초)
    private float keyHoldTimer = 0f; // 현재 타이머 값
    private EventInstance navigationSoundInstance; // 캐싱된 사운드 이벤트 인스턴스
    private string settingsFilePath; // 설정 파일 경로
    private EventInstance musicEventInstance; // FMOD 이벤트 인스턴스
    private float songChangeTimer;
    private string currentEventPath = ""; // 현재 재생 중인 음악의 경로

    void Start()
    {
        // JSON 파일 경로 설정
        string jsonPath = Application.streamingAssetsPath + "/MusicData.json";
        string difficultyJsonPath = Application.streamingAssetsPath + "/DifficultyData.json";
        settingsFilePath = Path.Combine(Application.persistentDataPath, "Settings.json");

        // 곡 데이터 및 난이도 데이터 로드
        LoadSongDataFromJSON(jsonPath);
        LoadDifficultyDataFromJSON(difficultyJsonPath);

        // 곡 목록을 UI에 채우기
        PopulateSongList();

        // Content 초기 위치 설정
        content.localPosition = new Vector3(content.localPosition.x, ContentStartPosition, content.localPosition.z);

        // 선택된 곡 UI 업데이트
        UpdateSelectedSong();

        // 초기 UI 업데이트
        if (File.Exists(settingsFilePath))
        {
            UpdateSettingsUI();
        }

        // 사운드 이벤트를 미리 로드하여 인스턴스 생성
        navigationSoundInstance = RuntimeManager.CreateInstance(navigationSound);
    }

    // JSON 파일에서 곡 데이터를 로드하는 함수
    void LoadSongDataFromJSON(string jsonPath)
    {
        if (!File.Exists(jsonPath))
        {
            Debug.LogError($"JSON 파일을 찾을 수 없습니다: {jsonPath}");
            return;
        }

        string json = File.ReadAllText(jsonPath);
        SongListWrapper parsedData = JsonUtility.FromJson<SongListWrapper>(json);

        if (parsedData == null || parsedData.songs == null || parsedData.songs.Count == 0)
        {
            Debug.LogError("곡 데이터 파싱 실패 또는 곡 데이터가 없습니다.");
            return;
        }

        songs = parsedData.songs;

        // 각 곡에 대해 앨범 커버 스프라이트 로드
        foreach (var song in songs)
        {
            song.albumCoverSprite = Resources.Load<Sprite>(song.albumCoverPath);
            if (song.albumCoverSprite == null)
            {
                Debug.LogError($"앨범 커버 로드 실패: {song.albumCoverPath}");
            }
        }
    }

    // JSON 파일에서 난이도 데이터를 로드하는 함수
    void LoadDifficultyDataFromJSON(string jsonPath)
    {
        if (!File.Exists(jsonPath))
        {
            Debug.LogError($"JSON 파일을 찾을 수 없습니다: {jsonPath}");
            return;
        }

        string json = File.ReadAllText(jsonPath);
        DifficultyListWrapper parsedData = JsonUtility.FromJson<DifficultyListWrapper>(json);

        if (parsedData == null || parsedData.difficulties == null || parsedData.difficulties.Count == 0)
        {
            Debug.LogError("난이도 데이터 파싱 실패 또는 난이도 데이터가 없습니다.");
            return;
        }

        difficulties = parsedData.difficulties;
    }

    // 곡 목록을 UI에 채우는 함수
    void PopulateSongList()
    {
        // 기존 곡 아이템들 제거
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        float itemHeight = songItemPrefab.GetComponent<RectTransform>().rect.height; // 아이템 높이
        float spacing = 5f; // 아이템 간 간격

        for (int i = 0; i < songs.Count; i++)
        {
            // 곡 아이템 프리팹 생성
            GameObject songItem = Instantiate(songItemPrefab, content);

            // 곡 제목과 아티스트 설정
            songItem.transform.Find("SongTitle").GetComponent<TMP_Text>().text = songs[i].title;
            songItem.transform.Find("ArtistName").GetComponent<TMP_Text>().text = songs[i].artist;

            // 앨범 커버 이미지 설정
            Image coverImage = songItem.transform.Find("AlbumCover").GetComponent<Image>();
            if (coverImage != null && songs[i].albumCoverSprite != null)
            {
                coverImage.sprite = songs[i].albumCoverSprite;
            }

            // 곡 아이템 위치 설정
            RectTransform songRect = songItem.GetComponent<RectTransform>();
            songRect.localPosition = new Vector3(0, -i * (itemHeight + spacing), 0);
            songRect.localScale = Vector3.one;
        }

        // Content 크기 조정
        RectTransform contentRect = content.GetComponent<RectTransform>();
        float totalHeight = songs.Count * (itemHeight + spacing) - spacing;
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, totalHeight);
    }

    // 선택된 곡을 강조하는 함수
    void HighlightSelectedSong()
    {
        for (int i = 0; i < content.childCount; i++)
        {
            Transform child = content.GetChild(i);
            Image highlight = child.Find("Highlight").GetComponent<Image>();
            RectTransform highlightRect = highlight.GetComponent<RectTransform>();

            if (i == currentIndex)
            {
                // 선택된 곡의 하이라이트 활성화
                highlightRect.DOSizeDelta(new Vector2(450f, highlightRect.sizeDelta.y), 0.3f); // 너비 증가
            }
            else
            {
                // 다른 곡의 하이라이트 비활성화
                highlightRect.DOSizeDelta(new Vector2(4f, highlightRect.sizeDelta.y), 0.3f); // 너비 축소
            }
        }
    }

    // 선택된 곡의 정보를 업데이트하는 함수
    void UpdateSelectedSong()
    {
        if (songs.Count == 0) return;

        // 현재 선택된 곡의 정보 표시
        SongData selectedSong = songs[currentIndex];
        songTitle.text = selectedSong.title;
        bpmText.text = "BPM:    " + selectedSong.bpm;
        albumCover.sprite = selectedSong.albumCoverSprite;
        leftPanelArtistName.text = "Artist: " + selectedSong.artist;
        backGround.sprite = selectedSong.albumCoverSprite;

        // 난이도 UI 업데이트
        currentDifficultyIndex = 0; // 난이도를 'nml'로 초기화
        UpdateDifficultyUI();

        // 선택된 곡을 화면 중앙으로 이동
        CenterSelectedSong();

        // 하이라이트 처리
        HighlightSelectedSong();

        // 점수 업데이트
        UpdateScoreText();

        // 타이머 초기화
        songChangeTimer = 0f;
    }

    // 난이도 UI 업데이트 함수
    void UpdateDifficultyUI()
    {
        GameObject[] difficultyUIs = { nmlUI, hrdUI, insUI, tmtUI };
        Vector2 defaultSize = new Vector2(1f, 17f); // 기본 크기
        Vector2 selectedSize = new Vector2(50f, 17f); // 선택된 크기

        for (int i = 0; i < difficultyUIs.Length; i++)
        {
            int level = i switch
            {
                0 => difficulties[currentIndex].nml?.level ?? 0,
                1 => difficulties[currentIndex].hrd?.level ?? 0,
                2 => difficulties[currentIndex].ins?.level ?? 0,
                3 => difficulties[currentIndex].tmt?.level ?? 0,
                _ => 0
            };

            // UI 활성화/비활성화 및 크기 조정
            GameObject ui = difficultyUIs[i];
            ui.SetActive(level > 0); // 난이도가 0보다 크면 UI 활성화

            Transform difBoxTransform = ui.transform.Find("DifBox");
            if (difBoxTransform != null)
            {
                RectTransform difBoxRect = difBoxTransform.GetComponent<RectTransform>();
                difBoxRect.DOSizeDelta(i == currentDifficultyIndex ? selectedSize : defaultSize, 0.3f)
                         .SetEase(Ease.OutCubic);
            }

            // 선택된 난이도 레벨 업데이트
            if (i == currentDifficultyIndex && level > 0)
            {
                this.level = level;
            }
        }

        // 현재 난이도에 따라 점수 업데이트
        UpdateScoreText();

        // 크기 조정 UI 업데이트
        if (scalingUI != null)
        {
            float additionalSize = level >= 16 ? (level - 15) : 0f;
            Vector2 targetSize = new Vector2((level * 10.5f) + additionalSize, scalingUI.sizeDelta.y);
            scalingUI.DOSizeDelta(targetSize, 0.3f).SetEase(Ease.OutCubic);
        }
    }

    public void UpdateScoreText()
    {
        if (difficulties.Count == 0 || currentIndex >= difficulties.Count)
        {
            Debug.LogError("No difficulty data available or index out of range.");
            scoreText.text = "0";
            return;
        }

        DifficultyData currentDifficulty = difficulties[currentIndex];
        int score = 0;

        switch (currentDifficultyIndex)
        {
            case 0:
                score = currentMode == "블청년" ? currentDifficulty.nml?.nb_score ?? 0 : currentDifficulty.nml?.pl_score ?? 0;
                break;
            case 1:
                score = currentMode == "블청년" ? currentDifficulty.hrd?.nb_score ?? 0 : currentDifficulty.hrd?.pl_score ?? 0;
                break;
            case 2:
                score = currentMode == "블청년" ? currentDifficulty.ins?.nb_score ?? 0 : currentDifficulty.ins?.pl_score ?? 0;
                break;
            case 3:
                score = currentMode == "블청년" ? currentDifficulty.tmt?.nb_score ?? 0 : currentDifficulty.tmt?.pl_score ?? 0;
                break;
            default:
                Debug.LogError("Invalid difficulty index.");
                break;
        }

        Debug.Log($"Updated Score: {score}");
        scoreText.text = $"{score}";
        UpdateClearIcons();
    }

    void UpdateClearIcons()
    {
        
    if (clearIconsParent == null || starIconPrefab == null || blueIconPrefab == null) return;

    // 기존 아이콘 제거
    foreach (Transform child in clearIconsParent.transform)
        Destroy(child.gameObject);

    // 현재 난이도 데이터 가져오기
    DifficultyLevel currentDifficulty = GetCurrentDifficulty();
    if (currentDifficulty == null) return;

    // 아이콘 생성
    CreateIcons(starIconPrefab, currentDifficulty.nb_status);

    if (currentMode == "할배")
        CreateIcons(blueIconPrefab, currentDifficulty.pl_status);
}

// 현재 난이도 데이터 가져오기
DifficultyLevel GetCurrentDifficulty()
{
    return currentDifficultyIndex switch
    {
        0 => difficulties[currentIndex].nml,
        1 => difficulties[currentIndex].hrd,
        2 => difficulties[currentIndex].ins,
        3 => difficulties[currentIndex].tmt,
        _ => null
    };
}

// 아이콘 생성 함수
void CreateIcons(GameObject prefab, int count)
{
    float spacing = 15f; // 아이콘 간 간격

    for (int i = 0; i < count; i++)
    {
        GameObject icon = Instantiate(prefab, clearIconsParent.transform);
        RectTransform iconRect = icon.GetComponent<RectTransform>();
        iconRect.anchoredPosition = new Vector2(i * spacing, 0);
        iconRect.localScale = Vector3.one;
    }
    UpdateScoreIcons();
}

void UpdateScoreIcons()
{
    if (clearIconsParent == null) return;

    // 현재 난이도 데이터 가져오기
    DifficultyLevel currentDifficulty = GetCurrentDifficulty();
    if (currentDifficulty == null) return;

    // 점수에 따른 아이콘 결정
    int score = GetScoreForCurrentDifficulty();
    ScoreIconUI.enabled = false;

    if (score >= 970000)
    {
        ScoreIconUI.sprite = scoreIcon[0];
        ScoreIconUI.enabled = true;
    }
    else if (score >= 900000)
    {
        ScoreIconUI.sprite = scoreIcon[1];
        ScoreIconUI.enabled = true;
    }
    else if (score >= 800000)
    {
        ScoreIconUI.sprite = scoreIcon[2];
        ScoreIconUI.enabled = true;
    }
    else if (score > 0)
    {
        ScoreIconUI.sprite = scoreIcon[3];
        ScoreIconUI.enabled = true;
    }

    // 점수가 0 이상이고 아이콘 프리팹이 있으면 생성
    if (ScoreIconUI != null)
    {
        RectTransform iconRect = ScoreIconUI.GetComponent<RectTransform>();
        iconRect.anchoredPosition = new Vector2(80, -140); // 아이콘 위치 초기화
        iconRect.localScale = Vector3.one;
    }
}

// 현재 난이도의 점수를 반환하는 함수
int GetScoreForCurrentDifficulty()
{
    switch (currentDifficultyIndex)
    {
        case 0: return currentMode == "블청년" ? GetCurrentDifficulty()?.nb_score ?? 0 : GetCurrentDifficulty()?.pl_score ?? 0;
        case 1: return currentMode == "블청년" ? GetCurrentDifficulty()?.nb_score ?? 0 : GetCurrentDifficulty()?.pl_score ?? 0;
        case 2: return currentMode == "블청년" ? GetCurrentDifficulty()?.nb_score ?? 0 : GetCurrentDifficulty()?.pl_score ?? 0;
        case 3: return currentMode == "블청년" ? GetCurrentDifficulty()?.nb_score ?? 0 : GetCurrentDifficulty()?.pl_score ?? 0;
        default: return 0;
    }
}

    // 선택된 곡을 화면 중앙으로 이동하는 함수
    void CenterSelectedSong()
    {
        if (songs.Count == 0) return;

        float newYPosition = ContentStartPosition + currentIndex * ContentMoveStep;
        content.DOLocalMoveY(newYPosition, MoveDuration).SetEase(Ease.OutCubic);
    }

    // JSON 파일 읽고 UI 업데이트
    void UpdateSettingsUI()
    {
        if (!File.Exists(settingsFilePath))
        {
            Debug.LogError($"설정 파일을 찾을 수 없습니다: {settingsFilePath}");
            return;
        }

        string json = File.ReadAllText(settingsFilePath);
        SettingsData settings = JsonUtility.FromJson<SettingsData>(json);

        if (settings != null)
        {
            // UI 업데이트
            noteSpeedText.text = $"{settings.NoteSpeed:F1}";
            modeText.text = settings.Mode;
            currentMode = settings.Mode;
        }
        else
        {
            Debug.LogError("설정 데이터를 파싱할 수 없습니다.");
        }
        UpdateScoreText();
    }

    void ScoreBoxUIScaling()
    {
        ScoreBox.DOKill();
        Vector2 ScoreBoxSize = new Vector2(230,60);
        ScoreBox.sizeDelta = new Vector2(1,60);
        ScoreBox.DOSizeDelta(ScoreBoxSize, 0.5f).SetEase(Ease.OutCubic);
    }

    void SongInfoUIScaling()
    {
        // 이전 애니메이션을 종료
        SongInfo.DOKill();

        // 초기 크기 설정
        SongInfo.sizeDelta = new Vector2(1, 110);

        // 새 애니메이션 시작
        Vector2 ScoreBoxSize = new Vector2(230, 110);
        SongInfo.DOSizeDelta(ScoreBoxSize, 1f).SetEase(Ease.OutCubic);
    }

    void PlaySelectedSong()
    {
        SongData selectedSong = songs[currentIndex];
        if (selectedSong.musicEventPath == currentEventPath)
        return;
        musicEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); // 이전 음악 정지
        // musicEventPath가 비어 있지 않다면 이벤트 로드 및 재생
        if (!string.IsNullOrEmpty(selectedSong.musicEventPath))
        {
            LoadAndPlayMusicEvent(selectedSong.musicEventPath);
        }
    }

    void LoadAndPlayMusicEvent(string eventPath)
    {
        // FMOD 이벤트 인스턴스를 생성하고, 해당 경로로 이벤트 로드
        musicEventInstance = FMODUnity.RuntimeManager.CreateInstance(eventPath);
        
        // 음악 이벤트 재생
        musicEventInstance.start();

        // 현재 재생 중인 음악 경로 업데이트
        currentEventPath = eventPath;
    }

    // 내비게이션 사운드 재생 함수
    void PlayNavigationSound()
    {
        // 사운드 이벤트 인스턴스가 유효한지 확인
        if (navigationSoundInstance.isValid())
        {
            navigationSoundInstance.start(); // 사운드 재생
        }

    }

    void OnDestroy()
    {
        // 씬 종료 시 캐시된 인스턴스를 해제합니다.
        if (navigationSoundInstance.isValid())
        {
            navigationSoundInstance.release();
        }
        // 게임 종료 시 음악 정지 및 리소스 해제
        if (musicEventInstance.isValid())
        {
            musicEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            musicEventInstance.release();
        }
    }

    // 현재 난이도 레벨을 반환하는 함수
    int GetCurrentDifficultyLevel()
    {
        switch (currentDifficultyIndex)
        {
            case 0: return difficulties[currentIndex].nml?.level ?? 0;
            case 1: return difficulties[currentIndex].hrd?.level ?? 0;
            case 2: return difficulties[currentIndex].ins?.level ?? 0;
            case 3: return difficulties[currentIndex].tmt?.level ?? 0;
            default: return 0;
        }
    }

    void SaveSongData()
    {
        PlayerPrefs.SetString("SongTitle", selectedSong.title);
        PlayerPrefs.SetString("SongDifficulty", selectedSong.difficulty);
        PlayerPrefs.SetString("SongMode", selectedSong.mode);
        PlayerPrefs.SetString("SheetFilePath", selectedSong.sheetFilePath);
        PlayerPrefs.SetString("FMODEventPath", selectedSong.fmodEventPath);
        PlayerPrefs.Save(); // 변경 사항 저장
    }

    void Update()
    {
        // 타이머 업데이트
    songChangeTimer += Time.deltaTime;

    // 타이머가 일정 시간(예: 5초) 이상이 되면 곡 재생
    if (songChangeTimer >= 0.5f)
    {
        PlaySelectedSong(); // 선택된 곡 재생
        songChangeTimer = 0f; // 타이머 초기화
    }

        // 타이머 업데이트
        keyHoldTimer -= Time.deltaTime;

        // 스페이스바를 눌러서 정지/재개 토글
        if (Input.GetKeyDown(KeyCode.Space))
        {
            stop = !stop; // stop 값을 반전시킴
            if (!stop) // stop이 끝났을 때
        {
            Invoke(nameof(UpdateSettingsUI), 0.1f); // 0.1초 후에 UpdateSettingsUI 실행
        }
            return; // 스페이스바 입력 후 아래 코드를 실행하지 않음
        }

        // stop이 true면 아래 입력 처리 코드 무시
        if (stop) return;

        // 방향키로 곡 이동
        if ((Input.GetKey(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.DownArrow)) && keyHoldTimer <= 0)
        {
            currentIndex = (currentIndex + 1) % songs.Count;
            UpdateSelectedSong();
            PlayNavigationSound();
            SongInfoUIScaling();
            keyHoldTimer = keyHoldDelay; // 타이머 초기화
        }
        else if ((Input.GetKey(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.UpArrow)) && keyHoldTimer <= 0)
        {
            currentIndex = (currentIndex - 1 + songs.Count) % songs.Count;
            UpdateSelectedSong();
            PlayNavigationSound();
            SongInfoUIScaling();
            keyHoldTimer = keyHoldDelay; // 타이머 초기화
        }

        // 방향키로 난이도 변경
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            do
            {
                currentDifficultyIndex = (currentDifficultyIndex - 1 + 4) % 4;
            }
            while (GetCurrentDifficultyLevel() == 0); // 유효한 난이도를 찾을 때까지 반복
            UpdateDifficultyUI();
            PlayNavigationSound();
            ScoreBoxUIScaling();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            do
            {
                currentDifficultyIndex = (currentDifficultyIndex + 1) % 4;
            }
            while (GetCurrentDifficultyLevel() == 0); // 유효한 난이도를 찾을 때까지 반복
            UpdateDifficultyUI();
            PlayNavigationSound();
            ScoreBoxUIScaling();
        }
        if (Input.GetKeyDown(KeyCode.Return)) // 엔터키 입력 감지
        {
            SaveSongData();
            SceneManager.LoadScene("GameScene"); // 게임 씬으로 이동
        }
    }
}

[System.Serializable]
public class SongData
{
    public string title; // 곡 제목
    public string artist; // 아티스트
    public int bpm; // BPM
    public string albumCoverPath; // 앨범 커버 경로
    [System.NonSerialized] public Sprite albumCoverSprite; // 런타임에 로드된 앨범 커버 스프라이트
    public string musicEventPath;
}

[System.Serializable]
public class DifficultyData
{
    public string title;
    public DifficultyLevel nml;
    public DifficultyLevel hrd;
    public DifficultyLevel ins;
    public DifficultyLevel tmt;
}

[System.Serializable]
public class DifficultyLevel
{
    public int level; // 난이도 레벨
    public int nb_score; // 노우브 점수
    public int nb_status; // 노우브 상태
    public int pl_score; // 플러스 점수
    public int pl_status; // 플러스 상태
}

[System.Serializable]
public class SongListWrapper
{
    public List<SongData> songs; // 곡 데이터 리스트
}

[System.Serializable]
public class DifficultyListWrapper
{
    public List<DifficultyData> difficulties; // 난이도 데이터 리스트
}

// 설정 데이터 구조체
[System.Serializable]
public class SettingsData
{
    public float NoteSpeed;
    public string Mode;
}

