/******************************************************************************
 * shptest.cs
 *
 * Project:  MonoShapelib
 * Purpose:  Application for generating sample Shapefiles of various types.
 *           Used by the stream2.sh test script.
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
        /// <summary>
        /// Write a small point file.
        /// </summary>
        static void Test_WritePoints( SHPT nSHPType, string pszFilename )

        {
            SHPHandle   hSHPHandle;
            SHPObject   psShape;
            double[]    x = new double[1], y = new double[1];
            double[]    z = new double[1], m = new double[1];

            hSHPHandle = SHPHandle.Create( pszFilename, nSHPType );

            x[0] = 1.0;
            y[0] = 2.0;
            z[0] = 3.0;
            m[0] = 4.0;
            psShape = SHPObject.Create( nSHPType, -1, 0, null, null,
                                       1, x, y, z, m );
            hSHPHandle.WriteObject( -1, psShape );
            psShape = null;
            
            x[0] = 10.0;
            y[0] = 20.0;
            z[0] = 30.0;
            m[0] = 40.0;
            psShape = SHPObject.Create( nSHPType, -1, 0, null, null,
                                       1, x, y, z, m );
            hSHPHandle.WriteObject( -1, psShape );
            psShape = null;

            hSHPHandle.Close();
        }

        /// <summary>
        /// Write a small multipoint file.
        /// </summary>
        static void Test_WriteMultiPoints( SHPT nSHPType, string pszFilename )

        {
            SHPHandle   hSHPHandle;
            SHPObject   psShape;
            double[]    x = new double[4], y = new double[4];
            double[]    z = new double[4], m = new double[4];
            int         i, iShape;

            hSHPHandle = SHPHandle.Create( pszFilename, nSHPType );

            for( iShape = 0; iShape < 3; iShape++ )
            {
                for( i = 0; i < 4; i++ )
                {
                    x[i] = iShape * 10 + i + 1.15;
                    y[i] = iShape * 10 + i + 2.25;
                    z[i] = iShape * 10 + i + 3.35;
                    m[i] = iShape * 10 + i + 4.45;
                }
                
                psShape = SHPObject.Create( nSHPType, -1, 0, null, null,
                                           4, x, y, z, m );
                hSHPHandle.WriteObject( -1, psShape );
                psShape = null;
            }    

            hSHPHandle.Close();
        }

        /// <summary>
        /// Write a small arc or polygon file.
        /// </summary>
        static void Test_WriteArcPoly( SHPT nSHPType, string pszFilename )

        {
            SHPHandle   hSHPHandle;
            SHPObject   psShape;
            double[]    x = new double[100], y = new double[100];
            double[]    z = new double[100], m = new double[100];
            int[]       anPartStart = new int[100];
            int[]       anPartType = new int[100], panPartType;
            int         i, iShape;

            hSHPHandle = SHPHandle.Create( pszFilename, nSHPType );

            if( nSHPType == SHPT.MULTIPATCH )
                panPartType = anPartType;
            else
                panPartType = null;

            for( iShape = 0; iShape < 3; iShape++ )
            {
                x[0] = 1.0;
                y[0] = 1.0+iShape*3;
                x[1] = 2.0;
                y[1] = 1.0+iShape*3;
                x[2] = 2.0;
                y[2] = 2.0+iShape*3;
                x[3] = 1.0;
                y[3] = 2.0+iShape*3;
                x[4] = 1.0;
                y[4] = 1.0+iShape*3;

                for( i = 0; i < 5; i++ )
                {
                    z[i] = iShape * 10 + i + 3.35;
                    m[i] = iShape * 10 + i + 4.45;
                }
                
                psShape = SHPObject.Create( nSHPType, -1, 0, null, null,
                                           5, x, y, z, m );
                hSHPHandle.WriteObject( -1, psShape );
                psShape = null;
            }

            /* -------------------------------------------------------------------- */
            /*      Do a multi part polygon (shape).  We close it, and have two     */
            /*      inner rings.                                                    */
            /* -------------------------------------------------------------------- */
            x[0] = 0.0;
            y[0] = 0.0;
            x[1] = 0;
            y[1] = 100;
            x[2] = 100;
            y[2] = 100;
            x[3] = 100;
            y[3] = 0;
            x[4] = 0;
            y[4] = 0;

            x[5] = 10;
            y[5] = 20;
            x[6] = 30;
            y[6] = 20;
            x[7] = 30;
            y[7] = 40;
            x[8] = 10;
            y[8] = 40;
            x[9] = 10;
            y[9] = 20;

            x[10] = 60;
            y[10] = 20;
            x[11] = 90;
            y[11] = 20;
            x[12] = 90;
            y[12] = 40;
            x[13] = 60;
            y[13] = 40;
            x[14] = 60;
            y[14] = 20;

            for( i = 0; i < 15; i++ )
            {
                z[i] = i;
                m[i] = i*2;
            }

            anPartStart[0] = 0;
            anPartStart[1] = 5;
            anPartStart[2] = 10;

            anPartType[0] = (int)SHPP.RING;
            anPartType[1] = (int)SHPP.INNERRING;
            anPartType[2] = (int)SHPP.INNERRING;
            
            psShape = SHPObject.Create( nSHPType, -1, 3, anPartStart, panPartType,
                                       15, x, y, z, m );
            hSHPHandle.WriteObject( -1, psShape );
            psShape = null;
            

            hSHPHandle.Close();
        }

        public static int Main( string[] args )

        {
            /* -------------------------------------------------------------------- */
            /*      Display a usage message.                                        */
            /* -------------------------------------------------------------------- */
            if( args.Length != 1 )
            {
                c.printf( "mshptest test_number\n" );
                c.exit( 1 );
            }

            /* -------------------------------------------------------------------- */
            /*      Figure out which test to run.                                   */
            /* -------------------------------------------------------------------- */

            if( c.atoi(args[0]) == 0 )
                Test_WritePoints( SHPT.NULL, "test0.shp" );

            else if( c.atoi(args[0]) == 1 )
                Test_WritePoints( SHPT.POINT, "test1.shp" );
            else if( c.atoi(args[0]) == 2 )
                Test_WritePoints( SHPT.POINTZ, "test2.shp" );
            else if( c.atoi(args[0]) == 3 )
                Test_WritePoints( SHPT.POINTM, "test3.shp" );

            else if( c.atoi(args[0]) == 4 )
                Test_WriteMultiPoints( SHPT.MULTIPOINT, "test4.shp" );
            else if( c.atoi(args[0]) == 5 )
                Test_WriteMultiPoints( SHPT.MULTIPOINTZ, "test5.shp" );
            else if( c.atoi(args[0]) == 6 )
                Test_WriteMultiPoints( SHPT.MULTIPOINTM, "test6.shp" );

            else if( c.atoi(args[0]) == 7 )
                Test_WriteArcPoly( SHPT.ARC, "test7.shp" );
            else if( c.atoi(args[0]) == 8 )
                Test_WriteArcPoly( SHPT.ARCZ, "test8.shp" );
            else if( c.atoi(args[0]) == 9 )
                Test_WriteArcPoly( SHPT.ARCM, "test9.shp" );

            else if( c.atoi(args[0]) == 10 )
                Test_WriteArcPoly( SHPT.POLYGON, "test10.shp" );
            else if( c.atoi(args[0]) == 11 )
                Test_WriteArcPoly( SHPT.POLYGONZ, "test11.shp" );
            else if( c.atoi(args[0]) == 12 )
                Test_WriteArcPoly( SHPT.POLYGONM, "test12.shp" );
            
            else if( c.atoi(args[0]) == 13 )
                Test_WriteArcPoly( SHPT.MULTIPATCH, "test13.shp" );
            else
            {
                c.printf( "Test `{0}' not recognised.\n", args[0] );
                c.exit( 10 );
            }

//#ifdef USE_DBMALLOC
//          malloc_dump(2);
//#endif

            //c.exit( 0 );
            return 0;
        }
    }
}
