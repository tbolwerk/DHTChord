using RelayService.DataAccessService.RoutingDataAccess.DHT.DistributedChordNetwork;

namespace DHT.DistributedChordNetwork
{
    public interface IDhtActions : IDhtNetworkResponseAction, IDhtNetworkRequestAction
    {
        public void Start();
    }
}