using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
// Transformei em class para facilitar a manipulação
[System.Serializable]
public class SecondaryQuest
{
    public string sourceNPC; // Usado apenas nos bastidores para controle
    public string title;     // Nome da missão (Ex: "Passado oculto")
    public string hint;      // A pista (Ex: "Vá até o Setor silencioso")
}

public class QuestHUDManager : MonoBehaviour
{
    [Header("Controle Geral")]
    [Tooltip("Arraste o QuestHUD_Panel aqui (ele precisa ter um Canvas Group)")]
    public CanvasGroup hudCanvasGroup; // <- Mudou de GameObject para CanvasGroup

    [Header("Main Quest UI")]
    public TextMeshProUGUI txtMainHint;

    [Header("Secondary Quests UI")]
    public GameObject secondaryQuestGroup;
    public TextMeshProUGUI[] txtSecondarySlots = new TextMeshProUGUI[4];

    private List<SecondaryQuest> activeSecondaryQuests = new List<SecondaryQuest>();

    void Start()
    {
        // Força a limpeza inicial. Esconde todos os 4 slots na inicialização
        // para garantir que nenhum texto placeholder fique aparecendo.
        foreach (var slot in txtSecondarySlots)
        {
            if (slot != null)
            {
                slot.gameObject.SetActive(false);
            }
        }

        UpdateSecondaryQuestsUI();
    }

    void Update()
    {
        // Verifica se o TAB foi pressionado
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            if (hudCanvasGroup != null)
            {
                // Se o Alpha for 1 (visível), muda para 0 (invisível), e vice-versa
                bool isVisible = hudCanvasGroup.alpha > 0f;
                hudCanvasGroup.alpha = isVisible ? 0f : 1f;

                // Opcional: Desativa a interação com o mouse quando estiver invisível
                hudCanvasGroup.interactable = !isVisible;
                hudCanvasGroup.blocksRaycasts = !isVisible;
            }
        }
    }

    /// <summary>
    /// Chama isso no NPC. O npcName fica oculto, serve só de ID.
    /// </summary>
    public void AddSecondaryQuest(string npcName, string questTitle, string hint)
    {
        // Verifica se esse NPC já deu a missão para não duplicar
        if (activeSecondaryQuests.Exists(q => q.sourceNPC == npcName))
        {
            return;
        }

        if (activeSecondaryQuests.Count >= 4)
        {
            Debug.LogWarning("Limite de 4 missões atingido.");
            return;
        }

        activeSecondaryQuests.Add(new SecondaryQuest
        {
            sourceNPC = npcName,
            title = questTitle,
            hint = hint
        });

        UpdateSecondaryQuestsUI();
    }

    private void UpdateSecondaryQuestsUI()
    {
        if (activeSecondaryQuests.Count == 0)
        {
            secondaryQuestGroup.SetActive(false);
            return;
        }

        secondaryQuestGroup.SetActive(true);

        for (int i = 0; i < txtSecondarySlots.Length; i++)
        {
            // Checagem de segurança para evitar NullReference caso esqueça de linkar no Inspector
            if (txtSecondarySlots[i] == null) continue;

            if (i < activeSecondaryQuests.Count)
            {
                txtSecondarySlots[i].gameObject.SetActive(true);
                // Aplica a formatação Título em negrito e pista normal
                txtSecondarySlots[i].text = $"<b>{activeSecondaryQuests[i].title}</b>: {activeSecondaryQuests[i].hint}";
            }
            else
            {
                // Esconde estritamente os slots que ainda não têm missão
                txtSecondarySlots[i].gameObject.SetActive(false);
            }
        }
    }
}