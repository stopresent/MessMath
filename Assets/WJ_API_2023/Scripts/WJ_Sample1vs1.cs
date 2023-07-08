using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WjChallenge;
using TexDrawLib;

// public enum CurrentStatus { WAITING, DIAGNOSIS, LEARNING }
public class WJ_Sample1vs1 : MonoBehaviour
{
    //[SerializeField] WJ_Connector wj_conn;
    // [SerializeField] Managers.Game.CurrentStatus      currentStatus;
    // public Managers.Game.CurrentStatus CurrentStatus => currentStatus;

    [Header("Panels")]
    [SerializeField] GameObject panel_diag_chooseDiff;  //난이도 선택 패널
    [SerializeField] GameObject panel_question;         //문제 패널(진단,학습)

    [SerializeField] Text textDescription;              //문제 설명 텍스트
    [SerializeField] TEXDraw textEquation;              //문제 텍스트(※TextDraw로 변경 필요)
    //[SerializeField] Button[] btAnsr = new Button[4]; //정답 버튼들
    [HideInInspector]
    [SerializeField]
    public TEXDraw[] textAnsr;                          //정답 버튼들 텍스트(※TextDraw로 변경 필요)

    [Header("Status")]
    public int currentQuestionIndex;
    bool isSolvingQuestion;
    float questionSolveTime;
    public int ansrIndex;

    [Header("For Debug")]
    //[SerializeField] WJ_DisplayText wj_displayText;     //텍스트 표시용(필수X)
    [SerializeField] Button getLearningButton;          //문제 받아오기 버튼

    public UI_Fight1vs1Game uI_Fight1Vs1Game;

    private void Awake()
    {
        textAnsr = new TEXDraw[4];

        for (int i = 0; i < 4; ++i)
            textAnsr[i] = new TEXDraw();

        //for (int i = 0; i < btAnsr.length; ++i)
        //    textAnsr[i] = btAnsr[i].GetComponentInChildren<TEXDraw>();

        //wj_displayText.SetState("대기중", "", "", "");
    }

    private void OnEnable()
    {
        Setup();
    }
    private void Start()
    {
        uI_Fight1Vs1Game = GameObject.Find("UI_Fight1vs1Game").GetComponent<UI_Fight1vs1Game>();
    }

    private void Setup()
    {
        switch (Managers.Game.CurrentStatus)
        {
            case Define.CurrentStatus.WAITING:
                //panel_diag_chooseDiff.SetActive(true);
                break;
        }

        if (Managers.Connector != null)
        {
            Debug.Log("wj_connector!");
            if (Managers.Game.CurrentStatus == Define.CurrentStatus.WAITING)
                Managers.Connector.onGetDiagnosis.AddListener(() => GetDiagnosis());
            if (Managers.Game.CurrentStatus == Define.CurrentStatus.LEARNING)
                Managers.Connector.onGetLearning.AddListener(() => GetLearning(0));
        }
        else Debug.LogError("Cannot find Connector");

    }

    private void Update()
    {
        if (isSolvingQuestion) questionSolveTime += Time.deltaTime;
    }

    /// <summary>
    /// 진단평가 문제 받아오기
    /// </summary>
    private void GetDiagnosis()
    {
        switch (Managers.Connector.cDiagnotics.data.prgsCd)
        {
            case "W":
                MakeQuestion(Managers.Connector.cDiagnotics.data.textCn,
                            Managers.Connector.cDiagnotics.data.qstCn,
                            Managers.Connector.cDiagnotics.data.qstCransr,
                            Managers.Connector.cDiagnotics.data.qstWransr);
                //wj_displayText.SetState("진단평가 중", "", "", "");
                break;
            case "E":
                Debug.Log("진단평가 끝! 학습 단계로 넘어갑니다.");
                //wj_displayText.SetState("진단평가 완료", "", "", "");
                Managers.Game.CurrentStatus = Define.CurrentStatus.LEARNING;
                getLearningButton.interactable = true;
                break;
        }
    }

    /// <summary>
    /// 
    ///  n 번째 학습 문제 받아오기
    /// </summary>
    private void GetLearning(int _index)
    {
        if (_index == 0) currentQuestionIndex = 0;

        MakeQuestion(Managers.Connector.cLearnSet.data.qsts[_index].textCn,
                    Managers.Connector.cLearnSet.data.qsts[_index].qstCn,
                    Managers.Connector.cLearnSet.data.qsts[_index].qstCransr,
                    Managers.Connector.cLearnSet.data.qsts[_index].qstWransr);
    }

    /// <summary>
    /// 받아온 데이터를 가지고 문제를 표시
    /// </summary>
    private void MakeQuestion(string textCn, string qstCn, string qstCransr, string qstWransr)
    {
        //panel_diag_chooseDiff.SetActive(false);
        //panel_question.SetActive(true);

        string correctAnswer;
        string[] wrongAnswers;

        //Debug.Log(textCn);
        //Debug.Log(qstCn);

        //textDescription.text = textCn;
        //textEquation.text = qstCn;

        correctAnswer = qstCransr;
        wrongAnswers = qstWransr.Split(',');
        int ansrCount = Mathf.Clamp(wrongAnswers.Length, 0, 3) + 1;

        //for (int i = 0; i < 4; i++)
        //{
        //    if (i < ansrCount)
        //        btAnsr[i].gameObject.SetActive(true);
        //    else
        //        btAnsr[i].gameObject.SetActive(false);
        //}

        ansrIndex = Random.Range(0, ansrCount);

        //for (int i = 0, q = 0; i < ansrCount; ++i, ++q)
        //{
        //    if (i == ansrIndex)
        //    {
        //        textAnsr[i].text = correctAnswer;
        //        --q;
        //    }
        //    else
        //        textAnsr[i].text = wrongAnswers[q];
        //}
        isSolvingQuestion = true;
    }

    /// <summary>
    /// 답을 고르고 맞았는 지 체크
    /// </summary>
    public void SelectAnswer(string ansr)
    {
        bool isCorrect;
        string ansrCwYn = "N";

        switch (Managers.Game.CurrentStatus)
        {
            case Define.CurrentStatus.DIAGNOSIS:
                isCorrect = ansr.CompareTo(Managers.Connector.cDiagnotics.data.qstCransr) == 0 ? true : false;
                ansrCwYn = isCorrect ? "Y" : "N";

                isSolvingQuestion = false;

                Managers.Connector.Diagnosis_SelectAnswer(ansr, ansrCwYn, (int)(questionSolveTime * 1000));

                //wj_displayText.SetState("진단평가 중", textAnsr[_idx].text, ansrCwYn, questionSolveTime + " 초");

                //panel_question.SetActive(false);
                questionSolveTime = 0;
                break;

            case Define.CurrentStatus.LEARNING:
                isCorrect = ansr.CompareTo(Managers.Connector.cLearnSet.data.qsts[currentQuestionIndex].qstCransr) == 0 ? true : false;
                ansrCwYn = isCorrect ? "Y" : "N";

                currentQuestionIndex++;
                uI_Fight1Vs1Game.SolvedQstNum++;

                // 테스트로 추가한 부분
                if (isCorrect)
                {
                    Debug.Log("정답");
                    Managers.Game.IsCorrect = true;

                    float damage = 100f / uI_Fight1Vs1Game.QstMaxNum;
                    uI_Fight1Vs1Game.damageToWitch(damage);

                    // TODO? CoinAnim Add?
                }
                else
                {
                    Debug.Log("오답");
                    uI_Fight1Vs1Game.damageToPlayer(1);
                    Managers.Game.IsCorrect = false;

                }

                isSolvingQuestion = false;

                Managers.Connector.Learning_SelectAnswer(currentQuestionIndex, ansr, ansrCwYn, (int)(questionSolveTime * 1000));

                //wj_displayText.SetState("문제풀이 중", textAnsr[_idx].text, ansrCwYn, questionSolveTime + " 초");

                if (currentQuestionIndex >= 8)
                {
                    //panel_question.SetActive(false);
                    //wj_displayText.SetState("문제풀이 완료", "", "", "");
                }
                else GetLearning(currentQuestionIndex);

                questionSolveTime = 0;
                break;
        }

    }

    ///// <summary>
    ///// 답을 고르고 맞았는 지 체크
    ///// </summary>
    //public void SelectAnswer(int _idx)
    //{
    //    bool isCorrect;
    //    string ansrCwYn = "N";

    //    switch (Managers.Game.CurrentStatus)
    //    {
    //        case Define.CurrentStatus.DIAGNOSIS:
    //            isCorrect = textAnsr[_idx].text.CompareTo(Managers.Connector.cDiagnotics.data.qstCransr) == 0 ? true : false;
    //            ansrCwYn = isCorrect ? "Y" : "N";

    //            isSolvingQuestion = false;

    //            Managers.Connector.Diagnosis_SelectAnswer(textAnsr[_idx].text, ansrCwYn, (int)(questionSolveTime * 1000));

    //            //wj_displayText.SetState("진단평가 중", textAnsr[_idx].text, ansrCwYn, questionSolveTime + " 초");

    //            panel_question.SetActive(false);
    //            questionSolveTime = 0;
    //            break;

    //        case Define.CurrentStatus.LEARNING:
    //            isCorrect = textAnsr[_idx].text.CompareTo(Managers.Connector.cLearnSet.data.qsts[currentQuestionIndex].qstCransr) == 0 ? true : false;
    //            ansrCwYn = isCorrect ? "Y" : "N";

    //            // 테스트로 추가한 부분
    //            if (isCorrect)
    //            {
    //                Debug.Log("정답");
    //                Managers.Game.Coin++;
    //                Managers.Game.IsCorrect = true;
    //                // TODO? CoinAnim Add?
    //            }
    //            else
    //            {
    //                Debug.Log("오답");
    //                Managers.Game.IsCorrect = false;

    //            }

    //            isSolvingQuestion = false;
    //            currentQuestionIndex++;

    //            Managers.Connector.Learning_SelectAnswer(currentQuestionIndex, textAnsr[_idx].text, ansrCwYn, (int)(questionSolveTime * 1000));

    //            //wj_displayText.SetState("문제풀이 중", textAnsr[_idx].text, ansrCwYn, questionSolveTime + " 초");

    //            if (currentQuestionIndex >= 8)
    //            {
    //                panel_question.SetActive(false);
    //                //wj_displayText.SetState("문제풀이 완료", "", "", "");
    //            }
    //            else GetLearning(currentQuestionIndex);

    //            questionSolveTime = 0;
    //            break;
    //    }

    //}

    //public void DisplayCurrentState(string state, string myAnswer, string isCorrect, string svTime)
    //{
    //    if (wj_displayText == null) return;

    //    wj_displayText.SetState(state, myAnswer, isCorrect, svTime);
    //}

    #region Unity ButtonEvent
    public void ButtonEvent_ChooseDifficulty(int a)
    {
        Managers.Game.CurrentStatus = Define.CurrentStatus.DIAGNOSIS;
        Managers.Connector.FirstRun_Diagnosis(a);
    }
    public void ButtonEvent_GetLearning()
    {
        Managers.Connector.Learning_GetQuestion();
        //wj_displayText.SetState("문제풀이 중", "-", "-", "-");
    }
    #endregion
}
