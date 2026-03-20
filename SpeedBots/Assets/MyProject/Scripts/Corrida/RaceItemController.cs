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
            // O jogador usa o item no SHIFT
            if (Keyboard.current != null && Keyboard.current.shiftKey.wasPressedThisFrame)
            {
                UsarItem();
            }
        }
        else
        {
            // Lógica simples da IA: se pegar um item, usa ele depois de 1 a 3 segundos
            if (itemGuardado != TipoItem.Nenhum)
            {
                tempoParaIAUsarItem -= Time.deltaTime;
                if (tempoParaIAUsarItem <= 0) UsarItem();
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
        // 1. Descobre o tamanho do robô para atirar o gancho pelo "ombro" dele, e não de dentro da barriga
        CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();
        float offset = (col != null) ? col.bounds.extents.x + 0.5f : 1.0f;

        Vector2 origem = new Vector2(transform.position.x + (direcao * offset), transform.position.y);
        float alcance = 15f;

        // 2. Lança o raio invisível
        RaycastHit2D hit = Physics2D.Raycast(origem, new Vector2(direcao, 0), alcance);

        // 3. MÁGICA VISUAL: Desenha uma linha vermelha na aba SCENE por 2 segundos para vocês VEREM o tiro
        Debug.DrawRay(origem, new Vector2(direcao * alcance, 0), Color.magenta, 2f);

        if (hit.collider != null)
        {
            Debug.Log($"[GANCHO] O tiro pegou em: {hit.collider.name} (Tag: {hit.collider.tag})");

            SpeedBotMovment vitimaPlayer = hit.collider.GetComponent<SpeedBotMovment>();
            SpeedBotIA vitimaIA = hit.collider.GetComponent<SpeedBotIA>();

            if (vitimaPlayer != null && !ehJogador)
            {
                Debug.Log("[GANCHO] Sucesso! A IA puxou o Player!");
                vitimaPlayer.SofrerPuxao(20f, -direcao);
            }
            else if (vitimaIA != null && ehJogador)
            {
                Debug.Log("[GANCHO] Sucesso! O Player puxou a IA!");
                vitimaIA.SofrerPuxao(20f, -direcao);

                // Dá um pequeno bônus de velocidade para o Player que acertou o tiro
                if (motorPlayer != null) motorPlayer.AtivarNitro(1.3f, 1f);
            }
        }
        else
        {
            Debug.Log("[GANCHO] O gancho foi atirado, mas não acertou nada (bateu no vento).");
        }
    }
}
