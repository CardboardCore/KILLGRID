using System;
using System.Collections;
using UnityEngine;

namespace Attic.Utils.Invoking
{
    public class InvokeChild : MonoBehaviour
    {
        public IEnumerator Invoke(Action action, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            action?.Invoke();

            Destroy(gameObject);
        }
    }
}
