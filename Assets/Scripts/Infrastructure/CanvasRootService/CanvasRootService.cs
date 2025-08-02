using Reflex.Attributes;
using UnityEngine;

namespace Infrastructure.CanvasRootService
{
    public class CanvasRootService : ICanvasRootService
    {
        private CanvasRoot canvasRoot;

        [Inject]
        public void Construct(CanvasRoot canvasRoot)
        {
            this.canvasRoot = canvasRoot;
        }

        void ICanvasRootService.Attach(GameObject gameObject)
        {
            if (gameObject == null)
            {
                throw new System.ArgumentNullException(nameof(gameObject));
            }

            gameObject.transform.SetParent(canvasRoot.transform, false);
        }
    }
}
