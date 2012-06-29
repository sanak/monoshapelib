/******************************************************************************
 * dbfdump.cs
 *
 * Project:  MonoShapelib
 * Purpose:  Sample application for dumping .dbf files to the terminal.
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

namespace MonoShapelib
{
    class MainClass
    {
        public static int Main( string[] args )

        {
            DBFHandle   hDBF;
            int[]   panWidth;
            int     i, iRecord;
            string  szFormat, pszFilename = null;
            int     nWidth, nDecimals;
            bool    bHeader = false;
            bool    bRaw = false;
            bool    bMultiLine = false;
            string  szTitle;
            int     mblendiff; // multibyte length diff
            string  szStringAttribute;

            /* -------------------------------------------------------------------- */
            /*      Handle arguments.                                               */
            /* -------------------------------------------------------------------- */
            for( i = 0; i < args.Length; i++ )
            {
                if( c.strcmp(args[i],"-h") == 0 )
                    bHeader = true;
                else if( c.strcmp(args[i],"-r") == 0 )
                    bRaw = true;
                else if( c.strcmp(args[i],"-m") == 0 )
                    bMultiLine = true;
                else
                    pszFilename = args[i];
            }

            /* -------------------------------------------------------------------- */
            /*      Display a usage message.                                        */
            /* -------------------------------------------------------------------- */
            if( pszFilename == null )
            {
                c.printf( "mdbfdump [-h] [-r] [-m] xbase_file\n" );
                    c.printf( "        -h: Write header info (field descriptions)\n" );
                    c.printf( "        -r: Write raw field info, numeric values not reformatted\n" );
                    c.printf( "        -m: Multiline, one line per field.\n" );
                c.exit( 1 );
            }

            /* -------------------------------------------------------------------- */
            /*      Open the file.                                                  */
            /* -------------------------------------------------------------------- */
            hDBF = DBFHandle.Open( pszFilename, "rb" );
            if( hDBF == null )
            {
                c.printf( "DBFHandle.Open({0},\"r\") failed.\n", args[0] );
                c.exit( 2 );
            }
            
            /* -------------------------------------------------------------------- */
            /*      If there is no data in this file let the user know.             */
            /* -------------------------------------------------------------------- */
            if( hDBF.GetFieldCount() == 0 )
            {
                c.printf( "There are no fields in this table!\n" );
                c.exit( 3 );
            }

            /* -------------------------------------------------------------------- */
            /*      Dump header definitions.                                        */
            /* -------------------------------------------------------------------- */
            if( bHeader )
            {
                for( i = 0; i < hDBF.GetFieldCount(); i++ )
                {
                    FT      eType;
                    string  pszTypeName = null;

                    eType = hDBF.GetFieldInfo( i, out szTitle, out nWidth, out nDecimals );
                    if( eType == FT.String )
                        pszTypeName = "String";
                    else if( eType == FT.Integer )
                        pszTypeName = "Integer";
                    else if( eType == FT.Double )
                        pszTypeName = "Double";
                    else if( eType == FT.Logical )
                        pszTypeName = "Logical";
                    else if( eType == FT.Invalid )
                        pszTypeName = "Invalid";

                    c.printf( "Field {0}: Type={1}, Title=`{2}', Width={3}, Decimals={4}\n",
                            i, pszTypeName, szTitle, nWidth, nDecimals );
                }
            }

            /* -------------------------------------------------------------------- */
            /*      Compute offsets to use when printing each of the field          */
            /*      values. We make each field as wide as the field title+1, or     */
            /*      the field value + 1.                                            */
            /* -------------------------------------------------------------------- */
            panWidth = new int[hDBF.GetFieldCount()];

            for( i = 0; i < hDBF.GetFieldCount() && !bMultiLine; i++ )
            {
                FT  eType;

                eType = hDBF.GetFieldInfo( i, out szTitle, out nWidth, out nDecimals );
                if( c.strlen(szTitle) > nWidth )
                    panWidth[i] = c.strlen(szTitle);
                else
                    panWidth[i] = nWidth;

                mblendiff = c.cpg.GetByteCount( szTitle ) - szTitle.Length;
                if( eType == FT.String )
                    szFormat = string.Concat( "{0,-", (panWidth[i] - mblendiff).ToString(), "} " );
                else
                    szFormat = string.Concat( "{0,", (panWidth[i] - mblendiff).ToString(), "} " );
                c.printf( szFormat, szTitle );
            }
            c.printf( "\n" );

            /* -------------------------------------------------------------------- */
            /*      Read all the records                                            */
            /* -------------------------------------------------------------------- */
            for( iRecord = 0; iRecord < hDBF.GetRecordCount(); iRecord++ )
            {
                if( bMultiLine )
                    c.printf( "Record: {0}\n", iRecord );
                
                for( i = 0; i < hDBF.GetFieldCount(); i++ )
                {
                    FT  eType;
                    
                    eType = hDBF.GetFieldInfo( i, out szTitle, out nWidth, out nDecimals );

                    if( bMultiLine )
                    {
                        c.printf( "{0}: ", szTitle );
                    }
                    
                    /* -------------------------------------------------------------------- */
                    /*      Print the record according to the type and formatting           */
                    /*      information implicit in the DBF field description.              */
                    /* -------------------------------------------------------------------- */
                    if( !bRaw )
                    {
                        if( hDBF.IsAttributeNull( iRecord, i ) )
                        {
                            if( eType == FT.String )
                                szFormat = string.Concat( "{0,-", nWidth.ToString(), "}" );
                            else
                                szFormat = string.Concat( "{0,", nWidth.ToString(), "}" );

                            c.printf( szFormat, "(NULL)" );
                        }
                        else
                        {
                            switch( eType )
                            {
                            case FT.String:
                                szStringAttribute = hDBF.ReadStringAttribute( iRecord, i );
                                mblendiff = c.cpg.GetByteCount( szStringAttribute ) - szStringAttribute.Length;
                                szFormat = string.Concat( "{0,-", (nWidth - mblendiff).ToString(), "}" );
                                c.printf( szFormat, 
                                        szStringAttribute);
                                break;
                                
                            case FT.Integer:
                                szFormat = string.Concat( "{0,", nWidth.ToString(), "}" );
                                c.printf( szFormat, 
                                        hDBF.ReadIntegerAttribute( iRecord, i ) );
                                break;

                            case FT.Double:
                                szFormat = string.Concat( "{0,", nWidth.ToString(),
                                                         ":F", nDecimals.ToString(), "}" );
                                c.printf( szFormat, 
                                        hDBF.ReadDoubleAttribute( iRecord, i ) );
                                break;

                            case FT.Logical:
                                szFormat = string.Concat( "{0,", nWidth.ToString(), "}" );
                                c.printf( szFormat,
                                         hDBF.ReadLogicalAttribute( iRecord, i ) );
                                break;
                                
                            default:
                                break;
                            }
                        }
                    }

                    /* -------------------------------------------------------------------- */
                    /*      Just dump in raw form (as formatted in the file).               */
                    /* -------------------------------------------------------------------- */
                    else
                    {
                        szFormat = string.Concat( "{0,-", nWidth.ToString(), "}" );
                        c.printf( szFormat, 
                                hDBF.ReadStringAttribute( iRecord, i ) );
                    }

                    /* -------------------------------------------------------------------- */
                    /*      Write out any extra spaces required to pad out the field        */
                    /*      width.                                                          */
                    /* -------------------------------------------------------------------- */
                    if( !bMultiLine )
                    {
                        szFormat = string.Concat( "{0,", (panWidth[i] - nWidth + 1).ToString(), "}" );
                        c.printf( szFormat, "" );
                    }

                    if( bMultiLine )
                        c.printf( "\n" );

                    c.fflush( Console.Out );
                }
                c.printf( "\n" );
            }

            hDBF.Close();

            return( 0 );
        }
    }
}
