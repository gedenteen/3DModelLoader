using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelsManager : MonoBehaviour
{
    [SerializeField] private float sensitivityRotation = 1f;
    [SerializeField] private float sensitivityMovement = 0.01f;
    [SerializeField] private float minZPos = -1f;
    [SerializeField] private float maxZPos = 2f;

    private Dictionary<int, GameObject> _createdModels = new Dictionary<int, GameObject>();
    private int _activeModelId = -1;
    private float _vertical;
    private float _horizontal;

    private void Awake()
    {
        InputManager.eventMovement.AddListener(MoveModel);
        InputManager.eventRotation.AddListener(RotateModel);
    }

    public void AddModel(int id, GameObject obj)
    {
        _createdModels.Add(id, obj);
        _activeModelId = id;
        _vertical = _horizontal = 0f;
    }

    public bool CheckModelAlreadyCreated(int id)
    {
        return _createdModels.ContainsKey(id);
    }

    public void DeactivateAllModels()
    {
        foreach (var item in _createdModels)
        {
            item.Value.SetActive(false);
        }
    }
    public void ActivateModelById(int id)
    {
        foreach (var item in _createdModels)
        {
            item.Value.SetActive(id == item.Key);
        }
        _activeModelId = id;
        _vertical = _horizontal = 0f;
    }

    private void MoveModel(float movement)
    {
        // Debug.Log("MoveModel:");
        Vector3 newPosition = _createdModels[_activeModelId].transform.position;
        newPosition.z = Mathf.Clamp(newPosition.z + movement * sensitivityMovement, minZPos, maxZPos);
        _createdModels[_activeModelId].transform.position = newPosition;
    }


    private void RotateModel(Vector2 inputRotation)
    {
        // Debug.Log($"RotateModel: inputRotation={inputRotation}");
        if (_activeModelId == -1)
        {
            return;
        }

        float dt = Time.deltaTime;
        _vertical += sensitivityRotation * inputRotation.y * dt;
        _horizontal -= sensitivityRotation * inputRotation.x * dt;

        _createdModels[_activeModelId].transform.eulerAngles = new Vector3(_vertical, _horizontal, 0f);
    }
}
