::
::	Use example programs to create a very simple dataset that
::	should display in ARCView II.
::

@echo off
mshpcreate.exe test polygon
mdbfcreate.exe test.dbf -s Description 30 -n TestInt 6 0 -n TestDouble 16 5

mshpadd.exe test 0 0 100 0 100 100 0 100 0 0 + 20 20 20 30 30 30 20 20
mdbfadd.exe test.dbf "Square with triangle missing" 1.5 2.5

mshpadd.exe test 150 150 160 150 180 170 150 150
mdbfadd.exe test.dbf "Smaller triangle" 100 1000.25

mshpadd.exe test 150 150 160 150 180 170 150 150
mdbfadd.exe test.dbf "" "" ""

mshpdump.exe test.shp
mdbfdump.exe test.dbf
@echo on