using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FixarProporcaoTela : MonoBehaviour
{
    [Header("Qual é a proporção oficial do seu jogo?")]
    [Tooltip("O padrão da indústria hoje é 16:9")]
    public float proporcaoLargura = 16f;
    public float proporcaoAltura = 9f;

    void Start()
    {
        AjustarCamera();
    }

    // Se quiser que ajuste em tempo real ao redimensionar a janela no PC, 
    // mude 'Start()' para 'Update()', mas no Start consome menos processamento.
    private void AjustarCamera()
    {
        // Calcula a proporção que você deseja vs a proporção do monitor do jogador
        float targetAspect = proporcaoLargura / proporcaoAltura;
        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        Camera cam = GetComponent<Camera>();

        // Se a tela do jogador for mais "larga" que o jogo (ex: Ultrawide)
        // Adiciona barras pretas nas laterais (Pillarbox)
        if (scaleHeight < 1.0f)
        {
            Rect rect = cam.rect;
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
            cam.rect = rect;
        }
        else // Se a tela for mais "quadrada" que o jogo (ex: Monitores antigos)
             // Adiciona barras pretas em cima e embaixo (Letterbox)
        {
            float scaleWidth = 1.0f / scaleHeight;
            Rect rect = cam.rect;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
            cam.rect = rect;
        }
    }
}
