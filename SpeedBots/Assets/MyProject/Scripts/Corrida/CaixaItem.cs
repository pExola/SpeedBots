using UnityEngine;

public class CaixaItem : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. O primeiro aviso: bateu em qualquer coisa!
        Debug.Log($"[CAIXA] Algo bateu em mim! Nome do objeto: {collision.gameObject.name} | Tag: {collision.tag}");

        RaceItemController inventario = collision.GetComponent<RaceItemController>();

        if (inventario != null)
        {
            Debug.Log($"[CAIXA] O objeto {collision.gameObject.name} tem um ItemController!");

            if (inventario.itemGuardado == RaceItemController.TipoItem.Nenhum)
            {
                int sorteio = Random.Range(1, 4);
                inventario.PegarItem((RaceItemController.TipoItem)sorteio);
                Debug.Log($"[CAIXA] Sucesso! Dei o item {(RaceItemController.TipoItem)sorteio} para {collision.gameObject.name}. Destruindo a caixa...");

                Destroy(gameObject);
            }
            else
            {
                Debug.Log($"[CAIXA] Falhou: O {collision.gameObject.name} já tem o item {inventario.itemGuardado} guardado. Não vou dar outro.");
            }
        }
        else
        {
            Debug.Log($"[CAIXA] Falhou: O objeto {collision.gameObject.name} não possui o script ItemController anexado.");
        }
    }
}
