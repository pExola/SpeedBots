using UnityEngine;

public class SpeedBotProgression : MonoBehaviour
{
    [Header("Progresso do RPG")]
    public int nivel = 1;
    public int nivelMaximo = 20;
    public float currentXP = 0;
    public float xpToNextLevel = 100;

    [Header("Física (Oculto do Jogador)")]
    public float velocidadeInicial = 10f;
    public float aceleracaoInicial = 12f;

    // Limites absolutos calculados para o Nível 20 (Faz a UI bater 100%)
    private float velMaximaPossivel = 29f;
    private float acelMaximaPossivel = 38.5f;

    private SpeedBotMovment movementScript;

    void Awake()
    {
        movementScript = GetComponent<SpeedBotMovment>();
        AtualizarAtributosNoMotor();
    }

    // --- MÉTODOS PARA A UI (100% Visual) ---
    public int GetStatusVelocidade()
    {
        return Mathf.RoundToInt((movementScript.velocidadeMaximaBase / velMaximaPossivel) * 100f);
    }

    public int GetStatusAceleracao()
    {
        return Mathf.RoundToInt((movementScript.aceleracaoBase / acelMaximaPossivel) * 100f);
    }

    // --- APLICAÇÃO REAL NA FÍSICA (As 3 Fases de Nivelamento) ---
    public void AtualizarAtributosNoMotor()
    {
        float velAtual = velocidadeInicial;
        float acelAtual = aceleracaoInicial;

        // Fase 1 (Até o Nível 5): Ganha +2.0
        // São no máximo 4 'level ups' possíveis nesta fase
        int niveisFase1 = Mathf.Clamp(nivel - 1, 0, 4);
        velAtual += niveisFase1 * 2f;
        acelAtual += niveisFase1 * 2f;

        // Fase 2 (Nível 6 ao 12): Ganha +1.0 Vel / +1.5 Acel
        // São no máximo 7 'level ups' possíveis nesta fase
        int niveisFase2 = Mathf.Clamp(nivel - 5, 0, 7);
        velAtual += niveisFase2 * 1f;
        acelAtual += niveisFase2 * 1.5f;

        // Fase 3 (Nível 13 ao 20): Ganha +0.5 Vel / +1.0 Acel
        // São no máximo 8 'level ups' possíveis nesta fase
        int niveisFase3 = Mathf.Clamp(nivel - 12, 0, 8);
        velAtual += niveisFase3 * 0.5f;
        acelAtual += niveisFase3 * 1f;

        // Aplica os valores finais na física do motor
        movementScript.velocidadeMaximaBase = velAtual;
        movementScript.aceleracaoBase = acelAtual;
    }

    // --- SISTEMA DE XP ---
    public void AdicionarXP(float valor)
    {
        if (nivel >= nivelMaximo) return;

        currentXP += valor;
        if (currentXP >= xpToNextLevel) SubirDeNivel();
    }

    private void SubirDeNivel()
    {
        nivel++;
        currentXP -= xpToNextLevel;
        xpToNextLevel *= 1.5f;

        if (nivel > nivelMaximo) nivel = nivelMaximo;

        AtualizarAtributosNoMotor();
    }

}
