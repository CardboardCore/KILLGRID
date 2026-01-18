using KILLGRID.Application.StateMachines;
using UnityEngine;

namespace Run.Application
{
    public class ApplicationManager : MonoBehaviour
    {
        private ApplicationStateMachine applicationStateMachine;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            applicationStateMachine = new ApplicationStateMachine(true);
        }

        private void Start()
        {
            applicationStateMachine.Start();
        }

        private void OnDestroy()
        {
            applicationStateMachine.Stop();
            applicationStateMachine = null;
        }
    }
}
