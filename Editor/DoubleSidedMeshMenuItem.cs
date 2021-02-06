
using UnityEngine;
using UnityEditor;

public static class DoubleSidedMeshMenuItem
{
    [MenuItem("Assets/Create/Double-Sided Mesh")]
    static void MakeDoubleSidedMeshAsset()
    {
        var sourceMesh = Selection.activeObject as Mesh;
        if(sourceMesh == null)
        {
            Debug.Log("You must have a mesh asset selected.");
            return;
        }

        Mesh insideMesh = Object.Instantiate(sourceMesh);
        int[] triangles = insideMesh.triangles;
        //System.Arrary.Reverse(triangles);
        insideMesh.triangles = triangles;

        Object.DestroyImmediate(insideMesh);
        AssetDatabase.CreateAsset(
            insideMesh, "Assets/Meshs/" + sourceMesh.name + ".asset"
        );
    }
}
