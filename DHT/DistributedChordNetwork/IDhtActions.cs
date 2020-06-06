using System.Collections.Generic;

namespace DHT
{
    public interface IDhtActions : IDHTNetworkResponseAction, IDHTNetworkRequestAction
    {
        public void Start();
        void Get(in uint key, NodeDto? successor, Node node);
    }
}