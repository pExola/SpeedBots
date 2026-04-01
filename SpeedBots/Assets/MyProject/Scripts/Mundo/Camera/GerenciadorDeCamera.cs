using UnityEngine;
using System.Collections;

public class GerenciadorDeCamera : MonoBehaviour
{
    public static GerenciadorDeCamera Instance { get; private set; }

    [Header("Configurações da Transição")]
    public bool transicaoSuave = true;
    public float velocidadeTransicao = 30f;

    private bool iniciou = false;

    // NOVA VARIÁVEL: Guarda para qual sala a câmera está olhando
    private Vector3 alvoAtual;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartCoroutine(AtrasarInicio());
    }

    private IEnumerator AtrasarInicio()
    {
        yield return new WaitForEndOfFrame();
        iniciou = true;
    }

    public void MudarEnquadramento(Vector3 centroDaNovaSala)
    {
        Vector3 posicaoAlvo = new Vector3(centroDaNovaSala.x, centroDaNovaSala.y, transform.position.z);

        // NOVA PROTEÇÃO: Se já estamos na sala pedida (ou indo para ela), ignora o comando!
        if (alvoAtual == posicaoAlvo) return;

        // Se for uma sala nova, registra o novo alvo
        alvoAtual = posicaoAlvo;

        if (!iniciou)
        {
            transform.position = posicaoAlvo;
            return;
        }

        if (transicaoSuave)
        {
            StopAllCoroutines();
            StartCoroutine(MoverCameraCoroutine(posicaoAlvo));
        }
        else
        {
            transform.position = posicaoAlvo;
        }
    }

    private IEnumerator MoverCameraCoroutine(Vector3 alvo)
    {
        while (Vector3.Distance(transform.position, alvo) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, alvo, velocidadeTransicao * Time.deltaTime);
            yield return null;
        }

        transform.position = alvo;
    }
}