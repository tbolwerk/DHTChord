for i in 0 1 2 3 4 5
do
	lsof -ti:900$i -sTCP:LISTEN | xargs kill
done
