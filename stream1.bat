@echo off

set EG_DATA=./eg_data

echo -------------------------------------------------------------------------
echo Test 1: dump anno.shp
echo -------------------------------------------------------------------------
powershell -Command "./mshpdump.exe $env:EG_DATA/anno.shp | select -first 250"

echo -------------------------------------------------------------------------
echo Test 2: dump brklinz.shp
echo -------------------------------------------------------------------------
powershell -Command "./mshpdump.exe $env:EG_DATA/brklinz.shp | select -first 500"

echo -------------------------------------------------------------------------
echo Test 3: dump polygon.shp
echo -------------------------------------------------------------------------
powershell -Command "./mshpdump.exe $env:EG_DATA/polygon.shp | select -first 500"

echo -------------------------------------------------------------------------
echo Test 4: dump pline.dbf - uses new F field type
echo -------------------------------------------------------------------------
powershell -Command "./mdbfdump.exe -m -h $env:EG_DATA/pline.dbf | select -first 50"

echo -------------------------------------------------------------------------
echo Test 5: NULL Shapes.
echo -------------------------------------------------------------------------
powershell -Command "./mshpdump.exe $env:EG_DATA/csah.dbf | select -first 150"

@echo on