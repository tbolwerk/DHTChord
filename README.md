# DistributedHashtableChord
Distributed hash table implementation with chord as overlay network bases on the wikipedia about chord

Open terminal
> cd DHT
> cd ./run.sh

Open new terminal
> dotnet run 6 localhost 9006 0 localhost 9000
> /put
> uniqueKey
> value
> /get
> uniqueKey

You should get your value, now try this with a new node starting and close the node new terminal

> dotnet run 7 localhost 9007 0 localhost 9000
> /get
> uniqueKey

Should return same value even when the node is shutdown.
