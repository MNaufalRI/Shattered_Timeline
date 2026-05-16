using UnityEngine;
using TMPro;

public class BossQuestManager : MonoBehaviour
{

    public enum BossQuestState
    {
        InvestigateSound,
        KillGluttony,
        ToBeContinued
    }

    [Header("Quest Status")]
    public BossQuestState currentQuest = BossQuestState.InvestigateSound;

    [Header("UI Components")]
    public TextMeshProUGUI questTitleText;
    public TextMeshProUGUI questObjectiveText;

    private void Start()
    {
        UpdateQuestUI();
    }

    private void UpdateQuestUI()
    {
        switch (currentQuest)
        {
            case BossQuestState.InvestigateSound:
                questTitleText.text = "Unknown Threat";
                questObjectiveText.text = "- Investigate the terrifying roar";
                break;

            case BossQuestState.KillGluttony:
                questTitleText.text = "Boss Encounter";
                questObjectiveText.text = "- Defeat The Gluttony";
                break;

            case BossQuestState.ToBeContinued:
                questTitleText.text = "To Be Continued";
                questObjectiveText.text = "This game is currently in development.\nStory continuation will be added soon.";
                break;
        }
    }

    public void StartBossFight()
    {
        if (currentQuest == BossQuestState.InvestigateSound)
        {
            currentQuest = BossQuestState.KillGluttony;
            UpdateQuestUI();
            Debug.Log("<color=orange>Quest Updated: Kill The Gluttony!</color>");
        }
    }

    public void BossDefeated()
    {
        if (currentQuest == BossQuestState.KillGluttony)
        {
            currentQuest = BossQuestState.ToBeContinued;
            UpdateQuestUI();
            Debug.Log("<color=cyan>Quest Updated: To Be Continued!</color>");
        }
    }
}