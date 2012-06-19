/******************************************************************************
 * shpdump.cs
 *
 * Project:  MonoShapelib
 * Purpose:  Sample application for dumping contents of a shapefile to 
 *           the terminal in human readable form.
 * Author:   Ko Nagase, geosanak@gmail.com
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

using System;
using System.Collections.Generic;

namespace MonoShapelib
{
    class MainClass
    {
        public static int Main( string[] args )
        
        {
            SHPHandle   hSHP;
            SHPT    nShapeType;
            int     nEntities, i, iPart, nInvalidCount=0;
            bool    bValidate = false;
            string  pszPlus;
            double[]  adfMinBound = new double[4], adfMaxBound = new double[4];
        
            if( args.Length > 0 && c.strcmp(args[0],"-validate") == 0 )
            {
                bValidate = true;
                string[] argsTemp = new string[args.Length - 1];
                for( int j = 1; j < args.Length; j++ )
                {
                    argsTemp[j-1] = args[j];
                }
                args = argsTemp;
            }
        
            /* -------------------------------------------------------------------- */
            /*      Display a usage message.                                        */
            /* -------------------------------------------------------------------- */
            if( args.Length != 1 )
            {
                c.printf( "shpdump [-validate] shp_file\n" );
                c.exit( 1 );
            }
        
            /* -------------------------------------------------------------------- */
            /*      Open the passed shapefile.                                      */
            /* -------------------------------------------------------------------- */
            hSHP = SHPHandle.Open( args[0], "rb" );
        
            if( hSHP == null )
            {
                c.printf( "Unable to open:{0}\n", args[0] );
                c.exit( 1 );
            }
        
            /* -------------------------------------------------------------------- */
            /*      Print out the file bounds.                                      */
            /* -------------------------------------------------------------------- */
            hSHP.GetInfo( out nEntities, out nShapeType, adfMinBound, adfMaxBound );
        
            c.printf( "Shapefile Type: {0}   # of Shapes: {1}\n\n",
                    SHP.TypeName( nShapeType ), nEntities );
            
            c.printf( "File Bounds: ({0:F3},{1:F3},{2:G},{3:G})\n"
                     + "         to  ({4:F3},{5:F3},{6:G},{7:G})\n",
                    adfMinBound[0], 
                    adfMinBound[1], 
                    adfMinBound[2], 
                    adfMinBound[3], 
                    adfMaxBound[0], 
                    adfMaxBound[1], 
                    adfMaxBound[2], 
                    adfMaxBound[3] );
            
            /* -------------------------------------------------------------------- */
            /*      Skim over the list of shapes, printing all the vertices.        */
            /* -------------------------------------------------------------------- */
            for( i = 0; i < nEntities; i++ )
            {
                int     j;
                SHPObject   psShape;
        
                psShape = hSHP.ReadObject( i );
        
                c.printf( "\nShape:{0} ({1})  nVertices={2}, nParts={3}\n"
                       + "  Bounds:({4:F3},{5:F3}, {6:G}, {7:G})\n"
                         + "      to ({8:F3},{9:F3}, {10:G}, {11:G})\n",
                        i, SHP.TypeName(psShape.nSHPType),
                            psShape.nVertices, psShape.nParts,
                            psShape.dfXMin, psShape.dfYMin,
                            psShape.dfZMin, psShape.dfMMin,
                            psShape.dfXMax, psShape.dfYMax,
                            psShape.dfZMax, psShape.dfMMax );
        
                for( j = 0, iPart = 1; j < psShape.nVertices; j++ )
                {
                    string   pszPartType = "";
            
                    if( j == 0 && psShape.nParts > 0 )
                        pszPartType = SHP.PartTypeName( (SHPP)psShape.panPartType[0] );
                        
                    if( iPart < psShape.nParts
                            && psShape.panPartStart[iPart] == j )
                    {
                        pszPartType = SHP.PartTypeName( (SHPP)psShape.panPartType[iPart] );
                        iPart++;
                        pszPlus = "+";
                    }
                    else
                        pszPlus = " ";
                
                    c.printf("   {0} ({1:F3},{2:F3}, {3:G}, {4:G}) {5} \n",
                               pszPlus,
                               psShape.padfX[j],
                               psShape.padfY[j],
                               psShape.padfZ[j],
                               psShape.padfM[j],
                               pszPartType );
                }
        
                if( bValidate )
                {
                    int nAltered = hSHP.RewindObject( psShape );
        
                    if( nAltered > 0 )
                    {
                        c.printf( "  {0} rings wound in the wrong direction.\n",
                                nAltered );
                        nInvalidCount++;
                    }
                }

                psShape = null;
            }
        
            hSHP.Close();
        
            if( bValidate )
            {
                c.printf( "{0} object has invalid ring orderings.\n", nInvalidCount );
            }
        
//#if USE_DBMALLOC
//          malloc_dump(2);
//#endif
        
            //c.exit( 0 );
            return 0;
        }
    }
}
