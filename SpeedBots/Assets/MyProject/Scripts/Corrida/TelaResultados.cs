using System.Collections; // Importa a biblioteca para usarmos Corrotinas
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TelaResultados : MonoBehaviour
{
    public static TelaResultados Instance { get; private set; }

    [Header("UI Simples (Estilo Sonic)")]
    public GameObject painelResultados;
    public TextMeshProUGUI textoTitulo;
    public TextMeshProUGUI textoTempo;
    public TextMeshProUGUI textoXP;
    public GameObject botaoContinuar;

    [Header("Transição")]
    public string nomeCenaOverworld = "Overworld";

    private float tempoAtualDaCorrida = 0f;
    private bool cronometroRodando = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (painelResultados != null) painelResultados.SetActive(false);
    }

    void Start()
    {
        tempoAtualDaCorrida = 0f;
        cronometroRodando = true;
    }

    void Update()
    {
        if (cronometroRodando)
        {
            tempoAtualDaCorrida += Time.deltaTime;
        }
    }

    public void MostrarResultados(bool vitoria, int xpGanho)
    {
        cronometroRodando = false; // Trava o cronômetro

        // A MÁGICA DA CÂMERA: Estaciona a câmera desligando o script dela
        if (Camera.main != null)
        {
            var cameraScript = Camera.main.GetComponent("CameraDinamica") as MonoBehaviour;
            if (cameraScript != null) cameraScript.enabled = false;
        }

        // Inicia a sequência de animação progressiva da UI
        StartCoroutine(SequenciaDeResultados(vitoria, xpGanho));
    }

    private IEnumerator SequenciaDeResultados(bool vitoria, int xpGanho)
    {
        // Passo 1: Esconde tudo para a tela nascer limpa
        painelResultados.SetActive(true);
        textoTitulo.gameObject.SetActive(false);
        textoTempo.gameObject.SetActive(false);
        textoXP.gameObject.SetActive(false);
        if (botaoContinuar != null) botaoContinuar.SetActive(false);

        yield return new WaitForSeconds(0.5f); // Respiro inicial

        // Passo 2: Revela o Título
        textoTitulo.gameObject.SetActive(true);
        if (vitoria)
        {
            textoTitulo.text = "SAM VENCEU!!!";
            textoTitulo.color = Color.yellow;
        }
        else
        {
            textoTitulo.text = "SAM PERDEU...";
            textoTitulo.color = new Color(0.7f, 0.7f, 0.7f);
        }

        yield return new WaitForSeconds(1.0f); // Pausa para ler o título

        // Passo 3: Revela o Tempo zerado e INICIA A CONTAGEM
        textoTempo.gameObject.SetActive(true);
        // O "yield return StartCoroutine" obriga o código a esperar a contagem terminar antes de ir para a próxima linha!
        // O número "1.0f" ali é a duração do efeito (vai levar 1 segundo contando do zero até o tempo real).
        yield return StartCoroutine(AnimarNumeroTempo(tempoAtualDaCorrida, 1.0f));

        yield return new WaitForSeconds(0.5f); // Pequena pausa entre o tempo terminar e o XP começar

        // Passo 4: Revela o XP zerado e INICIA A CONTAGEM
        textoXP.gameObject.SetActive(true);
        yield return StartCoroutine(AnimarNumeroXP(xpGanho, 1.0f)); // Conta o XP do 0 até o total durante 1 segundo

        yield return new WaitForSeconds(0.5f); // Pausa final

        // Passo 5: Acende o botão de continuar
        if (botaoContinuar != null) botaoContinuar.SetActive(true);
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