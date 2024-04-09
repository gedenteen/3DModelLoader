using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TransformData
{
    public Vector3 position;
    public Quaternion rotation;
}

[System.Serializable]
public class MeshData
{
    public List<Vector3> positions;
    public List<Vector3> normals;
    public List<Vector2> uvs;
    public List<int> indices;
}

[System.Serializable]
public class ObjectData
{
    public TransformData transform;
    public MeshData mesh;
    public string material;
}

[System.Serializable]
public class ListOfObjects
{
    public List<ObjectData> objects;
}
