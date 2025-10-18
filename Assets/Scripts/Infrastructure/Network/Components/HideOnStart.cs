using Mirror;

namespace Infrastructure.Network.Components
{
    public class HideOnStart : NetworkBehaviour
    {
        public override void OnStartClient()
        {
            gameObject.SetActive(false);
        }
    }
}
