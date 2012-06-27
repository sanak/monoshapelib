/******************************************************************************
 * dbfadd.cs
 *
 * Project:  MonoShapelib
 * Purpose:  Sample application for adding a record to an existing .dbf file.
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
            int     i, iRecord;

            /* -------------------------------------------------------------------- */
            /*      Display a usage message.                                        */
            /* -------------------------------------------------------------------- */
            if( args.Length < 2 )
            {
                c.printf( "dbfadd xbase_file field_values\n" );

                c.exit( 1 );
            }

            /* -------------------------------------------------------------------- */
            /*      Create the database.                                            */
            /* -------------------------------------------------------------------- */
            hDBF = DBFHandle.Open( args[0], "r+b" );
            if( hDBF == null )
            {
                c.printf( "DBFOpen({0},\"rb+\") failed.\n", args[0] );
                c.exit( 2 );
            }
            
            /* -------------------------------------------------------------------- */
            /*      Do we have the correct number of arguments?                     */
            /* -------------------------------------------------------------------- */
            if( hDBF.GetFieldCount() != args.Length - 1 )
            {
                c.printf( "Got {0} fields, but require {1}\n",
                        args.Length - 2, hDBF.GetFieldCount() );
                c.exit( 3 );
            }

            iRecord = hDBF.GetRecordCount();

            /* -------------------------------------------------------------------- */
            /*      Loop assigning the new field values.                            */
            /* -------------------------------------------------------------------- */
            for( i = 0; i < hDBF.GetFieldCount(); i++ )
            {
                string sFieldName;
                int nWidth, nDecimals;
                if( c.strcmp( args[i+2], "" ) == 0 )
                    hDBF.WriteNULLAttribute( iRecord, i );
                else if( hDBF.GetFieldInfo( i, out sFieldName, out nWidth,
                                           out nDecimals ) == FT.String )
                    hDBF.WriteStringAttribute( iRecord, i, args[i+2] );
                else
                    hDBF.WriteDoubleAttribute( iRecord, i, c.atof(args[i+2]) );
            }

            /* -------------------------------------------------------------------- */
            /*      Close and cleanup.                                              */
            /* -------------------------------------------------------------------- */
            hDBF.Close();

            return( 0 );
        }
    }
}
