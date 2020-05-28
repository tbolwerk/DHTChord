for i in 0 1 2 3 4 5
do
	if((i == 0))
	then
		dotnet run "0" "localhost" "9000" &
	else
		dotnet run "$i" "localhost" "900$i" "0" "localhost" "9000" &
	fi
done
