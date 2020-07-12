# DistributedHashtableChord
Distributed hash table implementation with chord as overlay network bases on the wikipedia about chord.
https://en.wikipedia.org/wiki/Chord_(peer-to-peer)

Make sure port 9000 --> 9007 are available before running, or change it in appsettings.

# RUN Without docker
## prereqesite
* Use a bash kind of terminal

Open terminal
> cd DHT
>
> sudo ./run.sh

# RUN with docker
## prereqesite
* Docker
* Docker compose
> docker-compose up --build

Open new terminal
> dotnet run 6 localhost 9006 0 localhost 9000
>
> /put
>
> uniqueKey
>
> value
>
> /get
>
> uniqueKey

You should get your value, now try this with a new node starting and close the node new terminal

> dotnet run 7 localhost 9007 0 localhost 9000
> /get
>
> uniqueKey

Should return same value even when the node is shutdown.
