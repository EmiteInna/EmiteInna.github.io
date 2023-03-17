using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using static UnityEditor.PlayerSettings;

[CustomEditor(typeof(TreePainterBase))]
public class TreePainterExample : Editor
{
    private GameObject grassObject=null;
    MeshFilter grassFilter=null;
    private Collider[] colliders=new Collider[1005];
    Dictionary<Vector3,int> havVectors;
    List<GameObject> grasses=new List<GameObject>();
    //int operationType = -1;
    //int index = 0;
    //struct PLANTSinfo
    //{
    //    public GameObject objectType;
    //    public Vector3 position;
    //    public Quaternion rotation;
    //}
    //private PLANTSinfo[] lastOperation = new PLANTSinfo[1005];
    private void OnSceneGUI()
    {
        if (TreePainter.Instance == null) return;
        if (TreePainter.Instance.enabled)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            Planting();
            
        }
    }
    public override void OnInspectorGUI()
    {
        //Debug.Log("I am here,not as always");
        base.OnInspectorGUI();
        EditorGUILayout.BeginVertical();
        if (TreePainter.Instance != null)
        {
            float radius = TreePainter.Instance.brushSize;
            float randomness = TreePainter.Instance.scaleRandom;
            float density = TreePainter.Instance.density;
            GUILayout.Label(string.Format("Radius is :{0}", radius));
            GUILayout.Label(string.Format("Randomness is :{0}", randomness));
            GUILayout.Label(string.Format("Density is :{0}", density));
        }
        else
        {
            GUILayout.Label("There's No TreePainter Opening");
        }
        EditorGUILayout.EndVertical();
    }
    void SpawningPlants(Vector3 hit,Vector3 normal)
    {
        float radius = TreePainter.Instance.brushSize;
        float randomness = TreePainter.Instance.scaleRandom;
        float density = TreePainter.Instance.density;
        GameObject tree = TreePainter.Instance.Plants[TreePainter.Instance.PlantSelect];
        if (tree == null) return;
        int collidercount = Physics.OverlapSphereNonAlloc(hit, radius, colliders);
        int treeCount = 0;
        for (int i = 0; i< collidercount; i++){
            if (colliders[i].CompareTag("PLANTS"))
            {
                treeCount++;
            }
        }
        int spawnCount = Mathf.Max(0, (int)density - treeCount);
        //index = 0;
        for (int i = 0; i< spawnCount; i++)
        {
            Vector3 pos = new Vector3(hit.x + Random.Range(-radius, radius)*randomness
                                    , hit.y,
                                    hit.z + Random.Range(-radius, radius) * randomness
                                    );
            Vector3 bp = new Vector3(pos.x,pos.y+100,pos.z);
            Ray ray = new Ray(bp, new Vector3(0, -1, 0));
            RaycastHit hit2;
            if (!Physics.Raycast(ray, out hit2, 100f, LayerMask.GetMask("TERRAIN")))
            {
                continue;
            }
            GameObject g= Instantiate(tree, hit2.point, Quaternion.identity);
            tree.tag = "PLANTS";
            g.transform.localScale = TreePainter.Instance.scaleBase * g.transform.localScale;
            g.transform.LookAt(g.transform.position + hit2.normal);
            //
            //PLANTSinfo pl = new PLANTSinfo();
            //pl.objectType = g;
            //pl.position = pos;
            //pl.rotation = Quaternion.Euler(new Vector3(-90, 0, 0));
            //if (index < 1005) lastOperation[index++] = pl;
            //operationType = 0;
            //undo
        }
    }
    void SpawningGrass(Vector3 hit, Vector3 normal)
    {
        float radius = TreePainter.Instance.brushSize;
        float randomness = TreePainter.Instance.scaleRandom;
        float density = TreePainter.Instance.density;
        GameObject tree = TreePainter.Instance.Plants[TreePainter.Instance.PlantSelect];
        if (tree == null) return;
        int spawnCount = (int)density;
        float dx = (radius / density);
        float dz = dx;
        int lft = (int)Mathf.Sqrt(spawnCount);
        //index = 0;
        for (int i = -lft;i<=lft ; i++)
        {
            for (int j = -lft; j <=lft; j++)
            {
                //Vector3 pos = CountVetexPosition(new Vector3((Mathf.Floor(hit.x / dx)) * dx
                //                        , hit.y,
                //                        (Mathf.Floor(hit.z / dz)) * dz
                //                        ), i * dx, j * dz, normal);
                Vector3 bp = new Vector3(hit.x+i*dx, hit.y + 10, hit.z + j * dz);
                Ray ray = new Ray(bp, new Vector3(0, -1, 0));
                RaycastHit hit2;
                if(!Physics.Raycast(ray,out hit2, 100f, LayerMask.GetMask("TERRAIN"))){
                    continue;
                }
                int collidercount = Physics.OverlapSphereNonAlloc(hit2.point, dx, colliders);
                bool flg = false;
                for(int k=0; k < collidercount; k++)
                {
                    if (colliders[k].gameObject.tag == "PLANTS")
                    {
                        flg = true;
                        break;
                    }
                }
                Debug.Log(flg);
                if (flg == true) continue;
                GameObject g = Instantiate(tree, hit2.point, Quaternion.identity);
                tree.tag = "PLANTS";
                g.transform.LookAt(g.transform.position + hit2.normal);
                grasses.Add(g);
                if(g.transform.GetChild(0)!=null)
                    grasses.Add(g.transform.GetChild(0).gameObject);
            }
        }
    }
    void ErasingPlants(Vector3 hit)
    {
        float radius = TreePainter.Instance.brushSize;
        float randomness = TreePainter.Instance.scaleRandom;
        float density = TreePainter.Instance.density;
        int collidercount = Physics.OverlapSphereNonAlloc(hit, radius, colliders);
        GameObject tree = TreePainter.Instance.Plants[TreePainter.Instance.PlantSelect];
        //index = 0;
        GameObject prefab= PrefabUtility.GetCorrespondingObjectFromSource(tree);
        for (int i = 0; i < collidercount; i++)
        {
            if (colliders[i].CompareTag("PLANTS"))
            {
             //   if (PrefabUtility.GetCorrespondingObjectFromSource(colliders[i].gameObject) == prefab)
             //   {
                    //PLANTSinfo pl = new PLANTSinfo();
                    //pl.objectType = tree;
                    //pl.position = colliders[i].gameObject.transform.position;
                    //pl.rotation = colliders[i].gameObject.transform.rotation;
                    //if (index < 1005) lastOperation[index++] = pl;
                    //operationType = 1;
                    DestroyImmediate(colliders[i].gameObject);
             //   }
            }
        }
    }
    void MergeMesh()
    {
        Debug.Log("Merging Mesh");
        List<Material> materials = new List<Material>();
        List<CombineInstance> combineInstances = new List<CombineInstance>();
        for (int i = 0; i < grasses.Count; i++)
        {
            GameObject g = grasses[i];
            if (g == null) continue;
            MeshFilter filter = g.GetComponent<MeshFilter>();
            if (filter == null || filter.sharedMesh == null) continue;
            EditorUtility.DisplayProgressBar("Mesh Combine", "Combine " + filter.sharedMesh.name, (float)i + 1 / Selection.objects.Length);
            combineInstances.Add(new CombineInstance()
            {
                mesh = filter.sharedMesh,
                transform = filter.transform.localToWorldMatrix
            });
            MeshRenderer renderer = g.GetComponent<MeshRenderer>();
            materials.AddRange(renderer.sharedMaterials);
        }
        EditorUtility.ClearProgressBar();
        GameObject combine = new GameObject("Combined Mesh");
        //if (grasses[0]!=null)
        //    combine.transform.position = grasses[0].transform.position;
        MeshFilter meshFilter = combine.AddComponent<MeshFilter>();
        meshFilter.mesh = new Mesh { name = $"Combined Mesh" };
        meshFilter.sharedMesh.CombineMeshes(combineInstances.ToArray(), true);
        MeshRenderer meshRenderer = combine.AddComponent<MeshRenderer>();
        meshRenderer.materials = materials.ToArray();
    }
    void MergingPlants()
    {
        MergeMesh();
        foreach(GameObject g in grasses)
        {
            DestroyImmediate(g);
        }
        grasses.Clear();
    }
    //void Undo()
    //{
    //    if (TreePainter.Instance.discarding == false) return;
    //    TreePainter.Instance.discarding = false;
    //    if (operationType == 0)
    //    {
    //        for(int i = 0; i < index; i++)
    //        {
    //            DestroyImmediate(lastOperation[i].objectType);
    //        }
    //    }
    //    else
    //    {
    //        for(int i = 0; i < index; i++)
    //        {
    //            PLANTSinfo pl = lastOperation[i];
    //            Instantiate(pl.objectType,pl.position,pl.rotation);
    //        }
    //    }
    //}
    //void SpawningGrassObject(Vector3 point)
    //{
    //    grassObject = new GameObject("GrassObject");
    //    grassFilter = grassObject.AddComponent<MeshFilter>();
    //    grassFilter.mesh = new Mesh{name = $"GrassMesh"};
    //    grassFilter.sharedMesh = grassFilter.mesh;
    //    havVectors = new Dictionary<Vector3, int>();
    //}
    Vector3 CountVetexPosition(Vector3 center, float dx, float dz, Vector3 normal)
    {
        float d = -(dz * normal.z + dx * normal.x) / (normal.y + 0.0001f);
        return new Vector3(center.x + dx, center.y + d, center.z + dz);

    }
    //void DrawingGrass(Vector3 point,Vector3 normal,float grassDensity)
    //{
    //    Vector2 xz = new Vector2(Mathf.Floor(point.x / grassDensity), Mathf.Floor(point.z/grassDensity));
    //    if (havVectors[new Vector3(xz.x, 0, xz.y)] == 1)
    //    {
    //        return;
    //    }
    //    havVectors[new Vector3(xz.x, 0, xz.y)] = 1;

    //}
    void DiscardingGrassObject()
    {
        grassObject=null;
        grassFilter = null;
        havVectors.Clear();
    }
    void Planting()
    {
    //    Undo();
        Event e = Event.current;
        RaycastHit raycastHit = new RaycastHit();
        Ray terrain=HandleUtility.GUIPointToWorldRay(e.mousePosition);
        if (Physics.Raycast(terrain, out raycastHit, Mathf.Infinity,LayerMask.GetMask("TERRAIN")))
        {
            //if (e.type == EventType.MouseDown)
            //{
            //    if (TreePainter.Instance.type == 3) SpawningGrassObject(raycastHit.point);
            //}
            //   Gizmos.DrawWireSphere(raycastHit.point, TreePainter.Instance.brushSize);
            if (e.type == EventType.MouseUp)
            {
                if (TreePainter.Instance.type == 3) MergingPlants();
            }
            if (e.type==EventType.MouseDrag&&e.button==0)
            {
                if(TreePainter.Instance.type==1)SpawningPlants(raycastHit.point,raycastHit.normal);
                if (TreePainter.Instance.type == 2) ErasingPlants(raycastHit.point);
                if (TreePainter.Instance.type == 3) SpawningGrass(raycastHit.point, raycastHit.normal);
            }
        }
    }
}
