namespace DHT
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
        STABILIZE_REPLICAS_JOIN,
        STABILIZE_REPLICAS_JOIN_RESPONSE,
        STABILIZE_REPLICAS_LEAVE
    }
}