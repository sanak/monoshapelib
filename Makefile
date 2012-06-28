CSC = mcs

default:	all

all:	mshapelib.dll mshpcreate.exe mshpadd.exe mshpdump.exe mshprewind.exe \
	mdbfcreate.exe mdbfadd.exe mdbfdump.exe mshptest.exe mshptreedump.exe

mshapelib.dll:		shpopen.cs dbfopen.cs shptree.cs
	$(CSC) $(CSFLAGS) /target:library /out:mshapelib.dll \
	shapefil.cs shpopen.cs dbfopen.cs shptree.cs c.cs

mshpcreate.exe:	shpcreate.cs mshapelib.dll
	$(CSC) $(CSFLAGS) /r:mshapelib.dll /out:mshpcreate.exe shpcreate.cs

mshpadd.exe:	shpadd.cs mshapelib.dll
	$(CSC) $(CSFLAGS) /r:mshapelib.dll /out:mshpadd.exe shpadd.cs

mshpdump.exe:	shpdump.cs mshapelib.dll
	$(CSC) $(CSFLAGS) /r:mshapelib.dll /out:mshpdump.exe shpdump.cs

mshprewind.exe:	shprewind.cs mshapelib.dll
	$(CSC) $(CSFLAGS) /r:mshapelib.dll /out:mshprewind.exe shprewind.cs

mdbfcreate.exe:	dbfcreate.cs mshapelib.dll
	$(CSC) $(CSFLAGS) /r:mshapelib.dll /out:mdbfcreate.exe dbfcreate.cs

mdbfadd.exe:	dbfadd.cs mshapelib.dll
	$(CSC) $(CSFLAGS) /r:mshapelib.dll /out:mdbfadd.exe dbfadd.cs

mdbfdump.exe:	dbfdump.cs mshapelib.dll
	$(CSC) $(CSFLAGS) /r:mshapelib.dll /out:mdbfdump.exe dbfdump.cs

mshptest.exe:	shptest.cs mshapelib.dll
	$(CSC) $(CSFLAGS) /r:mshapelib.dll /out:mshptest.exe shptest.cs

mshputils.exe:	shputils.cs mshapelib.dll
	$(CSC) $(CSFLAGS) /r:mshapelib.dll /out:mshputils.exe shputils.cs

mshptreedump.exe:	shptreedump.cs mshapelib.dll
	$(CSC) $(CSFLAGS) /r:mshapelib.dll /out:mshptreedump.exe shptreedump.cs

clean:
	rm -f *.pdb
	rm -f *.exe
	rm -f *.dll

test:	test2 test3

#
#	Note this stream only works if example data is unzipped.
#	$ unzip ./eg_data/shape_eg_data.zip -d ./eg_data
#
test1:
	@./stream1.sh > s1.out
	@if test "`diff s1.out stream1.out`" = '' ; then \
	    echo "******* Stream 1 Succeeded *********"; \
	    rm s1.out; \
	else \
	    echo "******* Stream 1 Failed *********"; \
	    diff s1.out stream1.out; \
	fi

test2:
	@./stream2.sh > s2.out
	@if test "`diff s2.out stream2.out`" = '' ; then \
	    echo "******* Stream 2 Succeeded *********"; \
	    rm s2.out; \
	    rm test*.s??; \
	else \
	    echo "******* Stream 2 Failed *********"; \
	    diff s2.out stream2.out; \
	fi

test3:
	@./makeshape.sh > s3.out
	@if test "`diff s3.out stream3.out`" = '' ; then \
	    echo "******* Stream 3 Succeeded *********"; \
	    rm s3.out; \
	    rm test.*; \
	else \
	    echo "******* Stream 3 Failed *********"; \
	    diff s3.out stream3.out; \
	fi
