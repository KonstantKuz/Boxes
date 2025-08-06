namespace Infrastructure.Network.Abstract
{
    public static class NetworkStateHolderExtensions
    {
        public static T GetStateOrDefault<T>(this NetworkStateHolderBase holder) where T : INetworkState
        {
            return holder.Data != null ? holder.Serializer.Deserialize<T>(holder.Data) : default;
        }
    }
}