using UnityEngine; // Importa as ferramentas principais do motor da Unity.

public class SpeedBotProgression : MonoBehaviour // É o "Mestre de RPG" do jogador: controla XP, níveis e dita a evolução da física do robô.
{
    [Header("Progresso do RPG")] // Cria um cabeçalho organizado no Inspector.
    public int nivel = 1; // Nível atual do jogador (começa no 1).
    public int nivelMaximo = 20; // Limite máximo de evolução (nível 20).
    public float currentXP = 0; // Quantidade de pontos de experiência que o jogador tem no momento.
    public float xpToNextLevel = 100; // Meta de pontos necessária para alcançar o próximo nível.

    [Header("Física (Oculto do Jogador)")] // Dados invisíveis usados para o cálculo base.
    public float velocidadeInicial = 10f; // Velocidade de fábrica no Nível 1.
    public float aceleracaoInicial = 12f; // Aceleração de fábrica no Nível 1.

    // Limites absolutos calculados manualmente para o Nível 20. 
    // Servem exclusivamente para a UI comparar o status atual com o máximo e gerar uma porcentagem limpa.
    private float velMaximaPossivel = 29f;
    private float acelMaximaPossivel = 38.5f;

    private SpeedBotMovment movementScript; // Guarda a referência do script de movimento do robô para injetar a física lá dentro.

    // Espaços para "salvar" os status originais do chassi (Crawler, Slider, Aerial) no Nível 1.
    private float arrancadaInicial;
    private float durabilidadeInicial;
    private float puloInicial;
    private float wallJumpYInicial;
    private float wallJumpXInicial;

    void Awake() // Roda no exato momento em que o robô nasce no jogo.
    {
        movementScript = GetComponent<SpeedBotMovment>(); // Tenta achar e conectar o script de movimento.

        // Salva quem o robô é (e seus status) no Nível 1 com os novos nomes definidos no movimento.
        arrancadaInicial = movementScript.arrancadaBase;
        durabilidadeInicial = movementScript.durabilidadeBase;
        puloInicial = movementScript.forcaPulo;
        wallJumpYInicial = movementScript.forcaWallJumpY;
        wallJumpXInicial = movementScript.forcaWallJumpX;

        AtualizarAtributosNoMotor(); // Aplica a matemática inicial logo no primeiro frame para garantir que os status estejam corretos.
    }

    // --- MÉTODOS PARA A UI (100% Visual) ---
    public int GetStatusVelocidade() // Tira a "matemática bruta" das costas do jogador...
    {
        // ...e entrega para a UI apenas uma porcentagem limpa (ex: 85%), comparando o status atual com o limite pré-calculado do Nível 20.
        return Mathf.RoundToInt((movementScript.velocidadeMaximaBase / velMaximaPossivel) * 100f);
    }

    public int GetStatusAceleracao() // Mesma lógica da velocidade, mas para a aceleração.
    {
        return Mathf.RoundToInt((movementScript.aceleracaoBase / acelMaximaPossivel) * 100f);
    }

    // --- APLICAÇÃO REAL NA FÍSICA (As 3 Fases de Nivelamento) ---
    public void AtualizarAtributosNoMotor() // A inteligência principal da progressão: dita como o robô evolui.
    {
        // 1. CALCULADORA DAS 3 FASES (Curva de Progressão)
        // O Mathf.Clamp descobre exatamente quantos "Level Ups" o robô teve em cada uma das três fases.
        int niveisFase1 = Mathf.Clamp(nivel - 1, 0, 4);   // Fase 1: Do Nível 2 ao 5 (Ganha MUITO poder).
        int niveisFase2 = Mathf.Clamp(nivel - 5, 0, 7);   // Fase 2: Do Nível 6 ao 12 (Ganha poder MÉDIO).
        int niveisFase3 = Mathf.Clamp(nivel - 12, 0, 8);  // Fase 3: Do Nível 13 ao 20 (Ganha POUCO poder, não quebra o endgame).

        // 2. APLICAÇÃO NO MOTOR PRINCIPAL (Injetando a matemática de fato)
        // Multiplica os níveis ganhos na fase pelo seu respectivo peso de evolução (2f, 1f, 0.5f).
        movementScript.velocidadeMaximaBase = velocidadeInicial
                                            + (niveisFase1 * 2f)
                                            + (niveisFase2 * 1f)
                                            + (niveisFase3 * 0.5f);

        // Mesma lógica de peso, mas ajustada para a aceleração.
        movementScript.aceleracaoBase = aceleracaoInicial
                                      + (niveisFase1 * 2f)
                                      + (niveisFase2 * 1.5f)
                                      + (niveisFase3 * 1f);

        // 3. APLICAÇÃO NA ARRANCADA E DURABILIDADE (Acompanhando as 3 Fases proporcionalmente)
        // Transformamos os números grandes das fases em ganhos percentuais pequenos (ex: +2 vira +0.02f).
        float ganhoArrancada = (niveisFase1 * 0.02f) + (niveisFase2 * 0.015f) + (niveisFase3 * 0.01f);
        float ganhoDurabilidade = (niveisFase1 * 0.02f) + (niveisFase2 * 0.01f) + (niveisFase3 * 0.005f);

        // Somamos o ganho percentual com a Base Única de cada classe (Lida no Awake).
        // A trava Mathf.Clamp01 garante que o status percentual do motor nunca ultrapasse 1.0 (ou seja, máximo de 100% de eficiência).
        movementScript.arrancadaBase = Mathf.Clamp01(arrancadaInicial + ganhoArrancada);
        movementScript.durabilidadeBase = Mathf.Clamp01(durabilidadeInicial + ganhoDurabilidade);

        // 4. PARKOUR (Ganhos bem pequenos nas pernas apenas para compensar a inércia maior da alta velocidade nos níveis altos)
        float ganhoPulo = (niveisFase1 * 0.2f) + (niveisFase2 * 0.1f) + (niveisFase3 * 0.05f);
        movementScript.forcaPulo = puloInicial + ganhoPulo; // Repassa a força do pulo para o motor.
        movementScript.forcaWallJumpY = wallJumpYInicial + ganhoPulo; // Repassa o Wall Jump Vertical.
        movementScript.forcaWallJumpX = wallJumpXInicial + (ganhoPulo / 2f); // X cresce menos (dividido por 2) para o robô não voar longe da parede.
    }

    // --- SISTEMA DE XP ---
    public void AdicionarXP(float valor) // Função chamada pelo juiz (FinishLine) quando o jogador ganha uma corrida.
    {
        if (nivel >= nivelMaximo) return; // Se já chegou no nível 20, recusa o XP para não quebrar o jogo.

        currentXP += valor; // Acumula o XP recebido na carteira do jogador.
        if (currentXP >= xpToNextLevel) SubirDeNivel(); // Se o XP acumulado bater ou passar a meta, aciona a evolução.
    }

    private void SubirDeNivel() // Acontece sempre que a meta de XP é atingida.
    {
        nivel++; // Aumenta o nível visual em +1.
        currentXP -= xpToNextLevel; // Desconta da carteira o XP que foi "gasto" para upar (mantendo a sobra, se houver).
        xpToNextLevel *= 1.5f; // Cria a clássica "escadinha de dificuldade": o próximo nível exige 1.5x mais XP que o anterior.

        if (nivel > nivelMaximo) nivel = nivelMaximo; // Trava de segurança final para não passar do nível 20 por acidente.

        AtualizarAtributosNoMotor(); // Agora que upou, roda toda a calculadora de 3 Fases lá de cima para deixar o robô mais forte!
    }
}