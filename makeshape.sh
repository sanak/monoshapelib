#!/bin/sh

#
#	Use example programs to create a very simple dataset that
#	should display in ARCView II.
#

mono mshpcreate.exe test polygon
mono mdbfcreate.exe test.dbf -s Description 30 -n TestInt 6 0 -n TestDouble 16 5

mono mshpadd.exe test 0 0 100 0 100 100 0 100 0 0 + 20 20 20 30 30 30 20 20
mono mdbfadd.exe test.dbf "Square with triangle missing" 1.5 2.5

mono mshpadd.exe test 150 150 160 150 180 170 150 150
mono mdbfadd.exe test.dbf "Smaller triangle" 100 1000.25

mono mshpadd.exe test 150 150 160 150 180 170 150 150
mono mdbfadd.exe test.dbf "" "" ""

mono mshpdump.exe test.shp
mono mdbfdump.exe test.dbf
