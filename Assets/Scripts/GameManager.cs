using UnityEngine;

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
        // Z키가 눌렸을 때
        if (Input.GetKeyDown(KeyCode.Z))
        {
            // 스프라이트를 활성화
            if (keyInput1 != null)
            {
                keyInput1.SetActive(true);
            }
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            // 스프라이트를 활성화
            if (keyInput2 != null)
            {
                keyInput2.SetActive(true);
            }
        }
        if (Input.GetKeyDown(KeyCode.Period))
        {
            // 스프라이트를 활성화
            if (keyInput3 != null)
            {
                keyInput3.SetActive(true);
            }
        }
        if (Input.GetKeyDown(KeyCode.Slash))
        {
            // 스프라이트를 활성화
            if (keyInput4 != null)
            {
                keyInput4.SetActive(true);
            }
        }

        // Z키가 떼어졌을 때 (원하는 경우 스프라이트를 다시 비활성화)
        if (Input.GetKeyUp(KeyCode.Z))
        {
            if (keyInput1 != null)
            {
                keyInput1.SetActive(false); // 스프라이트 비활성화
            }
        }
        if (Input.GetKeyUp(KeyCode.X))
        {
            if (keyInput2 != null)
            {
                keyInput2.SetActive(false); // 스프라이트 비활성화
            }
        }
        if (Input.GetKeyUp(KeyCode.Period))
        {
            if (keyInput3 != null)
            {
                keyInput3.SetActive(false); // 스프라이트 비활성화
            }
        }
        if (Input.GetKeyUp(KeyCode.Slash))
        {
            if (keyInput4 != null)
            {
                keyInput4.SetActive(false); // 스프라이트 비활성화
            }
        }
        // Z키가 눌렸을 때
        if (Input.GetKeyDown(KeyCode.Z))
        {
            // 스프라이트를 활성화
            if (gearKeyInput1 != null)
            {
                gearKeyInput1.SetActive(true);
            }
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            // 스프라이트를 활성화
            if (gearKeyInput2 != null)
            {
                gearKeyInput2.SetActive(true);
            }
        }
        if (Input.GetKeyDown(KeyCode.Period))
        {
            // 스프라이트를 활성화
            if (gearKeyInput3 != null)
            {
                gearKeyInput3.SetActive(true);
            }
        }
        if (Input.GetKeyDown(KeyCode.Slash))
        {
            // 스프라이트를 활성화
            if (gearKeyInput4 != null)
            {
                gearKeyInput4.SetActive(true);
            }
        }

        // Z키가 떼어졌을 때 (원하는 경우 스프라이트를 다시 비활성화)
        if (Input.GetKeyUp(KeyCode.Z))
        {
            if (gearKeyInput1 != null)
            {
                gearKeyInput1.SetActive(false); // 스프라이트 비활성화
            }
        }
        if (Input.GetKeyUp(KeyCode.X))
        {
            if (gearKeyInput2 != null)
            {
                gearKeyInput2.SetActive(false); // 스프라이트 비활성화
            }
        }
        if (Input.GetKeyUp(KeyCode.Period))
        {
            if (gearKeyInput3 != null)
            {
                gearKeyInput3.SetActive(false); // 스프라이트 비활성화
            }
        }
        if (Input.GetKeyUp(KeyCode.Slash))
        {
            if (gearKeyInput4 != null)
            {
                gearKeyInput4.SetActive(false); // 스프라이트 비활성화
            }
        }
    }
}
