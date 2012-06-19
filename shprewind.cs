/******************************************************************************
 * shprewind.cs
 *
 * Project:  MonoShapelib
 * Purpose:  Utility to validate and reset the winding order of rings in
 *           polygon geometries to match the ordering required by spec.
 * Author:   Ko Nagase, geosanak@gmail.com
 *
 ******************************************************************************
 * Copyright (c) 2002, Frank Warmerdam
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
            SHPHandle   hSHP, hSHPOut;
            SHPT    nShapeType;
            int     nEntities, i, nInvalidCount=0;
            double[] adfMinBound = new double[4], adfMaxBound = new double[4];
        
            /* -------------------------------------------------------------------- */
            /*      Display a usage message.                                        */
            /* -------------------------------------------------------------------- */
            if( args.Length != 2 )
            {
                c.printf( "mshprewind in_shp_file out_shp_file\n" );
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
        
            hSHP.GetInfo( out nEntities, out nShapeType, adfMinBound, adfMaxBound );
            
            /* -------------------------------------------------------------------- */
            /*      Create output shapefile.                                        */
            /* -------------------------------------------------------------------- */
            hSHPOut = SHPHandle.Create( args[1], nShapeType );
        
            if( hSHPOut == null )
            {
                c.printf( "Unable to create:{0}\n", args[1] );
                c.exit( 1 );
            }
        
            /* -------------------------------------------------------------------- */
            /*    Skim over the list of shapes, printing all the vertices.          */
            /* -------------------------------------------------------------------- */
            for( i = 0; i < nEntities; i++ )
            {
                //int j;
                SHPObject   psShape;
                
                psShape = hSHP.ReadObject( i );
                if( hSHP.RewindObject( psShape ) != 0 )
                    nInvalidCount++;
                hSHPOut.WriteObject( -1, psShape );
                psShape = null;
            }
        
            hSHP.Close();
            hSHPOut.Close();
        
            c.printf( "{0} objects rewound.\n", nInvalidCount );
        
            //c.exit( 0 );
            return 0;
        }
    }
}
