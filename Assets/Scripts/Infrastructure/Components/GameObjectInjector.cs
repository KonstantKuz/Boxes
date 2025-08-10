using System.Collections.Generic;
using Reflex.Attributes;
using Reflex.Core;
using Reflex.Injectors;
using UnityEngine;

namespace Infrastructure.Components
{
    public class GameObjectInjector : MonoBehaviour
    {
        [SerializeField]
        private List<Component> injectables;

        [Inject]
        public void Inject(Container container)
        {
            foreach (Component injectable in injectables)
            {
                AttributeInjector.Inject(injectable, container);
            }
        }
    }
}
