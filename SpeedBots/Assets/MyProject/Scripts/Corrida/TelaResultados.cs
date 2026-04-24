using UnityEngine; // Importa as ferramentas principais da Unity.
using TMPro; // Importa a biblioteca para formatar os textos com alta qualidade na UI.
using UnityEngine.SceneManagement; // Importa o gerenciador de cenas para podermos voltar ao Overworld.

public class TelaResultados : MonoBehaviour // O seu script original, agora no estilo Sonic!
{
    // Singleton rápido para a Linha de Chegada conseguir achar e chamar essa tela facilmente
    public static TelaResultados Instance { get; private set; }

    [Header("UI e Status")]
    public GameObject painelResultados; // O container invisível que segura os textos 

    // O texto único foi dividido em três para podermos organizar na tela como no jogo do Sonic
    public TextMeshProUGUI textoTitulo; // Para o "SAM VENCEU!!!" bem grande
    public TextMeshProUGUI textoTempo;  // Para mostrar o tempo de corrida
    public TextMeshProUGUI textoXP; // Para mostrar o XP

    [Header("Transição")]
    [Tooltip("Digite o nome exato da sua cena do mapa principal")]
    public string nomeCenaOverworld = "Overworld"; // Salva para onde o jogador vai ao clicar em Continuar.

    // --- VARIÁVEIS DO CRONÔMETRO ---
    private float tempoAtualDaCorrida = 0f;
    private bool cronometroRodando = false;

    void Awake()
    {
        // Configura o Singleton: garante que essa tela seja única e acessível
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Garante que o painel comece desligado para não aparecer no meio da corrida
        if (painelResultados != null) painelResultados.SetActive(false);
    }

    void Start()
    {
        // Assim que a pista carrega, o cronômetro zera e começa a rodar!
        tempoAtualDaCorrida = 0f;
        cronometroRodando = true;
    }

    void Update()
    {
        // Se a corrida não acabou, continua somando os segundos
        if (cronometroRodando)
        {
            tempoAtualDaCorrida += Time.deltaTime;
        }
    }

    // A função principal foi atualizada para receber também o tempo final e o XP ganho da linha de chegada
    public void MostrarResultados(bool vitoria, int xpGanho)
    {
        cronometroRodando = false;
        painelResultados.SetActive(true); // Liga os textos na tela

        if (vitoria) // Se cruzou a linha de chegada a tempo...
        {
            // 1. Configura o Título
            textoTitulo.text = "SAM VENCEU!!!";
            textoTitulo.color = Color.yellow; // Pinta o título de amarelo vitória

        }
        else // Se o tempo acabou ou o robô foi destruído...
        {
            textoTitulo.text = "SAM PERDEU...";
            textoTitulo.color = new Color(0.7f, 0.7f, 0.7f); // Pinta de cinza 
        }

        // 3. Usa o tempo que o próprio script calculou para formatar na tela
        int minutos = Mathf.FloorToInt(tempoAtualDaCorrida / 60f);
        int segundos = Mathf.FloorToInt(tempoAtualDaCorrida % 60f);
        textoTempo.text = $"TEMPO      {minutos}:{segundos:00}";

        // 3. XP limpo e direto, exatamente como você pediu
        textoXP.text = $"XP         {xpGanho}";
    }

    // Função para o Botão Continuar
    public void VoltarParaOverworld()
    {
        // Como nós não pausamos mais o Time.timeScale lá em cima, você pode apenas carregar a cena direto!
        SceneManager.LoadScene(nomeCenaOverworld);
    }
}