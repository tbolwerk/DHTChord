# DistributedHashtableChord
Distributed hash table implementation with chord as overlay network bases on the wikipedia about chord.
https://en.wikipedia.org/wiki/Chord_(peer-to-peer)

Make sure port 9000 --> 9007 are available before running, or change it in appsettings.

# RUN with docker compose
With this step we can see the distributed hash table in action, it spins up 5 docker containers and starts syncing.
## prerequisite
* Use a bash kind of terminal
* chmod +x run.sh
* chmod +x stop.sh

Open terminal and run 5 distributed hashtables using docker by doing the following:

```
cd DHT
sudo ./run.sh
```

To stop these containers we use 

```
sudo ./stop.sh
```

# RUN with `dotnet run`
## prerequisite
* dotnet

Open up a terminal for our bootstrap node.

```
cd DHT
dotnet run localhost 9000
```
then start up our second node

```
cd DHT
dotnet run localhost 9001 0 localhost 9000
```

and even a third

```
cd DHT
dotnet run localhost 9002 0 localhost 9000
```
try the following commands by typing:

```
/put
```
1. enter a uniqueKey followed by enter
2. enter a value followed by enter

In order to test our value is getting distrubted properly use the follwing command on a different node by entering the following command:

```
/get
```
1. enter the uniqueKey

You should get your value, now try this with a new node starting and close the node new terminal.

```
dotnet run localhost 9003 0 localhost 9000
/get
uniqueKey
```
Should return same value even when the node is shutdown.