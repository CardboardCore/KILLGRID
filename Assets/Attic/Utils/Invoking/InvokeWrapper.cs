using System;
using Attic.DI;
using UnityEngine;

namespace Attic.Utils.Invoking
{
    [Injectable]
    public class InvokeWrapper : MonoBehaviour
    {
        [SerializeField] private InvokeChild invokeChildPrefab;

        public void Invoke(Action action, float delay)
        {
            InvokeChild invokeChild = Instantiate(invokeChildPrefab, transform);
            StartCoroutine(invokeChild.Invoke(action, delay));
        }
    }
}
