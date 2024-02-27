using Unity.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[RequireComponent(typeof(NavMeshAgent))]
public class MonoMachine : MonoBehaviour
{
    [Header("States")]
    [SerializeField] private StateSO initialState;
    
    [field: SerializeField, Header("References")] public NavMeshAgent Agent { get; private set; }
    [field: SerializeField] public Transform Target { get; private set; }
    [field: SerializeField] public GameObject AttackObject { get; private set; }
    
    [field: SerializeField, Header("Inspector")] public string StateName { get; private set; }

    public IState CurrentState { get; private set; }
    
    private void Start()
    {
        SetState(initialState);
    }

    private void Update()
    {
        CurrentState?.Update();
    }

    private void FixedUpdate()
    {
        CurrentState?.FixedUpdate();
    }

    public void SetState(StateSO stateSo)
    {
        CurrentState?.Exit();
        CurrentState = stateSo.Initialize(this);
        StateName = CurrentState.Name;
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        if (!Agent)
            Agent = GetComponent<NavMeshAgent>();
    }

#endif
}