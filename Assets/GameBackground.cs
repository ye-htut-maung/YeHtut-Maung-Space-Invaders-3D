using UnityEngine;

public class GameBackground : MonoBehaviour
{
    private Camera gameCamera;
    private SpriteRenderer background;

    private void Awake()
    {
        gameCamera = Camera.main;
        Sprite sprite = Resources.Load<Sprite>("Background");
        if (gameCamera == null || !gameCamera.orthographic || sprite == null)
        {
            enabled = false;
            return;
        }

        GameObject visual = new GameObject("StarfieldBackground");
        visual.transform.SetParent(transform, false);
        background = visual.AddComponent<SpriteRenderer>();
        background.sprite = sprite;
        background.sortingOrder = -100;
        LateUpdate();
    }

    private void LateUpdate()
    {
        if (gameCamera == null || background == null) return;

        // Uniform scaling fills the view without stretching; excess edges are cropped.
        float viewHeight = gameCamera.orthographicSize * 2f;
        Vector2 imageSize = background.sprite.bounds.size;
        float scale = Mathf.Max(viewHeight * gameCamera.aspect / imageSize.x,
            viewHeight / imageSize.y);
        background.transform.localScale = Vector3.one * scale;
        background.transform.SetPositionAndRotation(
            gameCamera.transform.position + gameCamera.transform.forward * 30f,
            gameCamera.transform.rotation);
    }
}
