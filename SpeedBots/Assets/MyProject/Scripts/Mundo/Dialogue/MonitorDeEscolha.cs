using UnityEngine;

public class MonitorDeEscolha : MonoBehaviour
{
    [Header("Configuração do Gatilho")]
    [Tooltip("Nome exato do nó do Twine que confirma a escolha do robô.")]
    public string noGatilho = "Escolhido";

    // Variável interna para evitar processamento desnecessário
    private bool jaRegistrouEscolha = false;

    void Update()
    {
        // 1. Se a escolha já foi feita, desliga o monitoramento
        if (jaRegistrouEscolha || SelecaoSpeedBot.escolheuRobo)
        {
            return;
        }

        // 2. Verifica se o DialogueManager está ativo e lendo uma conversa
        if (DialogueManager.Instance != null && DialogueManager.Instance.isTalking)
        {
            // 3. Puxa o nome do nó que criamos agora no DialogueManager
            string noQueEstaRolando = DialogueManager.Instance.tituloDoNoAtual;

            // 4. Bateu com o gatilho? Transforma a variável permanentemente em true!
            if (noQueEstaRolando == noGatilho)
            {
                SelecaoSpeedBot.escolheuRobo = true;
                jaRegistrouEscolha = true;

                Debug.Log($"[Monitor] Sucesso! O nó '{noGatilho}' iniciou. Variável escolheuRobo agora é TRUE.");
            }
        }
    }
}