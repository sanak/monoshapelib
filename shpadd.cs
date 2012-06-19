/******************************************************************************
 * shpadd.cs
 *
 * Project:  MonoShapelib
 * Purpose:  Sample application for adding a shape to a shapefile.
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

namespace MonoShapelib
{
    class MainClass
    {
        public static int Main( string[] args )
        
        {
            SHPHandle   hSHP;
            SHPT        nShapeType;
            int         nEntities, nVertices, nParts, i, nVMax;
            int[]       panParts;
            double[]    padfX, padfY;
            SHPObject   psObject;
        
            /* -------------------------------------------------------------------- */
            /*      Display a usage message.                                        */
            /* -------------------------------------------------------------------- */
            if( args.Length < 1 )
            {
                c.printf( "mshpadd shp_file [[x y] [+]]*\n" );
                c.exit( 1 );
            }
        
            /* -------------------------------------------------------------------- */
            /*      Open the passed shapefile.                                      */
            /* -------------------------------------------------------------------- */
            hSHP = SHPHandle.Open( args[0], "r+b" );
        
            if( hSHP == null )
            {
                c.printf( "Unable to open:{0}\n", args[0] );
                c.exit( 1 );
            }
        
            hSHP.GetInfo( out nEntities, out nShapeType, null, null );
        
            if( args.Length == 1 )
                nShapeType = SHPT.NULL;
        
            /* -------------------------------------------------------------------- */
            /*      Build a vertex/part list from the command line arguments.       */
            /* -------------------------------------------------------------------- */
            nVMax = 1000;
            padfX = new double[nVMax];
            padfY = new double[nVMax];
            
            nVertices = 0;
        
            panParts = null;
            try
            {
                panParts = new int[1000];
            }
            catch
            {
                c.printf( "Out of memory\n" );
                c.exit( 1 );
            }
            
            nParts = 1;
            panParts[0] = 0;
        
            for( i = 1; i < args.Length;  )
            {
                if( args[i][0] == '+' )
                {
                    panParts[nParts++] = nVertices;
                    i++;
                }
                else if( i < args.Length-1 )
                {
                    if( nVertices == nVMax )
                    {
                        nVMax = nVMax * 2;
                        padfX = (double[]) c.realloc(ref padfX,nVMax);
                        padfY = (double[]) c.realloc(ref padfY,nVMax);
                    }
            
                    c.sscanf( args[i], "{0:G}", ref padfX[nVertices] );
                    c.sscanf( args[i+1], "{0:G}", ref padfY[nVertices] );
                    nVertices += 1;
                    i += 2;
                }
            }
        
            /* -------------------------------------------------------------------- */
            /*      Write the new entity to the shape file.                         */
            /* -------------------------------------------------------------------- */
            psObject = SHPObject.Create( nShapeType, -1, nParts, panParts, null,
                                        nVertices, padfX, padfY, null, null );
            hSHP.WriteObject( -1, psObject );
            psObject = null;
            
            hSHP.Close();
        
            c.free( ref panParts );
            c.free( ref padfX );
            c.free( ref padfY );
        
            return 0;
        }
    }
}
