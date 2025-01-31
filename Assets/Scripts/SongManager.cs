using UnityEngine;

public class SongManager : MonoBehaviour
{
    public static SongManager Instance;

    public string selectedSongName; // 선택된 곡 이름
    public string selectedSongPath; // 선택된 곡 파일 경로 등 추가 정보

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }
}