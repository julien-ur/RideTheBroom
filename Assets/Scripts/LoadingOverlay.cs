using UnityEngine;

// Source: https://github.com/tcdent/unity-vr-overlay

[RequireComponent(typeof(MeshFilter))]
public class LoadingOverlay : MonoBehaviour {
    private bool fading;
    private float fade_timer;

    public float in_alpha = 1.0f;
    public float out_alpha = 0.0f;

    private Color from_color;
    private Color to_color;
    private float _fadingTime;
    private Material material;
    private Renderer _renderer;

    void Start(){
        ReverseNormals(gameObject);
        fading = false;
        fade_timer = 0;

        _renderer = gameObject.GetComponent<Renderer>();
        material = _renderer.material;

        from_color = material.color;
        to_color = material.color;
    }
    
    void Update(){
        if(fading == false)
            return;

        fade_timer += Time.deltaTime;
        //material.SetColor("Tint", Color.Lerp(from_color, to_color, fade_timer));
        material.color = Color.Lerp(from_color, to_color, fade_timer);
        if(material.color == to_color){
            fading = false;
            //_renderer.enabled = false;
            fade_timer = 0;
        }
    }

    public void FadeOut(float time)
    {
        // Fade the overlay to `out_alpha`.
        from_color.a = in_alpha;
        to_color.a = out_alpha;
        _fadingTime = time;
        if (to_color != material.color){
            fading = true;
            //_renderer.enabled = true;
        }
    }

    public void FadeIn(float time){
        // Fade the overlay to `in_alpha`.
        from_color.a = out_alpha;
        to_color.a = in_alpha;
        _fadingTime = time;
        if(to_color != material.color){
            fading = true;
            //_renderer.enabled = true;
        }
    }

    public static void ReverseNormals(GameObject gameObject){
        // Renders interior of the overlay instead of exterior.
        // Included for ease-of-use. 
        // Public so you can use it, too.
        MeshFilter filter = gameObject.GetComponent(typeof(MeshFilter)) as MeshFilter;
        if(filter != null){
            Mesh mesh = filter.mesh;
            Vector3[] normals = mesh.normals;
            for(int i = 0; i < normals.Length; i++)
                normals[i] = -normals[i];
            mesh.normals = normals;

            for(int m = 0; m < mesh.subMeshCount; m++){
                int[] triangles = mesh.GetTriangles(m);
                for(int i = 0; i < triangles.Length; i += 3){
                    int temp = triangles[i + 0];
                    triangles[i + 0] = triangles[i + 1];
                    triangles[i + 1] = temp;
                }
                mesh.SetTriangles(triangles, m);
            }
        }
    }
}
