﻿using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 모든 가호를 관리하는 Manager
/// </summary>
public class GraceManager
{
    PlayerControllerCCF player;
    Vector3 playerPos;
    WitchController witch;

    // 각 가호들의 실행 중 유무 bool
    public bool gaussOn = false;
    public bool pythagorasOn = false;
    public bool newtonOn = false;

    // 플레이어가 화살과 충돌 후 연산 여부 bool
    // playerCollisionOff가 true일 때, 충돌 후 아무런 연산도 하지 않는다
    public bool playerCollisionOff = false;

    /// <summary>
    /// 모든 가호는 호출될 때 Setup()을 맨처음에 호출해야 한다.
    /// </summary>
    public void Setup()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControllerCCF>();
        playerPos = player.transform.position;
        witch = GameObject.FindGameObjectWithTag("Witch").GetComponent<WitchController>();
    }

    /// <summary>
    /// 가호 호출은 모두 CallGrace()를 통해서 한다. Managers.Grace.CallGrace("GraceOfGauss")
    /// </summary>
    /// <param name="graceName"> 가호의 이름을 입력 </param>
    public void CallGrace(string graceName)
    {
        Setup();
        switch (graceName)
        {
            case "GraceOfGauss":
                GraceOfGauss();
                break;
            case "GraceOfPythagoras":
                GraceOfPythagoras();
                break;
            case "GraceOfNewton":
                GraceOfNewton();
                break;
        }
    }

    /// <summary>
    /// 가우스의 가호 : 세레스 공전, 이 다음 공격 데미지를 두배로.
    /// </summary>
    public void GraceOfGauss()
    {
        if (gaussOn) return;
        gaussOn = true;
        float radius = 100f;
        float angle = 0f;
        float speed = 2f;
        float prevWitchHp = witch.Hp;

        // 공격 데미지 2배
        Managers.Game.Damage *= 2;

        GameObject CeresBack = Managers.Resource.Instantiate("Grace/CeresBack",player.transform.parent);
        GameObject Ceres = Managers.Resource.Instantiate("Grace/Ceres", player.transform.parent);
        Ceres.transform.position = new Vector3(playerPos.x - radius, playerPos.y, 0);
        CeresBack.transform.SetSiblingIndex(1);
        Ceres.transform.SetSiblingIndex(2);

        // 세레스 돌리기
        CoroutineHandler.StartCoroutine(RotateCeres(Ceres, CeresBack.transform, radius, angle, speed, prevWitchHp));
    }

    /// <summary>
    /// 세레스를 계속 돌린다. 마녀가 데미지를 받으면 멈춤.
    /// </summary>
    /// <param name="Ceres">세레스 오브젝트</param>
    /// <param name="center">중심으로 잡을 오브젝트</param>
    /// <param name="radius">반지름</param>
    /// <param name="angle">시작각도</param>
    /// <param name="speed">속도</param>
    /// <param name="prevWitchHp">마녀의 이전 체력</param>
    IEnumerator RotateCeres(GameObject Ceres, Transform center, float radius, float angle, float speed, float prevWitchHp)
    {
        int centerIndex = center.transform.GetSiblingIndex();
        int underPlayer = centerIndex + 1;
        int onPlayer = centerIndex + 2;
        while (player != null)
        {
            angle -= speed * Time.deltaTime;
            center.position = player.transform.position;
            Ceres.transform.position = center.position + new Vector3(Mathf.Cos(angle)*2, Mathf.Sin(angle), 0) * radius;
            
            // 플레이어 앞뒤로 돌리기
            if (Mathf.Sin(angle) > 0) Ceres.transform.SetSiblingIndex(underPlayer);
            else Ceres.transform.SetSiblingIndex(onPlayer);

            if (prevWitchHp != witch.Hp)
            {
                Debug.Log("witch hp : " + witch.Hp);
                Managers.Game.Damage = Managers.Game.Damage / 2;
                UnityEngine.Object.Destroy(center.gameObject);
                UnityEngine.Object.Destroy(Ceres.gameObject);
                yield break;
            }

            yield return null;
        }
        gaussOn = false;
    }

    /// <summary>
    /// 피타고라스의 가호 : 삼각형이 주위를 감싸며, 모드에 따라 다른 효과
    /// 스토리 모드에서는 1회 화살 피격 무시,
    /// 수학자 1vs1 모드에서는 5초간 화살 피격 무시
    /// 피격 무시 처리는 PlayerControllerCCF에서
    /// </summary>
    public void GraceOfPythagoras()
    {
        if (pythagorasOn) return;
        pythagorasOn = true;

        float Time = 5.0f;      // 지속시간은 5초

        GameObject PtriangleBack = Managers.Resource.Instantiate("Grace/PythagorasTriangleBack", player.transform.parent);
        GameObject Ptriangle = Managers.Resource.Instantiate("Grace/PythagorasTriangle", player.transform.parent);
        PtriangleBack.transform.position = playerPos;
        Ptriangle.transform.position = playerPos;
        PtriangleBack.GetComponent<Image>().CrossFadeAlpha(0f, Time, false);
        Ptriangle.GetComponent<Image>().CrossFadeAlpha(0f, Time, false);


        CoroutineHandler.StartCoroutine(EndPythagoras(Ptriangle, PtriangleBack, Time));

        // 수학자 1vs1 대련 Scene인 경우 
        if (Managers.Scene.CurrentSceneType == Define.Scene.Fight1vs1GameScene)
            playerCollisionOff = true;
        // 스토리 모드인 경우
        if(Managers.Scene.CurrentSceneType == Define.Scene.StoryGameScene)
        {
            // PlayerControllerCCF.OnTriggerEnter2D() 내에서
            // 스토리모드에서만 등장하는 Arrow와 닿았을때,
            // pythagorasOn == true이면 
            // Managers.Grace.Pythagoras = false; 만 하고 return;
        }
    }

    /// <summary>
    /// 피타고라스 가호 끝내기.
    /// </summary>
    /// <param name="Ptrangle">지속시간동안 화면에 떠있을 삼각형 오브젝트</param>
    /// <param name="PtrangleBack">삼각형 배경</param>
    /// <param name="time">지속시간</param>
    /// <returns></returns>
    IEnumerator EndPythagoras(GameObject Ptrangle, GameObject PtrangleBack, float time)
    {
        yield return new WaitForSeconds(time);

        UnityEngine.Object.Destroy(Ptrangle);
        UnityEngine.Object.Destroy(PtrangleBack);
        pythagorasOn = false;
        playerCollisionOff = false;
        Debug.Log("End Pythagoras!");
    }

    /// <summary>
    /// 뉴턴의 가호 : 3초동안 모든 것을 끌어당기는 만유인력의 힘이 생기며, 
    /// 모인 숫자와 기호들을 활용해 자동으로 3번 정답을 계산해줌 (그냥 정답 처리를 해준다).
    /// </summary>
    public void GraceOfNewton()
    {
        if (newtonOn) return;
        newtonOn = true;
        playerCollisionOff = true;

        // Scene에 따라 다르게 행동 => StoryGameScene / Fight1vs1GameScene
        bool isthisStoryScene = (Managers.Scene.CurrentSceneType == Define.Scene.StoryGameScene);
        string tag = isthisStoryScene ? "Arrow" : "ArrowOnlyin1vs1";
        List<GameObject> arrows = GameObject.FindGameObjectsWithTag(tag).ConvertTo<List<GameObject>>();

        CoroutineHandler.StartCoroutine(NewtonForce(arrows,3));
    }

    /// <summary>
    /// 뉴턴의 가호로 화살 끌어오고, 가호 끝내기
    /// </summary>
    /// <param name="arrows">대상 화살들</param>
    /// <param name="AnsNum">정답 처리 횟수</param>
    /// <returns></returns>
    IEnumerator NewtonForce(List<GameObject> arrows, int AnsNum)
    {
        int exit = 0;

        while (exit < arrows.Count)
        {
            foreach (GameObject arrow in arrows)
            {
                if (arrow.IsDestroyed()) { exit++; continue; }
                Vector2 force = playerPos - arrow.transform.position;
                arrow.GetComponent<Rigidbody2D>().AddForceAtPosition(force, playerPos, ForceMode2D.Impulse);
            }
            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.8f);
        newtonOn = false;
        playerCollisionOff = false;
        if (Managers.Scene.CurrentSceneType == Define.Scene.StoryGameScene)
        {
            int damage = Managers.Game.Damage;
            GameObject ui_storyGame = player.transform.parent.gameObject;
            // 데미지 3번
            ui_storyGame.GetComponent<UI_StoryGame>().damageToWitch(damage);
            ui_storyGame.GetComponent<UI_StoryGame>().damageToWitch(damage);
            ui_storyGame.GetComponent<UI_StoryGame>().damageToWitch(damage);
        }
        else if(Managers.Scene.CurrentSceneType == Define.Scene.Fight1vs1GameScene)
        {
            WJ_Sample1vs1 wJ_Sample1vs1 = player._fight1vs1sceneUi.wj_sample1vs1;

            for (int i = 0; i < AnsNum; i++)
            {
                int index = wJ_Sample1vs1.currentQuestionIndex;
                if(index >= 8)
                {
                    yield return CoroutineHandler.StartCoroutine(Managers.Connector.Send_Learning());
                    i--;
                    continue;
                }
                string qstCransr = Managers.Connector.cLearnSet.data.qsts[index].qstCransr;
                wJ_Sample1vs1.SelectAnswer(qstCransr);
            }

            player.transform.parent.GetComponent<UI_Fight1vs1Game>().Invoke("RefreshUI", 0);
        }
    }

}