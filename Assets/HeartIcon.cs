using UnityEngine;
using UnityEngine.UI;

// Draw a heart directly in the UI, without a texture or font dependency.
[RequireComponent(typeof(CanvasRenderer))]
public class HeartIcon : MaskableGraphic
{
    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        Rect rect = GetPixelAdjustedRect();
        const int segments = 48;
        mesh.AddVert(rect.center, color, Vector2.zero);
        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2 / segments;
            float sine = Mathf.Sin(angle);
            float x = 16 * sine * sine * sine;
            float y = 13 * Mathf.Cos(angle) - 5 * Mathf.Cos(2 * angle)
                - 2 * Mathf.Cos(3 * angle) - Mathf.Cos(4 * angle);
            Vector2 position = rect.center + new Vector2(x / 32 * rect.width,
                (y + 2.5f) / 29 * rect.height);
            float height = Mathf.InverseLerp(rect.yMin, rect.yMax, position.y);
            Color shaded = color * Mathf.Lerp(0.55f, 1.15f, height);
            shaded.a = color.a;
            mesh.AddVert(position, shaded, Vector2.zero);
        }
        for (int i = 0; i < segments; i++)
        {
            mesh.AddTriangle(0, i + 1, (i + 1) % segments + 1);
        }
    }
}
