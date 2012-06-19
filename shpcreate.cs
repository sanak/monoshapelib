/******************************************************************************
 * shpcreate.cs
 *
 * Project:  MonoShapelib
 * Purpose:  Sample application for creating a new shapefile.
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
            SHPT    nShapeType = SHPT.NULL;
            
            /* -------------------------------------------------------------------- */
            /*      Display a usage message.                                        */
            /* -------------------------------------------------------------------- */
            if( args.Length != 2 )
            {
                c.printf( "mshpcreate shp_file [point/arc/polygon/multipoint]\n" );
                c.exit( 1 );
            }
            
            /* -------------------------------------------------------------------- */
            /*      Figure out the shape type.                                      */
            /* -------------------------------------------------------------------- */
            if( c.strcmp(args[1],"POINT") == 0 || c.strcmp(args[1],"point") == 0 )
                nShapeType = SHPT.POINT;
            else if( c.strcmp(args[1],"ARC") == 0 || c.strcmp(args[1],"arc") == 0 )
                nShapeType = SHPT.ARC;
            else if( c.strcmp(args[1],"POLYGON") == 0 || c.strcmp(args[1],"polygon") == 0 )
                nShapeType = SHPT.POLYGON;
            else if( c.strcmp(args[1],"MULTIPOINT") == 0 || c.strcmp(args[1],"multipoint") == 0)
                nShapeType = SHPT.MULTIPOINT;
            else
            {
                c.printf( "Shape Type `{0}' not recognised.\n", args[1] );
                c.exit( 2 );
            }
            
            /* -------------------------------------------------------------------- */
            /*      Create the requested layer.                                     */
            /* -------------------------------------------------------------------- */
            hSHP = SHPHandle.Create( args[0], nShapeType );
            
            if( hSHP == null )
            {
                c.printf( "Unable to create:{0}\n", args[0] );
                c.exit( 3 );
            }
            
            hSHP.Close();
            
            return 0;
        }
    }
}
