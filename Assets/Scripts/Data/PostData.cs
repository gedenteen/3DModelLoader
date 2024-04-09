using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class PostDataInt
{
    public int id;
    public PostDataInt(int id)
    {
        this.id = id;
    }
}

[System.Serializable]
public class PostDataString
{
    public string id;
    public PostDataString(string id)
    {
        this.id = id;
    }
}