using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Gameplay.Player
{
    public class LocalComponentChecker : NetworkBehaviour
    {
        [SerializeField]
        private List<Component> destroyComponents;

        [SerializeField]
        private List<GameObject> destroyGameObjects;

        public override void OnStartClient()
        {
            if (!isLocalPlayer)
            {
                foreach (Component localComponent in destroyComponents)
                {
                    Destroy(localComponent);
                }

                foreach (GameObject localComponent in destroyGameObjects)
                {
                    Destroy(localComponent);
                }
            }
        }
    }
}
