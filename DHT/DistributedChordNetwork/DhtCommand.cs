namespace DHT.DistributedChordNetwork
{
    public enum DhtCommand
    {
        FIND_SUCCESSOR,
        NOTIFY,
        FOUND_SUCCESSOR,
        STABILIZE,
        STABILIZE_RESPONSE,
        CHECK_PREDECESSOR,
        CHECK_PREDECESSOR_RESPONSE,
        GET,
        GET_RESPONSE,
        PUT,
        PUT_RESPONSE,
        REMOVE_DATA_FROM_EXPIRED_REPLICAS,
        REMOVE_DATA_FROM_EXPIRED_REPLICAS_RESPONSE
    }
}