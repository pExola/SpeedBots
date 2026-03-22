using UnityEngine;

public class CameraDinamica : MonoBehaviour
{
    [Header("Alvo e Configurações")]
    public Transform alvo;

    public float suavidade = 0.15f;

    [Header("Posicionamento (Offset)")]
    public Vector3 avancoDireita = new Vector3(6f, 2f, -10f);
    public Vector3 avancoEsquerda = new Vector3(-6f, 2f, -10f);

    private Vector3 velocidadeRef = Vector3.zero;
    private SpeedBotMovment motorPlayer;

    void Start()
    {
        // 1. Verifica se você arrastou algo para a gaveta
        if (alvo == null)
        {
            return;
        }

        motorPlayer = alvo.GetComponent<SpeedBotMovment>();

    }

    void LateUpdate()
    {
        if (alvo == null || motorPlayer == null) return;

        float direcao = motorPlayer.GetDirecaoOlhar();
        Vector3 offsetDesejado = (direcao > 0) ? avancoDireita : avancoEsquerda;
        Vector3 posicaoAlvo = alvo.position + offsetDesejado;

        transform.position = Vector3.SmoothDamp(transform.position, posicaoAlvo, ref velocidadeRef, suavidade);
    }
}
