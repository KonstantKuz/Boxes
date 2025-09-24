using Mirror;

namespace Infrastructure.Network
{
    public class HideOnStart : NetworkBehaviour
    {
        public override void OnStartClient()
        {
            gameObject.SetActive(false);
        }
    }
}
