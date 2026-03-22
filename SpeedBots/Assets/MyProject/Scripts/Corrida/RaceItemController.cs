using UnityEngine;
using UnityEngine.InputSystem;

public class RaceItemController : MonoBehaviour
{
    public enum TipoItem { Nenhum, Nitro, Armadilha, Gancho }

    [Header("Inventário")]
    public TipoItem itemGuardado = TipoItem.Nenhum;
    public GameObject prefabArmadilha; // Arraste a bolinha de stun aqui no Inspector

    [Header("Configuração")]
    public bool ehJogador = true; // Marque false na IA

    private SpeedBotMovment motorPlayer;
    private SpeedBotIA motorIA;
    private float tempoParaIAUsarItem = 0f;

    void Awake()
    {
        motorPlayer = GetComponent<SpeedBotMovment>();
        motorIA = GetComponent<SpeedBotIA>();
    }

    void Update()
    {
        if (ehJogador)
        {
            if (Keyboard.current != null && Keyboard.current.shiftKey.wasPressedThisFrame)
            {
                UsarItem();
            }
        }
        else
        {
            if (itemGuardado != TipoItem.Nenhum)
            {
                // INTELIGÊNCIA ARTIFICIAL: Se a arma for o Gancho, ela age como um franco-atirador
                if (itemGuardado == TipoItem.Gancho)
                {
                    float direcao = motorIA.GetDirecaoOlhar();
                    CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();
                    float offset = (col != null) ? col.bounds.extents.x + 0.5f : 1.0f;
                    Vector2 origem = new Vector2(transform.position.x + (direcao * offset), transform.position.y);

                    // A IA fica "olhando" para frente o tempo todo
                    RaycastHit2D hit = Physics2D.Raycast(origem, new Vector2(direcao, 0), 15f);

                    // Se o raio bater em você (Player), ela puxa o gatilho na hora!
                    if (hit.collider != null && hit.collider.CompareTag("Player"))
                    {
                        UsarItem();
                    }
                }
                else
                {
                    // Se for Nitro ou Armadilha, ela usa a contagem regressiva normal
                    tempoParaIAUsarItem -= Time.deltaTime;
                    if (tempoParaIAUsarItem <= 0) UsarItem();
                }
            }
        }
    }

    public void PegarItem(TipoItem novoItem)
    {
        if (itemGuardado == TipoItem.Nenhum)
        {
            itemGuardado = novoItem;
            if (!ehJogador) tempoParaIAUsarItem = Random.Range(1f, 3f); // IA decide quando vai usar
        }
    }

    public void UsarItem()
    {
        if (itemGuardado == TipoItem.Nenhum) return;

        float direcao = ehJogador ? motorPlayer.GetDirecaoOlhar() : motorIA.GetDirecaoOlhar();

        switch (itemGuardado)
        {
            case TipoItem.Nitro:
                if (ehJogador) motorPlayer.AtivarNitro(1.8f, 1.5f); // 80% mais rápido por 1.5s
                else motorIA.AtivarNitro(1.8f, 1.5f);
                break;

            case TipoItem.Armadilha:
                // Instancia a armadilha nas costas do robô
                Vector2 posArmadilha = new Vector2(transform.position.x - (direcao * 1.5f), transform.position.y);
                Instantiate(prefabArmadilha, posArmadilha, Quaternion.identity);
                break;

            case TipoItem.Gancho:
                AtirarGancho(direcao);
                break;
        }

        itemGuardado = TipoItem.Nenhum;
    }

    private void AtirarGancho(float direcao)
    {
        CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();
        float offset = (col != null) ? col.bounds.extents.x + 0.5f : 1.0f;

        Vector2 origem = new Vector2(transform.position.x + (direcao * offset), transform.position.y);
        float alcance = 15f;

        RaycastHit2D hit = Physics2D.Raycast(origem, new Vector2(direcao, 0), alcance);
        Debug.DrawRay(origem, new Vector2(direcao * alcance, 0), Color.magenta, 2f);

        if (hit.collider != null)
        {
            SpeedBotMovment vitimaPlayer = hit.collider.GetComponent<SpeedBotMovment>();
            SpeedBotIA vitimaIA = hit.collider.GetComponent<SpeedBotIA>();

            // Adicionamos o 1.5f (os segundos do debuff) na chamada do Puxão
            if (vitimaPlayer != null && !ehJogador)
            {
                vitimaPlayer.SofrerPuxao(20f, -direcao, 1.5f);
            }
            else if (vitimaIA != null && ehJogador)
            {
                vitimaIA.SofrerPuxao(20f, -direcao, 1.5f);
                if (motorPlayer != null) motorPlayer.AtivarNitro(1.3f, 1f);
            }
        }
    }
}
