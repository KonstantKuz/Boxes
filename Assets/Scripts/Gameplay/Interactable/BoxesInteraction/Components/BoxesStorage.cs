using ObservableCollections;
using UnityEngine;

namespace Gameplay.Interactable.BoxesInteraction.Components
{
    [RequireComponent(typeof(Collider))]
    public class BoxesStorage : MonoBehaviour
    {
        [SerializeField]
        private Collider platformCollider;

        private readonly ObservableHashSet<Box> boxes = new();
        private new Rigidbody rigidbody;

        private Rigidbody Rigidbody => rigidbody ??= GetComponent<Rigidbody>();
        public ObservableHashSet<Box> Boxes => boxes;

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Box box) && !boxes.Contains(box))
            {
                if (box.TryGetComponent(out AttachableObject attachableObject) && attachableObject.IsAttached)
                {
                    return;
                }

                Vector3 anchorPoint = box.transform.position;

                if (platformCollider != null)
                {
                    anchorPoint = platformCollider.ClosestPoint(box.transform.position);
                }

                box.Attach(Rigidbody, anchorPoint);
                boxes.Add(box);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out Box box) && boxes.Contains(box))
            {
                box.Attach(null);
                boxes.Remove(box);
            }
        }
    }
}
