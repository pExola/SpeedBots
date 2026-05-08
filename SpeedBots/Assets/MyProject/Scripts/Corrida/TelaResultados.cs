using System.Collections; // Importa a biblioteca para usarmos Corrotinas
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class TelaResultados : MonoBehaviour
{
    public static TelaResultados Instance { get; private set; }

    [Header("UI Simples")]
    public GameObject painelResultados;
    public TextMeshProUGUI textoTitulo;
    public TextMeshProUGUI textoTempo;
    public TextMeshProUGUI textoXP;
    public GameObject botaoContinuar;

    [Header("Cutscene de Derrota")]
    public GameObject painelCutscene; // A Raw Image 
    public VideoPlayer videoDerrota;  // O componente que toca o vídeo
    public TextMeshProUGUI textoCutscene;

    [Header("Largada")]
    public TextMeshProUGUI textoLargada;
    // O "semáforo". Os robôs vão ler isso para saber se podem acelerar
    public bool corridaLiberada = false;

    [Header("Transição")]
    public string nomeCenaOverworld = "Overworld";
    public string nomeCenaPosDerrota = "Mundo_PosCorrida";

    private float tempoAtualDaCorrida = 0f;
    private bool cronometroRodando = false;
    private bool venceuACorrida = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (painelResultados != null) painelResultados.SetActive(false);
        if (painelCutscene != null) painelCutscene.SetActive(false);
    }

    void Start()
    {
        // Em vez de começar o cronômetro direto, nós chamamos a corrotina da largada!
        tempoAtualDaCorrida = 0f;
        cronometroRodando = false;
        corridaLiberada = false;

        StartCoroutine(RotinaDeLargada());
    }

    void Update()
    {
        if (cronometroRodando)
        {
            tempoAtualDaCorrida += Time.deltaTime;
        }
    }

    private IEnumerator RodarCutsceneDerrota()
    {
        painelResultados.SetActive(false);
        painelCutscene.SetActive(true);

        // Garante que o texto comece escondido
        if (textoCutscene != null) textoCutscene.gameObject.SetActive(false);

        videoDerrota.Play();

        // ESPERA O TEMPO DO GUIA: O texto deve surgir aos 4 segundos de vídeo
        yield return new WaitForSeconds(3.0f);

        if (textoCutscene != null)
        {
            StartCoroutine(EfeitoMaquinaDeEscrever("Eu... Perdi...?", 0.15f));
        }

        // Espera o restante do vídeo acabar (Duração total - 4 segundos já esperados)
        float tempoRestante = (float)videoDerrota.length - 3.0f;
        yield return new WaitForSeconds(Mathf.Max(0, tempoRestante));

        SceneManager.LoadScene(nomeCenaPosDerrota);
    }

    private IEnumerator EfeitoMaquinaDeEscrever(string textoFinal, float tempoPorLetra)
    {
        textoCutscene.text = ""; // Garante que a caixa de texto comece completamente vazia
        textoCutscene.gameObject.SetActive(true);

        // O "foreach" pega a frase inteira, quebra em letras individuais e faz um loop
        foreach (char letra in textoFinal.ToCharArray())
        {
            textoCutscene.text += letra; // Adiciona a próxima letra na tela
            yield return new WaitForSeconds(tempoPorLetra); // Espera uns milissegundos antes da próxima
        }
    }

    public void MostrarResultados(bool vitoria, int xpGanho)
    {
        cronometroRodando = false;
        venceuACorrida = vitoria;

        if (Camera.main != null)
        {
            var cameraScript = Camera.main.GetComponent("CameraDinamica") as MonoBehaviour;
            if (cameraScript != null) cameraScript.enabled = false;
        }

        // A cascata de XP agora roda sempre, ANTES de qualquer decisão de cena
        StartCoroutine(SequenciaDeResultados(vitoria, xpGanho));
    }

    private IEnumerator RotinaDeLargada()
    {
        textoLargada.gameObject.SetActive(true);

        textoLargada.text = "3";
        yield return new WaitForSeconds(1f);

        textoLargada.text = "2";
        yield return new WaitForSeconds(1f);

        textoLargada.text = "1";
        yield return new WaitForSeconds(1f);

        // O momento do disparo!
        textoLargada.text = "VAI!!!";
        textoLargada.color = Color.green; // Pinta de verde para dar o "Game Feel"

        // Acende o semáforo verde para os robôs e liga o relógio da fase!
        corridaLiberada = true;
        cronometroRodando = true;

        yield return new WaitForSeconds(1f); // Deixa o "VAI" na tela por 1 segundinho
        textoLargada.gameObject.SetActive(false); // Apaga o texto para limpar a visão da pista
    }

    private IEnumerator SequenciaDeResultados(bool vitoria, int xpGanho)
    {
        painelResultados.SetActive(true);
        textoTitulo.gameObject.SetActive(false);
        textoTempo.gameObject.SetActive(false);
        textoXP.gameObject.SetActive(false);
        if (botaoContinuar != null) botaoContinuar.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        textoTitulo.gameObject.SetActive(true);
        if (vitoria)
        {
            textoTitulo.text = "SAM VENCEU!!!";
            textoTitulo.color = Color.yellow;
        }
        else
        {
            textoTitulo.text = "SAM PERDEU...";
            
        }

        yield return new WaitForSeconds(1.0f);

        textoTempo.gameObject.SetActive(true);
        yield return StartCoroutine(AnimarNumeroTempo(tempoAtualDaCorrida, 1.0f));

        yield return new WaitForSeconds(0.5f);

        textoXP.gameObject.SetActive(true);
        yield return StartCoroutine(AnimarNumeroXP(xpGanho, 1.0f));

        yield return new WaitForSeconds(0.5f);

        // O botão aparece por último, respeitando a sua preferência
        if (botaoContinuar != null) botaoContinuar.SetActive(true);
    }

    // Esta função deve ser vinculada ao OnClick() do seu botão na Unity
    public void BotaoContinuarClicado()
    {
        if (venceuACorrida)
        {
            // Se venceu, volta para o mapa principal (Overworld)
            SceneManager.LoadScene(nomeCenaOverworld);
        }
        else
        {
            // Se perdeu, o clique no botão é o que dispara a cutscene
            StartCoroutine(RodarCutsceneDerrota());
        }
    }

    // --- AS NOVAS CORROTINAS DE GAME FEEL (CONTAGEM) ---

    private IEnumerator AnimarNumeroTempo(float tempoFinalCravado, float duracaoAnimacao)
    {
        float tempoRolando = 0f; // Nosso próprio relógio interno para a animação

        // Enquanto o tempo da animação não acabar...
        while (tempoRolando < duracaoAnimacao)
        {
            tempoRolando += Time.deltaTime; // Soma os frames

            // Mathf.Lerp calcula o valor intermediário exato. Ex: aos 0.5s de animação, ele vai estar na metade do tempoFinal.
            float valorCalculadoNesteFrame = Mathf.Lerp(0, tempoFinalCravado, tempoRolando / duracaoAnimacao);

            // Formata o número falso que está crescendo e joga na tela
            int min = Mathf.FloorToInt(valorCalculadoNesteFrame / 60f);
            int seg = Mathf.FloorToInt(valorCalculadoNesteFrame % 60f);
            textoTempo.text = $"TEMPO      {min}:{seg:00}";

            // Dica de Game Feel: Se quiser colocar som, coloque um AudioManager.Play("Tique") aqui!

            yield return null; // Pausa a corrotina e espera o próximo frame do jogo para atualizar o número de novo
        }

        // TRAVA DE SEGURANÇA: Garante que no último frame o número exibido seja exatamente o tempo real cravado, sem erros de arredondamento.
        int minFinais = Mathf.FloorToInt(tempoFinalCravado / 60f);
        int segFinais = Mathf.FloorToInt(tempoFinalCravado % 60f);
        textoTempo.text = $"TEMPO      {minFinais}:{segFinais:00}";
    }

    private IEnumerator AnimarNumeroXP(int xpFinal, float duracaoAnimacao)
    {
        float tempoRolando = 0f;

        while (tempoRolando < duracaoAnimacao)
        {
            tempoRolando += Time.deltaTime;

            // O RoundToInt garante que a tela nunca mostre "XP 45.3", arredondando sempre para números inteiros bonitos.
            int xpCalculadoNesteFrame = Mathf.RoundToInt(Mathf.Lerp(0, xpFinal, tempoRolando / duracaoAnimacao));

            textoXP.text = $"XP         {xpCalculadoNesteFrame}";

            yield return null; // Espera o próximo frame
        }

        // TRAVA DE SEGURANÇA: Crava o XP final exato na tela.
        textoXP.text = $"XP         {xpFinal}";
    }

    public void VoltarParaOverworld()
    {
        SceneManager.LoadScene(nomeCenaOverworld);
    }

}