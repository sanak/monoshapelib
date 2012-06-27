/******************************************************************************
 * dbfcreate.cs
 *
 * Project:  MonoShapelib
 * Purpose:  Sample application for creating a new .dbf file.
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
            DBFHandle   hDBF;
            int     i;

            /* -------------------------------------------------------------------- */
            /*      Display a usage message.                                        */
            /* -------------------------------------------------------------------- */
            if( args.Length < 1 )
            {
                c.printf( "dbfcreate xbase_file [[-s field_name width],[-n field_name width decimals]]...\n" );

                c.exit( 1 );
            }

            /* -------------------------------------------------------------------- */
            /*      Create the database.                                            */
            /* -------------------------------------------------------------------- */
            hDBF = DBFHandle.Create( args[0] );
            if( hDBF == null )
            {
                c.printf( "DBFCreate({0}) failed.\n", args[0] );
                c.exit( 2 );
            }

            /* -------------------------------------------------------------------- */
            /*      Loop over the field definitions adding new fields.              */
            /* -------------------------------------------------------------------- */
            for( i = 1; i < args.Length; i++ )
            {
                if( c.strcmp(args[i],"-s") == 0 && i < args.Length-2 )
                {
                    if( hDBF.AddField( args[i+1], FT.String, c.atoi(args[i+2]), 0 )
                            == -1 )
                    {
                        c.printf( "DBFAddField({0},FTString,{1},0) failed.\n",
                                args[i+1], c.atoi(args[i+2]) );
                        c.exit( 4 );
                    }
                    i = i + 2;
                }
                else if( c.strcmp(args[i],"-n") == 0 && i < args.Length-3 )
                {
                    if( hDBF.AddField( args[i+1], FT.Double, c.atoi(args[i+2]), 
                                c.atoi(args[i+3]) ) == -1 )
                    {
                        c.printf( "DBFAddField({0},FTDouble,{1},{2}) failed.\n",
                                args[i+1], c.atoi(args[i+2]), c.atoi(args[i+3]) );
                        c.exit( 4 );
                    }
                    i = i + 3;
                }
                else
                {
                    c.printf( "Argument incomplete, or unrecognised:{0}\n", args[i] );
                    c.exit( 3 );
                }
            }

            hDBF.Close();

            return( 0 );
        }
    }
}
