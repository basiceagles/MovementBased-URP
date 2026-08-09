using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class CombineMesh : MonoBehaviour
{
    [SerializeField] private bool combineOnStart = true;
    [SerializeField] private bool deactivateChildren = true;

    private void Start()
    {
        if (combineOnStart)
        {
            CombineChildMeshes();
        }
    }

    public void CombineChildMeshes()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length - 1];

        int index = 0;
        for (int i = 0; i < meshFilters.Length; i++)
        {
            if (meshFilters[i].gameObject == gameObject) continue;

            combine[index].mesh = meshFilters[i].sharedMesh;
            combine[index].transform = transform.worldToLocalMatrix * meshFilters[i].transform.localToWorldMatrix;
            
            if (deactivateChildren)
            {
                meshFilters[i].gameObject.SetActive(false);
            }
            index++;
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.name = gameObject.name + "_CombinedMesh";
        combinedMesh.CombineMeshes(combine);

        MeshFilter myMeshFilter = GetComponent<MeshFilter>();
        myMeshFilter.mesh = combinedMesh;
        gameObject.SetActive(true);
    }
}

