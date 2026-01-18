using UnityEngine;

namespace Attic.Mirror.Actors.Interacting
{
    [CreateAssetMenu(fileName = "InteractColorConfig", menuName = "Synergy/Interact/Interact Color Config")]
    public class InteractColorConfig : ScriptableObject
    {
        [SerializeField] private Color defaultColor;
        [SerializeField] private Color alternativeColor;

        public Color GetColor(InteractInput interactInput)
        {
            switch (interactInput)
            {
                case InteractInput.Default:
                    return defaultColor;

                case InteractInput.Alternative:
                    return alternativeColor;

                default:
                    return defaultColor;
            }
        }
    }
}
