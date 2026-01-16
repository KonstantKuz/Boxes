using System.Linq;
using Gameplay.Interactable.BoxesInteraction.Abstract;
using Gameplay.Interactable.BoxesInteraction.State;
using Reflex.Attributes;
using UnityEngine;

namespace Gameplay.Interactable.BoxesInteraction.Components
{
    [RequireComponent(typeof(BoxesStorage))]
    public class BoxesStorageSpreader : MonoBehaviour
    {
        [SerializeField]
        private float spreadPeriod = 2f;

        [SerializeField]
        private float spreadForce = 5f;

        private IBoxesInteractionMediator mediator;
        private BoxesStorage storage;
        private float timer;

        [Inject]
        private void Construct(IBoxesInteractionMediator mediator)
        {
            this.mediator = mediator;
        }

        private void Awake()
        {
            storage = GetComponent<BoxesStorage>();
        }

        private void OnEnable()
        {
            timer = spreadPeriod;
        }

        private void Update()
        {
            timer -= Time.deltaTime;

            if (timer <= 0f && storage.Boxes.Count > 0)
            {
                SpreadRandomBox();
                timer = spreadPeriod;
            }
        }

        private void SpreadRandomBox()
        {
            Box box = storage.Boxes.OrderBy(_ => Random.value).FirstOrDefault();

            if (box == null)
            {
                return;
            }

            float y = mediator.Config.ThrowForce.y;
            float x = mediator.Config.ThrowForce.x;
            float randomAngle = Random.Range(-30, 30);
            Vector3 direction = -(Quaternion.AngleAxis(randomAngle, transform.up) * transform.forward);
            Vector3 throwVelocity = (direction + Vector3.up * y) * x;

            box.StateHolder.WriteState(new BoxSharedState(
                ownerNetId: box.State.OwnerNetId,
                holderNetId: 0,
                lastActionId: box.State.LastActionId + 1,
                lastActionType: BoxActionType.Throw,
                throwVelocity: throwVelocity.normalized * spreadForce
            ));
        }
    }
}
