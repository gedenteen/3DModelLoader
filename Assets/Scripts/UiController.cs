using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UiState
{
    Begin,
    ModelView
}

public class UiController : MonoBehaviour
{
    [SerializeField] List<GameObject> objectForStateBegin = new List<GameObject>();
    [SerializeField] List<GameObject> objectForStateModelView = new List<GameObject>();

    private UiState _currentState = UiState.Begin;

    public void SetState(UiState state)
    {
        switch (state)
        {
            case UiState.Begin:
                foreach (GameObject go in objectForStateBegin)
                {
                    go.SetActive(true);
                }
                foreach (GameObject go in objectForStateModelView)
                {
                    go.SetActive(false);
                }
                break;
            case UiState.ModelView:
                foreach (GameObject go in objectForStateBegin)
                {
                    go.SetActive(false);
                }
                foreach (GameObject go in objectForStateModelView)
                {
                    go.SetActive(true);
                }
                break;
            default:
                break;
        }

        _currentState = state;
    }

    public void SetStateBegin()
    {
        SetState(UiState.Begin);
    }

    public void SetStateModelView()
    {
        SetState(UiState.ModelView);
    }
}
