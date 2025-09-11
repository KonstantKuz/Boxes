using ObservableCollections;
using UnityEngine;

namespace Gameplay.Interactable.BoxesInteraction.Components
{
    [RequireComponent(typeof(Collider))]
    public class BoxesStorage : MonoBehaviour
    {
        private ObservableHashSet<Box> boxes;

        public ObservableHashSet<Box> Boxes => boxes;

        private void Awake()
        {
            boxes = new ObservableHashSet<Box>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Box box) && !boxes.Contains(box))
            {
                boxes.Add(box);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out Box box) && boxes.Contains(box))
            {
                boxes.Remove(box);
            }
        }
    }
}
