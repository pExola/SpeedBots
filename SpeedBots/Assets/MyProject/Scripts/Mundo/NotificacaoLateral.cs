using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NotificacaoLateral : MonoBehaviour
{
    public static NotificacaoLateral Instance { get; private set; }

    [Header("Componentes da UI")]
    public RectTransform painelAnimado;
    public TextMeshProUGUI textoNotificacao;

    [Header("Configurações do Game Feel")]
    public float tempoNaTela = 2f;
    // Como agora usamos pixels reais por segundo, aumente esse valor no Inspector (ex: 800 a 1500)
    public float velocidadeDeslize = 1200f;

    // Posições (Âncora na direita da tela, Pivot X = 1)
    private Vector2 posicaoEntrada = new Vector2(800f, 0f);  // Bem escondido na direita
    private Vector2 posicaoVisivel = new Vector2(-20f, 0f);  // Ponto de parada na tela
    private Vector2 posicaoSaida = new Vector2(-3000f, 0f);  // Um destino bem longo para a esquerda

    private Queue<string> filaDeAvisos = new Queue<string>();
    private bool estaMostrando = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (painelAnimado != null)
        {
            painelAnimado.anchoredPosition = posicaoEntrada;
            // Garante que o painel nasça desativado para não poluir a tela
            painelAnimado.gameObject.SetActive(false);
        }
    }

    public void MostrarLoot(string nomeItem, int quantidade)
    {
        filaDeAvisos.Enqueue($"+{quantidade} {nomeItem}");

        if (!estaMostrando)
        {
            StartCoroutine(ProcessarFila());
        }
    }

    private IEnumerator ProcessarFila()
    {
        estaMostrando = true;

        while (filaDeAvisos.Count > 0)
        {
            textoNotificacao.text = filaDeAvisos.Dequeue();

            // Coloca o painel na direita e LIGA ele
            painelAnimado.anchoredPosition = posicaoEntrada;
            painelAnimado.gameObject.SetActive(true);

            // 1. Desliza para DENTRO (Da Direita para a Esquerda)
            // O MoveTowards garante que a velocidade visual seja sempre idêntica
            while (Vector2.Distance(painelAnimado.anchoredPosition, posicaoVisivel) > 0.1f)
            {
                painelAnimado.anchoredPosition = Vector2.MoveTowards(painelAnimado.anchoredPosition, posicaoVisivel, velocidadeDeslize * Time.deltaTime);
                yield return null;
            }
            painelAnimado.anchoredPosition = posicaoVisivel; // Crava a posição

            // 2. Espera na tela para leitura
            yield return new WaitForSeconds(tempoNaTela);

            // 3. Desliza para FORA (Continua indo para a Esquerda)
            while (Vector2.Distance(painelAnimado.anchoredPosition, posicaoSaida) > 0.1f)
            {
                painelAnimado.anchoredPosition = Vector2.MoveTowards(painelAnimado.anchoredPosition, posicaoSaida, velocidadeDeslize * Time.deltaTime);

                // Trava de otimização: Se ele já deslizou o suficiente para sair da visão (ex: -2000), encerra o loop.
                if (painelAnimado.anchoredPosition.x < -2000f) break;

                yield return null;
            }

            // O SEGUREDO: Desliga o painel completamente antes de voltar para a direita!
            // Assim, ele nunca será visto sendo "teletransportado".
            painelAnimado.gameObject.SetActive(false);

            // Um pequeno respiro antes de puxar o próximo item da fila
            yield return new WaitForSeconds(0.2f);
        }

        estaMostrando = false;
    }
}