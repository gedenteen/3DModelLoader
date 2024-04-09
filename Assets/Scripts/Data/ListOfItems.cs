using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Item
{
    public int id;
    public string icon;
    public string name;
}

[System.Serializable]
public class ListOfItems
{
    public List<Item> items = new List<Item>();
}
