using UnityEngine;

public class SpeedBotProgressionIA : MonoBehaviour
{
    [Header("Configuração de Dificuldade")]
    [Tooltip("Define o Nível do inimigo. Altere isso para testar o balanceamento da pista.")]
    [Range(1, 20)] public int nivel = 1;

    [Header("Física Base (Oculto)")]
    public float velocidadeInicial = 10f;
    public float aceleracaoInicial = 12f;

    private SpeedBotIA movementScript;

    private float arrancadaInicial;
    private float durabilidadeInicial;
    private float puloInicial;
    private float wallJumpYInicial;
    private float wallJumpXInicial;

    void Awake()
    {
        movementScript = GetComponent<SpeedBotIA>();

        arrancadaInicial = movementScript.arrancadaBase;
        durabilidadeInicial = movementScript.durabilidadeBase;
        puloInicial = movementScript.forcaPulo;
        wallJumpYInicial = movementScript.forcaWallJumpY;
        wallJumpXInicial = movementScript.forcaWallJumpX;

        AtualizarAtributosNoMotor();
    }

    public void AtualizarAtributosNoMotor()
    {
        // 1. CALCULADORA DAS 3 FASES (Exatamente igual ao Player)
        int niveisFase1 = Mathf.Clamp(nivel - 1, 0, 4);
        int niveisFase2 = Mathf.Clamp(nivel - 5, 0, 7);
        int niveisFase3 = Mathf.Clamp(nivel - 12, 0, 8);

        // 2. APLICAÇÃO NO MOTOR PRINCIPAL
        movementScript.velocidadeMaximaBase = velocidadeInicial
                                            + (niveisFase1 * 2f)
                                            + (niveisFase2 * 1f)
                                            + (niveisFase3 * 0.5f);

        movementScript.aceleracaoBase = aceleracaoInicial
                                      + (niveisFase1 * 2f)
                                      + (niveisFase2 * 1.5f)
                                      + (niveisFase3 * 1f);

        // 3. APLICAÇÃO NA ARRANCADA E DURABILIDADE
        float ganhoArrancada = (niveisFase1 * 0.02f) + (niveisFase2 * 0.015f) + (niveisFase3 * 0.01f);
        float ganhoDurabilidade = (niveisFase1 * 0.02f) + (niveisFase2 * 0.01f) + (niveisFase3 * 0.005f);

        movementScript.arrancadaBase = Mathf.Clamp01(arrancadaInicial + ganhoArrancada);
        movementScript.durabilidadeBase = Mathf.Clamp01(durabilidadeInicial + ganhoDurabilidade);

        // 4. PARKOUR
        float ganhoPulo = (niveisFase1 * 0.2f) + (niveisFase2 * 0.1f) + (niveisFase3 * 0.05f);
        movementScript.forcaPulo = puloInicial + ganhoPulo;
        movementScript.forcaWallJumpY = wallJumpYInicial + ganhoPulo;
        movementScript.forcaWallJumpX = wallJumpXInicial + (ganhoPulo / 2f);
    }
}
