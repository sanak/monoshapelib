/******************************************************************************
 * shputils.cs
 *
 * Project:  MonoShapelib
 * Purpose:  
 *   Altered "shpdump" and "dbfdump" to allow two files to be appended.
 *   Other Functions:
 *     Selecting from the DBF before the write occurs.
 *     Change the UNITS between Feet and Meters and Shift X,Y.
 *     Clip and Erase boundary.  The program only passes thru the
 *     data once.
 *
 *   Bill Miller   North Carolina - Department of Transporation 
 *   Feb. 1997 -- bmiller@dot.state.nc.us
 *         There was not a lot of time to debug hidden problems;
 *         And the code is not very well organized or documented.
 *         The clip/erase function was not well tested.
 *   Oct. 2000 -- bmiller@dot.state.nc.us
 *         Fixed the problem when select is using numbers
 *         larger than short integer.  It now reads long integer.
 *   NOTE: DBF files created using windows NT will read as a string with
 *         a length of 381 characters.  This is a bug in "dbfopen".
 *
 *
 * Author:   Ko Nagase (geosanak@gmail.com)
 *
 ******************************************************************************
 * Copyright (c) 1999, Frank Warmerdam
 *
 * This software is available under the following "MIT Style" license,
 * or at the option of the licensee under the LGPL (see LICENSE.LGPL).  This
 * option is discussed in more detail in shapelib.html.
 *
 * --
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a
 * copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included
 * in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
 * OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 ******************************************************************************
 */

namespace MonoShapelib
{
    class MainClass
    {
        static string   infile, outfile, temp;

        /* Variables for shape files */
        static SHPHandle    hSHP;
        static SHPHandle    hSHPappend;
        static SHPT         nShapeType;    
        static int          nEntities, iPart;
        static SHPT         nShapeTypeAppend;
        static int          nEntitiesAppend;
        static SHPObject    psCShape;
        static double[]     adfBoundsMin = new double[4], adfBoundsMax = new double[4];


        /* Variables for DBF files */
        static DBFHandle    hDBF;
        static DBFHandle    hDBFappend;
            
        static FT   iType;
        static FT   jType;
            
        static string   iszTitle;
        static string   jszTitle;

        static int[]    pt;
        static string   iszFormat, iszField;
        static string   jszFormat, jszField;
        static int      i, ti, iWidth, iDecimals, iRecord;
        static int      j, tj, jWidth, jDecimals, jRecord;


        /* -------------------------------------------------------------------- */
        /* Variables for the DESCRIBE function */
        /* -------------------------------------------------------------------- */
        static bool     ilist = false, iall = false;
        /* -------------------------------------------------------------------- */
        /* Variables for the SELECT function */
        /* -------------------------------------------------------------------- */
        static bool     found = false, newdbf = false;
        static string   selectitem, cpt;
        static long[]   selectvalues = new long[150];
        static long     selcount=0;
        static bool     iselect = false;
        static int      iselectitem = -1;
        static bool     iunselect = false;

        /* -------------------------------------------------------------------- */
        /* Variables for the CLIP and ERASE functions */
        /* -------------------------------------------------------------------- */
        static double   cxmin, cymin, cxmax, cymax; 
        static bool     iclip  = false, ierase = false;
        static bool     itouch = false, iinside = false, icut = false;
        static bool     ibound = false, ipoly = false;
        static string   clipfile;

        /* -------------------------------------------------------------------- */
        /* Variables for the FACTOR function */
        /* -------------------------------------------------------------------- */
        static double   infactor,outfactor,factor = 0;  /* NO FACTOR */
        static bool     iunit = false;
        static bool     ifactor = false;

           
        /* -------------------------------------------------------------------- */
        /* Variables for the SHIFT function */
        /* -------------------------------------------------------------------- */
        static double   xshift = 0, yshift = 0;  /* NO SHIFT */

        public static int Main( string[] args )
        {
            int     nCount_;
            SHPT    nShapeType_;
            /* -------------------------------------------------------------------- */
            /*      Check command line usage.                                       */
            /* -------------------------------------------------------------------- */
            if( args.Length < 1 ) error();
            c.strcpy(ref infile, args[0]);
            if (args.Length > 1) {
                c.strcpy(ref outfile,args[1]);
                if (strncasecmp2(outfile, "LIST",0) == 0) { ilist = true; }
                if (strncasecmp2(outfile, "ALL",0) == 0)  { iall  = true; }
            }
            if (ilist || iall || args.Length == 1 ) {
                setext(ref infile, "shp");
                c.printf("DESCRIBE: {0}\n",infile);
                c.strcpy(ref outfile,"");
            }
            /* -------------------------------------------------------------------- */
            /*      Look for other functions on the command line. (SELECT, UNIT)    */
            /* -------------------------------------------------------------------- */
            for (i = 2; i < args.Length; i++)
            {
                if ((strncasecmp2(args[i],  "SEL",3) == 0) ||
                    (strncasecmp2(args[i],  "UNSEL",5) == 0))
                {
                    if (strncasecmp2(args[i],  "UNSEL",5) == 0) iunselect=true;
                    i++;
                    if (i >= args.Length) error();
                    c.strcpy(ref selectitem,args[i]);
                    i++;
                    if (i >= args.Length) error();
                    selcount=0;
                    c.strcpy(ref temp,args[i]);
                    cpt=temp;
                    tj = c.atoi(cpt);
                    ti = 0;
                    int pos = 0;
                    while (tj>0) {
                        selectvalues[selcount] = tj;
                        while( cpt[pos] >= '0' && cpt[pos] <= '9')
                            pos++; 
                        while( cpt[pos] > '\0' && (cpt[pos] < '0' || cpt[pos] > '9') )
                            pos++; 
                        tj=c.atoi(cpt.Substring(pos));
                        selcount++;
                    }
                    iselect=true;
                }  /*** End SEL & UNSEL ***/
                else if ((strncasecmp2(args[i], "CLIP",4) == 0) ||
                        (strncasecmp2(args[i],  "ERASE",5) == 0))
                {
                    if (strncasecmp2(args[i],  "ERASE",5) == 0) ierase=true;
                    i++;
                    if (i >= args.Length) error();
                    c.strcpy(ref clipfile,args[i]);
                    c.sscanf(args[i],"{0:F}",ref cxmin);
                    i++;
                    if (i >= args.Length) error();
                    if (strncasecmp2(args[i],  "BOUND",5) == 0) {
                        setext(ref clipfile, "shp");
                        hSHP = SHPHandle.Open( clipfile, "rb" );
                        if( hSHP == null )
                        {
                            c.printf( "ERROR: Unable to open the clip shape file:{0}\n", clipfile );
                            c.exit( 1 );
                        }

                        hSHPappend.GetInfo( out nCount_, out nShapeType_,
                                    adfBoundsMin, adfBoundsMax );
                        cxmin = adfBoundsMin[0];
                        cymin = adfBoundsMin[1];
                        cxmax = adfBoundsMax[0];
                        cymax = adfBoundsMax[1];
                        c.printf("Theme Clip Boundary: ({0:F},{1:F}) - ({2:F},{3:F})\n",
                               cxmin, cymin, cxmax, cymax);
                        ibound=true;
                    } else {  /*** xmin,ymin,xmax,ymax ***/
                        c.sscanf(args[i],"{0:F}",ref cymin);
                        i++;
                        if (i >= args.Length) error();
                        c.sscanf(args[i],"{0:F}",ref cxmax);
                        i++;
                        if (i >= args.Length) error();
                        c.sscanf(args[i],"{0:F}",ref cymax);
                        c.printf("Clip Box: ({0:F},{1:F}) - ({2:F},{3:F})\n",cxmin, cymin, cxmax, cymax);
                    }
                    i++;
                    if (i >= args.Length) error();
                    if      (strncasecmp2(args[i], "CUT",3) == 0)    icut=true;
                    else if (strncasecmp2(args[i], "TOUCH",5) == 0)  itouch=true;
                    else if (strncasecmp2(args[i], "INSIDE",6) == 0) iinside=true;
                    else error();
                    iclip=true;
                } /*** End CLIP & ERASE ***/
                else if (strncasecmp2(args[i],  "FACTOR",0) == 0)
                {
                    i++;
                    if (i >= args.Length) error();
                    infactor=findunit(args[i]);
                    if (infactor == 0) error();
                    iunit=true;
                    i++;
                    if (i >= args.Length) error();
                    outfactor=findunit(args[i]);
                    if (outfactor == 0)
                    {
                        c.sscanf(args[i],"{0:F}",ref factor);
                        if (factor == 0) error();
                    }
                    if (factor == 0)
                    {
                        if (infactor ==0)
                        { c.puts("ERROR: Input unit must be defined before output unit"); c.exit(1); }
                        factor=infactor/outfactor;
                    }
                    c.printf("Output file coordinate values will be factored by {0:G}\n",factor);
                    ifactor=(factor != 1); /* True if a valid factor */
                } /*** End FACTOR ***/
                else if (strncasecmp2(args[i],"SHIFT",5) == 0)
                {
                    i++;
                    if (i >= args.Length) error();
                    c.sscanf(args[i],"{0:F}",ref xshift);
                    i++;
                    if (i >= args.Length) error();
                    c.sscanf(args[i],"{0:F}",ref yshift);
                    iunit=true;
                    c.printf("X Shift: {0:G}   Y Shift: {1:G}\n",xshift,yshift);
                } /*** End SHIFT ***/
                else {
                    c.printf("ERROR: Unknown function {0}\n",args[i]);  error();
                }
            }
            /* -------------------------------------------------------------------- */
            /*      If there is no data in this file let the user know.             */
            /* -------------------------------------------------------------------- */
            openfiles();  /* Open the infile and the outfile for shape and dbf. */
            if( hDBF.GetFieldCount() == 0 )
            {
                c.puts( "There are no fields in this table!" );
                c.exit( 1 );
            }
            /* -------------------------------------------------------------------- */
            /*      Print out the file bounds.                                      */
            /* -------------------------------------------------------------------- */
            iRecord = hDBF.GetRecordCount();
            hSHP.GetInfo( out nCount_, out nShapeType_, adfBoundsMin, adfBoundsMax );

            c.printf( "Input Bounds:  ({0:G},{1:G}) - ({2:G},{3:G})   Entities: {4}   DBF: {5}\n",
                    adfBoundsMin[0], adfBoundsMin[1],
                    adfBoundsMax[0], adfBoundsMax[1],
                    nEntities, iRecord );

            if (c.strcmp(outfile,"") == 0) /* Describe the shapefile; No other functions */
            {
                ti = hDBF.GetFieldCount();
                showitems();
                c.exit(0);
            }

            if (iclip) check_theme_bnd();
            
            jRecord = hDBFappend.GetRecordCount();
            hSHPappend.GetInfo( out nCount_, out nShapeType_, adfBoundsMin, adfBoundsMax );
            if (nEntitiesAppend == 0)
                c.puts("New Output File\n");
            else
                c.printf( "Append Bounds: ({0:G},{1:G})-({2:G},{3:G})   Entities: {4}  DBF: {5}\n",
                        adfBoundsMin[0], adfBoundsMin[1],
                        adfBoundsMax[0], adfBoundsMax[1],
                        nEntitiesAppend, jRecord );
            
            /* -------------------------------------------------------------------- */
            /*      Find matching fields in the append file or add new items.       */
            /* -------------------------------------------------------------------- */
            mergefields();
            /* -------------------------------------------------------------------- */
            /*      Find selection field if needed.                                 */
            /* -------------------------------------------------------------------- */
            if (iselect)    findselect();

            /* -------------------------------------------------------------------- */
            /*      Read all the records                                            */
            /* -------------------------------------------------------------------- */
            jRecord = hDBFappend.GetRecordCount();
            for( iRecord = 0; iRecord < nEntities; iRecord++)  /** DBFGetRecordCount(hDBF) **/
            {
                /* -------------------------------------------------------------------- */
                /*      SELECT for values if needed. (Can the record be skipped.)       */
                /* -------------------------------------------------------------------- */
                if (iselect)
                    if (selectrec() == 0) goto SKIP_RECORD;   /** SKIP RECORD **/

                /* -------------------------------------------------------------------- */
                /*      Read a Shape record                                             */
                /* -------------------------------------------------------------------- */
                psCShape = hSHP.ReadObject( iRecord );

                /* -------------------------------------------------------------------- */
                /*      Clip coordinates of shapes if needed.                           */
                /* -------------------------------------------------------------------- */
                if (iclip)
                    if (clip_boundary() == 0) goto SKIP_RECORD; /** SKIP RECORD **/

                /* -------------------------------------------------------------------- */
                /*      Read a DBF record and copy each field.                          */
                /* -------------------------------------------------------------------- */
                for( i = 0; i < hDBF.GetFieldCount(); i++ )
                {
                    /* -------------------------------------------------------------------- */
                    /*      Store the record according to the type and formatting           */
                    /*      information implicit in the DBF field description.              */
                    /* -------------------------------------------------------------------- */
                    if (pt[i] > -1)  /* if the current field exists in output file */
                    {
                        string sFieldName;
                        switch( hDBF.GetFieldInfo( i, out sFieldName, out iWidth, out iDecimals ) )
                        {
                        case FT.String:
                            hDBFappend.WriteStringAttribute(jRecord, pt[i],
                                                    (hDBF.ReadStringAttribute( iRecord, i )) );
                            break;

                        case FT.Integer:
                            hDBFappend.WriteIntegerAttribute(jRecord, pt[i],
                                                     (hDBF.ReadIntegerAttribute( iRecord, i )) );
                            break;

                        case FT.Double:
                            hDBFappend.WriteDoubleAttribute(jRecord, pt[i],
                                                    (hDBF.ReadDoubleAttribute( iRecord, i )) );
                            break;
                        }
                    }
                }
                jRecord++;
                /* -------------------------------------------------------------------- */
                /*      Change FACTOR and SHIFT coordinates of shapes if needed.        */
                /* -------------------------------------------------------------------- */
                if (iunit)
                {
                    for( j = 0; j < psCShape.nVertices; j++ ) 
                    {
                        psCShape.padfX[j] = psCShape.padfX[j] * factor + xshift;
                        psCShape.padfY[j] = psCShape.padfY[j] * factor + yshift;
                    }
                }
                
                /* -------------------------------------------------------------------- */
                /*      Write the Shape record after recomputing current extents.       */
                /* -------------------------------------------------------------------- */
                psCShape.ComputeExtents();
                hSHPappend.WriteObject( -1, psCShape );

              SKIP_RECORD:
                psCShape = null;
                j=0;
            }

            /* -------------------------------------------------------------------- */
            /*      Print out the # of Entities and the file bounds.                */
            /* -------------------------------------------------------------------- */
            jRecord = hDBFappend.GetRecordCount();
            hSHPappend.GetInfo( out nEntitiesAppend, out nShapeTypeAppend,
                        adfBoundsMin, adfBoundsMax );
            
            c.printf( "Output Bounds: ({0:G},{1:G}) - ({2:G},{3:G})   Entities: {4}  DBF: {5}\n\n",
                    adfBoundsMin[0], adfBoundsMin[1],
                    adfBoundsMax[0], adfBoundsMax[1],
                    nEntitiesAppend, jRecord );

            /* -------------------------------------------------------------------- */
            /*      Close the both shapefiles.                                      */
            /* -------------------------------------------------------------------- */
            hSHP.Close();
            hSHPappend.Close();
            hDBF.Close();
            hDBFappend.Close();
            if (nEntitiesAppend == 0) {
                c.puts("Remove the output files.");
                setext(ref outfile, "dbf");
                c.remove(outfile);
                setext(ref outfile, "shp");
                c.remove(outfile);
                setext(ref outfile, "shx");
                c.remove(outfile);
            }
            return( 0 );
        }


        /************************************************************************/
        /*                             openfiles()                              */
        /************************************************************************/
        static void openfiles() {
            /* -------------------------------------------------------------------- */
            /*      Open the DBF file.                                              */
            /* -------------------------------------------------------------------- */
            setext(ref infile, "dbf");
            hDBF = DBFHandle.Open( infile, "rb" );
            if( hDBF == null )
            {
                c.printf( "ERROR: Unable to open the input DBF:{0}\n", infile );
                c.exit( 1 );
            }
            /* -------------------------------------------------------------------- */
            /*      Open the append DBF file.                                       */
            /* -------------------------------------------------------------------- */
            if (c.strcmp(outfile,"") != 0) {
                setext(ref outfile, "dbf");
                hDBFappend = DBFHandle.Open( outfile, "rb+" );
                newdbf=false;
                if( hDBFappend == null )
                {
                    newdbf=true;
                    hDBFappend = DBFHandle.Create( outfile );
                    if( hDBFappend == null )
                    {
                        c.printf( "ERROR: Unable to open the append DBF:%s\n", outfile );
                        c.exit( 1 );
                    }
                }
            }
            /* -------------------------------------------------------------------- */
            /*      Open the passed shapefile.                                      */
            /* -------------------------------------------------------------------- */
            setext(ref infile, "shp");
            hSHP = SHPHandle.Open( infile, "rb" );

            if( hSHP == null )
            {
                c.printf( "ERROR: Unable to open the input shape file:{0}\n", infile );
                c.exit( 1 );
            }

            hSHP.GetInfo( out nEntities, out nShapeType, null, null );

            /* -------------------------------------------------------------------- */
            /*      Open the passed append shapefile.                               */
            /* -------------------------------------------------------------------- */
            if (c.strcmp(outfile,"") != 0) {
                setext(ref outfile, "shp");
                hSHPappend = SHPHandle.Open( outfile, "rb+" );

                if( hSHPappend == null )
                {
                    hSHPappend = SHPHandle.Create( outfile, nShapeType );
                    if( hSHPappend == null )
                    {
                        c.printf( "ERROR: Unable to open the append shape file:{0}\n",
                                outfile );
                        c.exit( 1 );
                    }
                }
                hSHPappend.GetInfo( out nEntitiesAppend, out nShapeTypeAppend,
                            null, null );

                if (nShapeType != nShapeTypeAppend) 
                {
                    c.puts( "ERROR: Input and Append shape files are of different types.");
                    c.exit( 1 );
                }
            }
        }

        /* -------------------------------------------------------------------- */
        /*      Change the extension.  If there is any extension on the         */
        /*      filename, strip it off and add the new extension                */
        /* -------------------------------------------------------------------- */
        static void setext(ref string pt, string ext)
        {
            int i;
            for( i = c.strlen(pt)-1; 
                 i > 0 && pt[i] != '.' && pt[i] != '/' && pt[i] != '\\';
                 i-- ) {}

            if( pt[i] == '.' )
                pt = pt.Substring(0, i);
                
            c.strcat(ref pt,".");
            c.strcat(ref pt,ext);
        }



        /* -------------------------------------------------------------------- */
        /*      Find matching fields in the append file.                        */
        /*      Output file must have zero records to add any new fields.       */
        /* -------------------------------------------------------------------- */
        static void mergefields()
        {
            int i,j;
            ti = hDBF.GetFieldCount();
            tj = hDBFappend.GetFieldCount();
            /* Create a pointer array for the max # of fields in the output file */
            pt = new int[ti+tj+1]; 
            
            for( i = 0; i < ti; i++ )
            {
                pt[i]= -1;  /* Initial pt values to -1 */
            }
            /* DBF must be empty before adding items */
            jRecord = hDBFappend.GetRecordCount();
            for( i = 0; i < ti; i++ )
            {
                iType = hDBF.GetFieldInfo( i, out iszTitle, out iWidth, out iDecimals );
                found=false;
                {
                    for( j = 0; j < tj; j++ )   /* Search all field names for a match */
                    {
                        jType = hDBFappend.GetFieldInfo( j, out jszTitle, out jWidth, out jDecimals );
                        if (iType == jType && (c.strcmp(iszTitle, jszTitle) == 0) )
                        {
                            if (found || newdbf)
                            {
                                if (i == j)  pt[i]=j;
                                c.printf("Warning: Duplicate field name found ({0})\n",iszTitle);
                                /* Duplicate field name
                                   (Try to guess the correct field by position) */
                            }
                            else
                            {
                                pt[i]=j;  found=true; 
                            }
                        }
                    }
                }

                if (pt[i] == -1  && (! found) )  /* Try to force into an existing field */
                {                                /* Ignore the field name, width, and decimal places */
                    jType = hDBFappend.GetFieldInfo( j, out jszTitle, out jWidth, out jDecimals );
                    if (iType == jType) 
                    {
                        pt[i]=i;  found=true;
                    }
                }
                if ( (! found) &&  jRecord == 0)  /* Add missing field to the append table */
                {                 /* The output DBF must be is empty */
                    pt[i]=tj;
                    tj++;
                    if( hDBFappend.AddField( iszTitle, iType, iWidth, iDecimals )
                            == -1 )
                    {
                        c.printf( "Warning: DBFAddField({0}, TYPE:{1}, WIDTH:{2}  DEC:{3}, ITEM#:{4} of {5}) failed.\n",
                                 iszTitle, iType, iWidth, iDecimals, (i+1), (ti+1) );
                        pt[i]=-1;
                    }
                }
            }
        }


        static void findselect()
        {
            /* Find the select field name */
            iselectitem = -1;
            for( i = 0; i < ti  &&  iselectitem < 0; i++ )
            {
                iType = hDBF.GetFieldInfo( i, out iszTitle, out iWidth, out iDecimals );
                if (strncasecmp2(iszTitle, selectitem, 0) == 0) iselectitem = i;
            }
            if (iselectitem == -1) 
            {
                c.printf("Warning: Item not found for selection ({0})\n",selectitem);
                iselect = false;
                iall = false;
                showitems();
                c.printf("Continued... (Selecting entire file)\n");
            }
            /* Extract all of the select values (by field type) */
            
        }

        static void showitems()
        {
            string  stmp=null,slow=null,shigh=null;
            double  dtmp,dlow,dhigh,dsum,mean;
            long    itmp,ilow,ihigh,isum;
            long    maxrec;
            //string  pt;

            c.printf("Available Items: ({0})",ti);
            maxrec = hDBF.GetRecordCount();
            if (maxrec > 5000 && ! iall) {
                maxrec=5000; c.printf("  ** ESTIMATED RANGES (MEAN)  For more records use \"All\"");
            }
            else {
                c.printf("          RANGES (MEAN)");
            }
              
            for( i = 0; i < ti; i++ )
            {
                switch( hDBF.GetFieldInfo( i, out iszTitle, out iWidth, out iDecimals ) )
                {
                case FT.String:
                    c.strcpy(ref slow, "~");
                    c.strcpy(ref shigh,"\0");
                    c.printf("\n  String  {0,3}  {1,-16}",iWidth,iszTitle);
                    for( iRecord = 0; iRecord < maxrec; iRecord++ ) {
                        c.strncpy(out stmp,hDBF.ReadStringAttribute( iRecord, i ),39);
                        if (c.strcmp(stmp,"!!") > 0) {
                          if (strncasecmp2(stmp,slow,0)  < 0) c.strncpy(out slow, stmp,39);
                          if (strncasecmp2(stmp,shigh,0) > 0) c.strncpy(out shigh,stmp,39);
                        }
                    }
                    slow = slow.TrimEnd();
                    shigh = shigh.TrimEnd();
                    if (strncasecmp2(slow,shigh,0) < 0)         c.printf("{0} to {1}",slow,shigh);
                    else if (strncasecmp2(slow,shigh,0) == 0)   c.printf("= {0}",slow);
                    else                                        c.printf("No Values");
                    break;
                case FT.Integer:
                    c.printf("\n  Integer {0,3}  {1,-16}",iWidth,iszTitle);
                    ilow =  1999999999;
                    ihigh= -1999999999;
                    isum =  0;
                    for( iRecord = 0; iRecord < maxrec; iRecord++ ) {
                        itmp = hDBF.ReadIntegerAttribute( iRecord, i );
                        if (ilow > itmp)  ilow = itmp;
                        if (ihigh < itmp) ihigh = itmp;
                        isum = isum + itmp;
                    }
                    mean=isum/maxrec;
                    if (ilow < ihigh)       c.printf("%d to %d \t(%.1f)",ilow,ihigh,mean);
                    else if (ilow == ihigh) c.printf("= %d",ilow);
                    else                    c.printf("No Values");
                    break;

                case FT.Double:
                    c.printf("\n  Real  {0,3},{1}  {2,-16}",iWidth,iDecimals,iszTitle);
                    dlow =  999999999999999.0;
                    dhigh= -999999999999999.0;
                    dsum =  0;
                    for( iRecord = 0; iRecord < maxrec; iRecord++ ) {
                        dtmp = hDBF.ReadDoubleAttribute( iRecord, i );
                        if (dlow > dtmp) dlow = dtmp;
                        if (dhigh < dtmp) dhigh = dtmp;
                        dsum = dsum + dtmp;
                    }
                    mean=dsum/maxrec;
                    c.sprintf(ref stmp,"%%.%df to %%.%df \t(%%.%df)",iDecimals,iDecimals,iDecimals);
                    if (dlow < dhigh)       c.printf(stmp,dlow,dhigh,mean);
                    else if (dlow == dhigh) {
                        c.sprintf(ref stmp,"= %%.%df",iDecimals);
                        c.printf(stmp,dlow);
                    }
                    else c.printf("No Values");
                    break;

                }

            }
            c.printf("\n");
        }

        static int selectrec()
        {
            FT      ty;
            long    value;
            string  sFieldName;

            ty = hDBF.GetFieldInfo( iselectitem, out sFieldName, out iWidth, out iDecimals);
            switch(ty)
            {
            case FT.String:
                c.puts("Invalid Item");
                iselect=false;
                break;
            case FT.Integer:
                value = hDBF.ReadIntegerAttribute( iRecord, iselectitem );
                for (j = 0; j<selcount; j++)
                {
                    if (selectvalues[j] == value)
                    if (iunselect)  return(0);  /* Keep this record */
                    else            return(1);  /* Skip this record */
                }
                break;
            case FT.Double:
                c.puts("Invalid Item");
                iselect=false;
                break;
            }
            if (iunselect)  return(1);  /* Skip this record */
            else            return(0);  /* Keep this record */
        }


        static void check_theme_bnd()
        {
            if ( (adfBoundsMin[0] >= cxmin) && (adfBoundsMax[0] <= cxmax) &&
                 (adfBoundsMin[1] >= cymin) && (adfBoundsMax[1] <= cymax) )
            {   /** Theme is totally inside clip area **/
                if (ierase) nEntities=0; /** SKIP THEME  **/
                else        iclip=false; /** WRITE THEME (Clip not needed) **/
            }
                    
            if ( ( (adfBoundsMin[0] < cxmin) && (adfBoundsMax[0] < cxmin) ) ||
                 ( (adfBoundsMin[1] < cymin) && (adfBoundsMax[1] < cymin) ) ||
                 ( (adfBoundsMin[0] > cxmax) && (adfBoundsMax[0] > cxmax) ) ||
                 ( (adfBoundsMin[1] > cymax) && (adfBoundsMax[1] > cymax) ) )
            {   /** Theme is totally outside clip area **/
                if (ierase) iclip=false; /** WRITE THEME (Clip not needed) **/
                else        nEntities=0; /** SKIP THEME  **/
            }
                    
            if (nEntities == 0)
                c.puts("WARNING: Theme is outside the clip area."); /** SKIP THEME  **/
        }

        static int clip_boundary()
        {
            bool inside;
            bool prev_outside;
            int  i2;
            int  j2;
            
            /*** FIRST check the boundary of the feature ***/
            if ( ( (psCShape.dfXMin < cxmin) && (psCShape.dfXMax < cxmin) ) ||
                ( (psCShape.dfYMin < cymin) && (psCShape.dfYMax < cymin) ) ||
                ( (psCShape.dfXMin > cxmax) && (psCShape.dfXMax > cxmax) ) ||
                ( (psCShape.dfYMin > cymax) && (psCShape.dfYMax > cymax) ) )
            {   /** Feature is totally outside clip area **/
                if (ierase) return(1); /** WRITE RECORD **/
                else        return(0); /** SKIP  RECORD **/
            }

            if ( (psCShape.dfXMin >= cxmin) && (psCShape.dfXMax <= cxmax) &&
                (psCShape.dfYMin >= cymin) && (psCShape.dfYMax <= cymax) )
            {   /** Feature is totally inside clip area **/
                if (ierase) return(0); /** SKIP  RECORD **/
                else        return(1); /** WRITE RECORD **/
            }
                
            if (iinside) 
            { /** INSIDE * Feature might touch the boundary or could be outside **/
                if (ierase) return(1); /** WRITE RECORD **/
                else        return(0); /** SKIP  RECORD **/
            }

            if (itouch)
            {   /** TOUCH **/
                if ( ( (psCShape.dfXMin <= cxmin) || (psCShape.dfXMax >= cxmax) ) && 
                     (psCShape.dfYMin >= cymin) && (psCShape.dfYMax <= cymax)    )
                {   /** Feature intersects the clip boundary only on the X axis **/
                    if (ierase) return(0); /** SKIP  RECORD **/
                    else        return(1); /** WRITE RECORD **/
                }

                if (   (psCShape.dfXMin >= cxmin) && (psCShape.dfXMax <= cxmax)   && 
                   ( (psCShape.dfYMin <= cymin) || (psCShape.dfYMax >= cymax) )  )
                {   /** Feature intersects the clip boundary only on the Y axis **/
                    if (ierase) return(0); /** SKIP  RECORD **/
                    else        return(1); /** WRITE RECORD **/
                }
                   
                for( j2 = 0; j2 < psCShape.nVertices; j2++ ) 
                {   /** At least one vertex must be inside the clip boundary **/
                    if ( (psCShape.padfX[j2] >= cxmin  &&  psCShape.padfX[j2] <= cxmax) ||
                         (psCShape.padfY[j2] >= cymin  &&  psCShape.padfY[j2] <= cymax)  )
                    {
                        if (ierase) return(0); /** SKIP  RECORD **/
                             else   return(1); /** WRITE RECORD **/
                    }
                }
                   
                /** All vertices are outside the clip boundary **/ 
                if (ierase) return(1); /** WRITE RECORD **/
                else        return(0); /** SKIP  RECORD **/
            }   /** End TOUCH **/
              
            if (icut)
            {   /** CUT **/
                /*** Check each vertex in the feature with the Boundary and "CUT" ***/
                /*** THIS CODE WAS NOT COMPLETED!  READ NOTE AT THE BOTTOM ***/
                i2=0;
                prev_outside=false;
                for( j2 = 0; j2 < psCShape.nVertices; j2++ ) 
                {
                    inside = psCShape.padfX[j2] >= cxmin  &&  psCShape.padfX[j2] <= cxmax  &&
                             psCShape.padfY[j2] >= cymin  &&  psCShape.padfY[j2] <= cymax ;
                          
                    if (ierase) inside=(! inside);
                    if (inside)
                    {
                        if (i2 != j2)
                        {
                            if (prev_outside)
                            {
                                /*** AddIntersection(i2);   /*** Add intersection ***/
                                prev_outside=false;
                            }
                            psCShape.padfX[i2]=psCShape.padfX[j2];     /** move vertex **/
                            psCShape.padfY[i2]=psCShape.padfY[j2];
                        }
                        i2++;
                    } else {
                        if ( (! prev_outside) && (j2 > 0) )
                        {
                            /*** AddIntersection(i2);   /*** Add intersection (Watch out for j2==i2-1) ***/
                            /*** Also a polygon may overlap twice and will split into a several parts  ***/
                            prev_outside=true;
                        }
                    }
                }
                 
                c.printf("Vertices:{0}   OUT:{1}   Number of Parts:{2}\n",
                    psCShape.nVertices,i2, psCShape.nParts );
                   
                psCShape.nVertices = i2;
                 
                if (i2 < 2) return(0); /** SKIP RECORD **/
                /*** (WE ARE NOT CREATING INTERESECTIONS and some lines could be reduced to one point) **/

                if (i2 == 0) return(0); /** SKIP  RECORD **/
                else         return(1); /** WRITE RECORD **/
            }  /** End CUT **/
            return(0); // TODO:is valid?
        }


        /************************************************************************/
        /*                            strncasecmp2()                            */
        /*                                                                      */
        /*      Compare two strings up to n characters                          */
        /*      If n=0 then s1 and s2 must be an exact match                    */
        /************************************************************************/

        static int strncasecmp2(string s1, string s2, int n)

        {
            /*
            int j,i;
            if (n<1) n=c.strlen(s1)+1;
            for (i=0; i<n; i++)
            {
                if (*s1 != *s2)
                {
                    if (*s1 >= 'a' && *s1 <= 'z') {
                        j=*s1-32;
                        if (j != *s2) return(*s1-*s2);
                    } else {
                        if (*s1 >= 'A' && *s1 <= 'Z') { j=*s1+32; }
                                               else   { j=*s1;    }
                        if (j != *s2) return(*s1-*s2); 
                    }
                }
                s1++;
                s2++;
            }
            return(0);
            */
            if (n<1) {
                return string.Compare(s1, s2);
            } else {
                return string.Compare(s1, 0, s2, 0, n);
            }
        }

        private struct unitkey
        {
            public string   name;
            public double   value;
            public unitkey( string name, double value )
            {
                this.name = name;
                this.value = value;
            }
        }

        static unitkey[] unitkeytab = {
            new unitkey( "CM",            39.37 ),
            new unitkey( "CENTIMETER",    39.37 ),
            new unitkey( "CENTIMETERS",   39.37 ),  /** # of inches * 100 in unit **/
            new unitkey( "METER",          3937 ),
            new unitkey( "METERS",         3937 ),
            new unitkey( "KM",          3937000 ),
            new unitkey( "KILOMETER",   3937000 ), 
            new unitkey( "KILOMETERS",  3937000 ),
            new unitkey( "INCH",            100 ),
            new unitkey( "INCHES",          100 ),
            new unitkey( "FEET",           1200 ),
            new unitkey( "FOOT",           1200 ),
            new unitkey( "YARD",           3600 ),
            new unitkey( "YARDS",          3600 ),       
            new unitkey( "MILE",        6336000 ),
            new unitkey( "MILES",       6336000 )  
        };

        static double findunit(string unit)
        {
            double unitfactor=0;
            for (j = 0; j < unitkeytab.Length; j++) {
                if (strncasecmp2(unit, unitkeytab[j].name, 0) == 0) unitfactor=unitkeytab[j].value;
            }
            return(unitfactor);
        }

        /* -------------------------------------------------------------------- */
        /*      Display a usage message.                                        */
        /* -------------------------------------------------------------------- */
        static void error()
        {
            c.puts( "The program will append to an existing shape file or it will" );
            c.puts( "create a new file if needed." );
            c.puts( "Only the items in the first output file will be preserved." );
            c.puts( "When an item does not match with the append theme then the item");
            c.puts( "might be placed to an existing item at the same position and type." );
            c.puts( "  OTHER FUNCTIONS:" );
            c.puts( "  - Describe all items in the dbase file (Use ALL for more than 5000 recs.)");
            c.puts( "  - Select a group of shapes from a comma separated selection list.");
            c.puts( "  - UnSelect a group of shapes from a comma separated selection list.");
            c.puts( "  - Clip boundary extent or by theme boundary." );
            c.puts( "      Touch writes all the shapes that touch the boundary.");
            c.puts( "      Inside writes all the shapes that are completely within the boundary.");
            c.puts( "      Boundary clips are only the min and max of a theme boundary." );
            c.puts( "  - Erase boundary extent or by theme boundary." );
            c.puts( "      Erase is the direct opposite of the Clip function." );
            c.puts( "  - Change coordinate value units between meters and feet.");
            c.puts( "      There is no way to determine the input unit of a shape file.");
            c.puts( "      Skip this function if the shape file is already in the correct unit.");
            c.puts( "      Clip and Erase will be done before the unit is changed.");
            c.puts( "      A shift will be done after the unit is changed."); 
            c.puts( "  - Shift X and Y coordinates.\n" );
            c.puts( "Finally, There can only be one select or unselect in the command line.");
            c.puts( "         There can only be one clip or erase in the command line.");
            c.puts( "         There can only be one unit and only one shift in the command line.\n");
            c.puts( "Ex: shputils in.shp out.shp   SELECT countycode 3,5,9,13,17,27");
            c.puts( "    shputils in.shp out.shp   CLIP   10 10 90 90 Touch   FACTOR Meter Feet");
            c.puts( "    shputils in.shp out.shp   FACTOR Meter 3.0");
            c.puts( "    shputils in.shp out.shp   CLIP   clip.shp Boundary Touch   SHIFT 40 40");
            c.puts( "    shputils in.shp out.shp   SELECT co 112   CLIP clip.shp Boundary Touch\n");
            c.puts( "USAGE: shputils  <DescribeShape>   {ALL}");
            c.puts( "USAGE: shputils  <InputShape>  <OutShape|AppendShape>" );
            c.puts( "   { <FACTOR>       <FEET|MILES|METERS|KM> <FEET|MILES|METERS|KM|factor> }" );
            c.puts( "   { <SHIFT>        <xshift> <yshift> }" );
            c.puts( "   { <SELECT|UNSEL> <Item> <valuelist> }" );
            c.puts( "   { <CLIP|ERASE>   <xmin> <ymin> <xmax> <ymax> <TOUCH|INSIDE|CUT> }" );
            c.puts( "   { <CLIP|ERASE>   <theme>      <BOUNDARY>     <TOUCH|INSIDE|CUT> }" );
            c.puts( "     Note: CUT is not complete and does not create intersections.");
            c.puts( "           For more information read programmer comment.");

            /****   Clip functions for Polygon and Cut is not supported
            There are several web pages that describe methods of doing this function.
            It seem easy to impliment until you start writting code.  I don't have the
            time to add these functions but a did leave a simple cut routine in the 
            program that can be called by using CUT instead of TOUCH in the 
            CLIP or ERASE functions.  It does not add the intersection of the line and
            the clip box, so polygons could look incomplete and lines will come up short.

            Information about clipping lines with a box:
                   http://www.csclub.uwaterloo.ca/u/mpslager/articles/sutherland/wr.html
                Information about finding the intersection of two lines:
               http://www.whisqu.se/per/docs/math28.htm
               
            THE CODE LOOKS LIKE THIS:
            ********************************************************	  
            void Intersect_Lines(float x0,float y0,float x1,float y1,
                             float x2,float y2,float x3,float y3,
                             float *xi,float *yi)
                             {
            //  this function computes the intersection of the sent lines
            //  and returns the intersection point, note that the function assumes
            //  the lines intersect. the function can handle vertical as well
            //  as horizontal lines. note the function isn't very clever, it simply
            //  applies the math, but we don't need speed since this is a
            //  pre-processing step
            //  The Intersect_lines program came from (http://www.whisqu.se/per/docs/math28.htm)

            float a1,b1,c1, // constants of linear equations 
              a2,b2,c2,
              det_inv,  // the inverse of the determinant of the coefficientmatrix
              m1,m2;    // the slopes of each line
              
            // compute slopes, note the cludge for infinity, however, this will
            // be close enough
            if ((x1-x0)!=0)
            m1 = (y1-y0)/(x1-x0);
            else
            m1 = (float)1e+10;  // close enough to infinity


            if ((x3-x2)!=0) 
            m2 = (y3-y2)/(x3-x2);
            else
            m2 = (float)1e+10;  // close enough to infinity

            // compute constants
            a1 = m1;
            a2 = m2;
            b1 = -1;
            b2 = -1;
            c1 = (y0-m1*x0);
            c2 = (y2-m2*x2);
            // compute the inverse of the determinate
            det_inv = 1/(a1*b2 - a2*b1);
            // use Kramers rule to compute xi and yi
            *xi=((b1*c2 - b2*c1)*det_inv);
            *yi=((a2*c1 - a1*c2)*det_inv);
            } // end Intersect_Lines
            **********************************************************/

            c.exit( 1 );
        }
    }
}
