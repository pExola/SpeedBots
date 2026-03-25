using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    public float interactRange = 1.0f;
    public LayerMask interactableLayer;

    private PlayerOverworld movement;

    void Awake()
    {
        movement = GetComponent<PlayerOverworld>();
    }

    void Update()
    {
        if (Keyboard.current != null && (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame))
        {
            if (DialogueManager.Instance != null && DialogueManager.Instance.isTalking)
            {
                DialogueManager.Instance.AvancarDialogo();
            }
            else
            {
                TentarInteragir();
            }
        }
    }

    void TentarInteragir()
    {
        // 1. Avisa que o botão funcionou e mostra a direção
        Debug.Log($"[INTERAÇÃO] Botão apertado! Atirando raio na direção: {movement.lastFacingDirection}");

        // 2. MÁGICA VISUAL: Desenha o raio na aba SCENE por 2 segundos
        Vector2 origem = transform.position;
        Vector2 direcao = movement.lastFacingDirection * interactRange;
        Debug.DrawRay(origem, direcao, Color.red, 2f);

        // 3. Atira o raio de verdade
        RaycastHit2D hit = Physics2D.Raycast(origem, movement.lastFacingDirection, interactRange, interactableLayer);

        if (hit.collider != null)
        {
            Debug.Log($"[INTERAÇÃO] O raio bateu em: {hit.collider.gameObject.name}");

            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                Debug.Log("[INTERAÇÃO] Sucesso! O objeto tem a interface. Iniciando diálogo...");
                interactable.Interagir();
            }
            else
            {
                Debug.Log($"[INTERAÇÃO] Falha: O objeto {hit.collider.gameObject.name} não possui o script NPC.");
            }
        }
        else
        {
            Debug.Log("[INTERAÇÃO] O raio não bateu em nada. Verifique a distância ou a Layer no Inspector.");
        }
    }
}
