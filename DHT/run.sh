#!/usr/bin/env bash
for i in 0 1 2 3 4 5
do
	if((i == 0))
	then
		dotnet run "127.0.0.1" "9000" &
	else
		dotnet run "127.0.0.1" "900$i" "0" "127.0.0.1" "9000" &
	fi
done
