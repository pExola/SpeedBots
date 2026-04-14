using UnityEngine; // Importa as ferramentas principais do motor da Unity.

public class SpeedBotProgressionIA : MonoBehaviour // É o "irmão gêmeo" do sistema de progressão, feito exclusivamente para a Inteligência Artificial.
{
    [Header("Configuração de Dificuldade")] // Cria um cabeçalho organizado no Inspector.

    // Mostra um aviso quando você passa o mouse por cima no Inspector.
    [Tooltip("Define o Nível do inimigo. Altere isso para testar o balanceamento da pista.")]

    // A mágica do Level Design: Cria um slider (barra de arrastar) de 1 a 20 na Unity.
    // Se a pista estiver fácil demais, basta arrastar para o nível 10 e ele ganha todos os buffs sozinho, sem farmar XP!
    [Range(1, 20)] public int nivel = 1;

    [Header("Física Base (Oculto)")]
    public float velocidadeInicial = 10f; // Velocidade de fábrica da IA no Nível 1.
    public float aceleracaoInicial = 12f; // Aceleração de fábrica da IA no Nível 1.

    private SpeedBotIA movementScript; // Guarda a referência do "cérebro" da Inteligência Artificial.

    // Espaços para "salvar" os status originais do chassi da IA no Nível 1.
    private float arrancadaInicial;
    private float durabilidadeInicial;
    private float puloInicial;
    private float wallJumpYInicial;
    private float wallJumpXInicial;

    void Awake() // Roda no exato momento em que o robô inimigo nasce na fase.
    {
        movementScript = GetComponent<SpeedBotIA>(); // Conecta o script diretamente no cérebro da IA.

        // Salva as capacidades originais do chassi da IA antes de aplicar os buffs de nível.
        arrancadaInicial = movementScript.arrancadaBase;
        durabilidadeInicial = movementScript.durabilidadeBase;
        puloInicial = movementScript.forcaPulo;
        wallJumpYInicial = movementScript.forcaWallJumpY;
        wallJumpXInicial = movementScript.forcaWallJumpX;

        // Ejeta todos os cálculos no cérebro da IA logo no primeiro frame do jogo.
        AtualizarAtributosNoMotor();
    }

    public void AtualizarAtributosNoMotor() // O método que garante que a IA siga exatamente as mesmas regras de crescimento do Jogador.
    {
        // 1. CALCULADORA DAS 3 FASES (Exatamente igual ao Player para ser uma corrida justa)
        int niveisFase1 = Mathf.Clamp(nivel - 1, 0, 4);  // Descobre quantos níveis ela ganhou na Fase 1 (onde o crescimento é muito forte).
        int niveisFase2 = Mathf.Clamp(nivel - 5, 0, 7);  // Descobre quantos níveis ganhou na Fase 2 (crescimento médio).
        int niveisFase3 = Mathf.Clamp(nivel - 12, 0, 8); // Descobre quantos níveis ganhou na Fase 3 (crescimento lento).

        // 2. APLICAÇÃO NO MOTOR PRINCIPAL (Ejetando a velocidade na IA)
        movementScript.velocidadeMaximaBase = velocidadeInicial
                                            + (niveisFase1 * 2f)    // Ganha +2 por nível na fase 1.
                                            + (niveisFase2 * 1f)    // Ganha +1 por nível na fase 2.
                                            + (niveisFase3 * 0.5f); // Ganha +0.5 por nível na fase 3.

        // Ejetando a aceleração na IA (A mesma matemática da Fase 1 = +2, Fase 2 = +1.5, Fase 3 = +1)
        movementScript.aceleracaoBase = aceleracaoInicial
                                      + (niveisFase1 * 2f)
                                      + (niveisFase2 * 1.5f)
                                      + (niveisFase3 * 1f);

        // 3. APLICAÇÃO NA ARRANCADA E DURABILIDADE (Sinergias de Terreno e Resistência)
        // Calcula os bônus percentuais pequenos baseados na mesma curva de 3 Fases.
        float ganhoArrancada = (niveisFase1 * 0.02f) + (niveisFase2 * 0.015f) + (niveisFase3 * 0.01f);
        float ganhoDurabilidade = (niveisFase1 * 0.02f) + (niveisFase2 * 0.01f) + (niveisFase3 * 0.005f);

        // Soma os ganhos com a base e injeta na IA, garantindo pelo Clamp01 que o limite não passe de 1.0 (100%).
        movementScript.arrancadaBase = Mathf.Clamp01(arrancadaInicial + ganhoArrancada);
        movementScript.durabilidadeBase = Mathf.Clamp01(durabilidadeInicial + ganhoDurabilidade);

        // 4. PARKOUR (Melhorando o pulo para acompanhar a velocidade de níveis mais altos)
        float ganhoPulo = (niveisFase1 * 0.2f) + (niveisFase2 * 0.1f) + (niveisFase3 * 0.05f);

        // Ejeta a força final de pulos diretos nas "pernas" do cérebro da IA.
        movementScript.forcaPulo = puloInicial + ganhoPulo;
        movementScript.forcaWallJumpY = wallJumpYInicial + ganhoPulo;
        movementScript.forcaWallJumpX = wallJumpXInicial + (ganhoPulo / 2f);
    }
}