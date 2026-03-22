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

    private float arrancadaInicial;
    private float durabilidadeInicial;
    private float puloInicial;
    private float wallJumpYInicial;
    private float wallJumpXInicial;
    void Awake()
    {
        movementScript = GetComponent<SpeedBotMovment>();

        // Salva quem o robô é no Nível 1 com os novos nomes
        arrancadaInicial = movementScript.arrancadaBase;
        durabilidadeInicial = movementScript.durabilidadeBase;
        puloInicial = movementScript.forcaPulo;
        wallJumpYInicial = movementScript.forcaWallJumpY;
        wallJumpXInicial = movementScript.forcaWallJumpX;

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
        // 1. CALCULADORA DAS 3 FASES
        // Descobre exatamente quantos "Level Ups" o robô teve em cada fase
        int niveisFase1 = Mathf.Clamp(nivel - 1, 0, 4);   // Níveis 2 ao 5
        int niveisFase2 = Mathf.Clamp(nivel - 5, 0, 7);   // Níveis 6 ao 12
        int niveisFase3 = Mathf.Clamp(nivel - 12, 0, 8);  // Níveis 13 ao 20

        // 2. APLICAÇÃO NO MOTOR PRINCIPAL (A matemática que você definiu)
        movementScript.velocidadeMaximaBase = velocidadeInicial
                                            + (niveisFase1 * 2f)
                                            + (niveisFase2 * 1f)
                                            + (niveisFase3 * 0.5f);

        movementScript.aceleracaoBase = aceleracaoInicial
                                      + (niveisFase1 * 2f)
                                      + (niveisFase2 * 1.5f)
                                      + (niveisFase3 * 1f);

        // 3. APLICAÇÃO NA ARRANCADA E DURABILIDADE (Acompanhando as 3 Fases proporcionalmente)
        // Transformamos os números em percentuais (+2 vira +0.02f)
        float ganhoArrancada = (niveisFase1 * 0.02f) + (niveisFase2 * 0.015f) + (niveisFase3 * 0.01f);
        float ganhoDurabilidade = (niveisFase1 * 0.02f) + (niveisFase2 * 0.01f) + (niveisFase3 * 0.005f);

        // Somamos o ganho com a Base Única de cada classe (Lida no Awake)
        // O Clamp01 garante que o status nunca ultrapasse 1.0 (100% de eficiência)
        movementScript.arrancadaBase = Mathf.Clamp01(arrancadaInicial + ganhoArrancada);
        movementScript.durabilidadeBase = Mathf.Clamp01(durabilidadeInicial + ganhoDurabilidade);

        // 4. PARKOUR (Ganhos bem pequenos apenas para compensar a inércia da alta velocidade)
        float ganhoPulo = (niveisFase1 * 0.2f) + (niveisFase2 * 0.1f) + (niveisFase3 * 0.05f);
        movementScript.forcaPulo = puloInicial + ganhoPulo;
        movementScript.forcaWallJumpY = wallJumpYInicial + ganhoPulo;
        movementScript.forcaWallJumpX = wallJumpXInicial + (ganhoPulo / 2f); // X cresce menos para não voar longe
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
