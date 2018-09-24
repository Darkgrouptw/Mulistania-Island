using UnityEngine;
using UnityEngine.UI;
using System.Collections;

////////////////////////////////////////////////////////////
// 這個主要使用來管理 Boy 動作的 controller
////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////
// 狀態 State ( ResetToSet 用)
// 0 => Idle
// 1 => Walk
// 2 => UseCard
////////////////////////////////////////////////////////////
enum ResetToSet_State
{
    IDLE_STATE = 0,
    WALK_STATE,
    USEWARD_STATE
}


////////////////////////////////////////////////////////////
// 動畫裡，會有一些動作僵持的狀態，需要去記錄這些狀態
////////////////////////////////////////////////////////////
enum IsAnimFixedType
{
    USECARD_ANIM_STATE = 0,
    UNFIXED = -1,
}

public class BoyController : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    // 方向的狀態
    // 0 => 沒動 (維持上一個狀態)    => 假設這個沒動，就要表現 lastState 的 Idle
    // 1 => 前
    // 2 => 後
    // 3 => 左
    // 4 => 左前
    // 5 => 左後
    // 6 => 右
    // 7 => 右前
    // 8 => 右後
    //
    // 5 2 8
    // 3 0 6
    // 4 1 7
    ////////////////////////////////////////////////////////////
    //  接人物 Parent 的 GameObject
    public GameObject Idle;
    public GameObject Walk;
    public GameObject UseCard;

    public float MoveSpeed = 3;

    // 將人物一個一個部分，切開來 (是上面的 Child)
    private GameObject[] BoyIdle = new GameObject[2];                       // Idle 上、下
    private GameObject[] BoyWalk = new GameObject[8];                       // 走路

    // 細向控制
    private int lastState = 0;                                              // 前一次的狀態，如果一樣，就繼續播動畫，如果不一樣就繪動畫然後重播
    private float ControlGap = 0.01f;
    private IsAnimFixedType IsAnimFixed = IsAnimFixedType.UNFIXED;

    private void Start()
    {
        for (int i = 0; i < BoyIdle.Length; i++)
            BoyIdle[i] = Idle.GetComponentsInChildren<Transform>(true)[i * 5 + 1].gameObject;       // 每4個一循環

        // 8 個方位加進來
        for (int i = 0; i < 8; i++)
        {
            // 每一開始 4 個一循環 (上下)
            // 接下來都是 10 個一循環
            int index = (i >= 2) ? (5 * 2 + (i - 2) * 11 + 1) : (i * 5 + 1);
            BoyWalk[i] = Walk.GetComponentsInChildren<Transform>(true)[index].gameObject;
        }
    }
    private void Update()
    {
        #region 這邊是用來判斷狀態回來時，表現的問題
        if (IsAnimFixed != IsAnimFixedType.UNFIXED)
        {
            switch(IsAnimFixed)
            {
                case IsAnimFixedType.USECARD_ANIM_STATE:
                    Animator anim = UseCard.GetComponent<Animator>();
                    if(anim.GetCurrentAnimatorStateInfo(0).IsName("AnimStop"))
                        IsAnimFixed = IsAnimFixedType.UNFIXED;
                    break;
            }
        }
        #endregion
        #region 要先判斷有沒有僵持的動作
        // 先判斷是否是僵持狀態
        if (IsAnimFixed != IsAnimFixedType.UNFIXED)
            return;

        // 使用卡片
        if (Input.GetKeyDown(KeyCode.Z))
        {
            ResetToUseCardState();
            return;
        }
        #endregion
        #region 要先判斷有沒有僵持的動作
        // 先判斷是否是僵持狀態
        if (IsAnimFixed != IsAnimFixedType.UNFIXED)
            return;
        #endregion
        #region 接收上下左右的操控
        int TempState = 0;
        float GetInputH = Input.GetAxis("Horizontal");      // 水平（左右）
        float GetInputV = Input.GetAxis("Vertical");        // 垂直 (上下)

        // 設定狀態機
        // 左 +1 右 +2
        if (GetInputV < -ControlGap)
            TempState = 1;
        else if (GetInputV > ControlGap)
            TempState = 2;

        // 下 +3 上 +6
        if (GetInputH < -ControlGap)
            TempState += 3;
        else if (GetInputH > ControlGap)
            TempState += 6;

        // 總結
        //Debug.Log(TempState);
        if (TempState != 0)
        {
            ResetToWalkState(TempState);
            BoyMove(lastState);
            lastState = TempState;
        }
        else if (TempState == 0)
            ResetToIdleState(lastState);
        #endregion
    }

    #region 重製狀態
    // 用來重製動畫 & GameObject 的
    private void ResetGameObject(ResetToSet_State state, int orgState)
    {
        switch (state)
        {
            #region 等待
            case ResetToSet_State.IDLE_STATE:
                {
                    Idle.SetActive(true);
                    Walk.SetActive(false);
                    UseCard.SetActive(false);
                    for (int i = 0; i < BoyIdle.Length; i++)
                        if (orgState != i)
                            BoyIdle[i].SetActive(false);
                        else
                            BoyIdle[i].SetActive(true);
                }
                break;
            #endregion
            #region 走路
            case ResetToSet_State.WALK_STATE:
                {
                    Idle.SetActive(false);
                    Walk.SetActive(true);
                    UseCard.SetActive(false);
                    for (int i = 0; i < BoyWalk.Length; i++)
                        if (orgState != i)
                            BoyWalk[i].SetActive(false);
                        else
                            BoyWalk[i].SetActive(true);
                }
                break;
            #endregion
            #region 使用卡片
            case ResetToSet_State.USEWARD_STATE:
                {
                    Idle.SetActive(false);
                    Walk.SetActive(false);
                    UseCard.SetActive(true);
                }
                break;
            #endregion
        }
    }


    //  人走路的移動 Function
    private void BoyMove(int lastState)
    {
        switch (lastState)
        {
            case 1:
                this.gameObject.transform.localPosition += new Vector3(0, -MoveSpeed, 0) * Time.deltaTime;
                break;
            case 2:
                this.gameObject.transform.localPosition += new Vector3(0, MoveSpeed, 0) * Time.deltaTime;
                break;
            case 3:
                this.gameObject.transform.localPosition += new Vector3(-MoveSpeed, 0, 0) * Time.deltaTime;
                break;
            case 4:
                this.gameObject.transform.localPosition += new Vector3(-MoveSpeed * Mathf.Cos(45.0f / 180 * Mathf.PI), -MoveSpeed * Mathf.Sin(45.0f / 180 * Mathf.PI), 0) * Time.deltaTime;
                break;
            case 5:
                this.gameObject.transform.localPosition += new Vector3(-MoveSpeed * Mathf.Cos(45.0f / 180 * Mathf.PI), MoveSpeed * Mathf.Sin(45.0f / 180 * Mathf.PI), 0) * Time.deltaTime;
                break;
            case 6:
                this.gameObject.transform.localPosition += new Vector3(MoveSpeed, 0, 0) * Time.deltaTime;
                break;
            case 7:
                this.gameObject.transform.localPosition += new Vector3(MoveSpeed * Mathf.Cos(45.0f / 180 * Mathf.PI), -MoveSpeed * Mathf.Sin(45.0f / 180 * Mathf.PI), 0) * Time.deltaTime;
                break;
            case 8:
                this.gameObject.transform.localPosition += new Vector3(MoveSpeed * Mathf.Cos(45.0f / 180 * Mathf.PI), MoveSpeed * Mathf.Sin(45.0f / 180 * Mathf.PI), 0) * Time.deltaTime;
                break;
        }
    }

    // Idle
    private void ResetToIdleState(int state)
    {
        if (state == 5 || state == 2 || state == 8)
            state = 1;
        else
            state = 0;
        ResetGameObject(ResetToSet_State.IDLE_STATE, state);
    }
    // Walk
    private void ResetToWalkState(int state)
    {
        ResetGameObject(ResetToSet_State.WALK_STATE, state - 1);
    }
    // UseCard
    private void ResetToUseCardState()
    {
        ResetGameObject(ResetToSet_State.USEWARD_STATE, 0);
        IsAnimFixed = IsAnimFixedType.USECARD_ANIM_STATE;
    }
    #endregion
}
