/******************************************************************************
 * c.cs
 *
 * Project:  MonoShapelib
 * Purpose:  C function emulation file for MonoShapelib.
 * Author:   Ko Nagase, geosanak@gmail.com
 *
 ******************************************************************************
 * Copyright (c) 2012, Ko Nagase
 *
 * This software is available under the following "MIT Style" license,
 * or at the option of the licensee under the LGPL (see LICENSE.LGPL).
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
using System.Diagnostics;
using System.IO;

namespace MonoShapelib
{
    public class c
    {
        #region Methods

        #region <memory.h>

        public static void memcpy( ref double dst, byte[] src, int offset, int count )
    
        {
            Debug.Assert( count == 8 );
            dst = BitConverter.ToDouble( src, offset );
        }

        public static void memcpy( ref int dst, byte[] src, int offset, int count )
    
        {
            Debug.Assert( count == 4 );
            dst = BitConverter.ToInt32( src, offset );
        }

        public static void memcpy( byte[] b, int offset, int[] a, int c )

        {
            for( int i = 0; i < c/4; i++ )
            {
                Buffer.BlockCopy( BitConverter.GetBytes( a[i] ), 0, b, offset + i * 4, 4 );
            }
        }

        public static void memcpy( int[] b, byte[] a, int offset, int c )
        
        {
            for( int i = 0; i < c/4; i++ )
            {
                b[i] = BitConverter.ToInt32( a, offset + i * 4 );
            }
        }

        #endregion

        #region <stdio.h>

        public static int fclose( FileStream stream )
        
        {
            try
            {
                stream.Close();
                return 0;
            }
            catch
            {
                return -1; // EOF
            }
        }

        public static FileStream fopen( string filename, string mode )

        {
            FileStream  stream = null;
            FileAccess access = FileAccess.Read;
            FileMode _mode = FileMode.Open;
            switch( mode )
            {
            case "rb":
                access = FileAccess.Read;
                _mode = FileMode.Open;
                break;
            case "r+b":
                access = FileAccess.ReadWrite;
                _mode = FileMode.Open;
                break;
            case "wb":
                access = FileAccess.Write;
                _mode = FileMode.Create;
                break;
            }
            try
            {
                stream = new FileStream( filename, _mode, access );
            }
            catch
            {
                stream = null;
            }

            return stream;
        }

        public static int fprintf( TextWriter stream, string format, params object[] args )

        {
            string str = string.Format( format, args );
            stream.Write( str );
            return str.Length;
        }

        public static int fread( byte[] buffer, int size, int count, FileStream stream )
        
        {
            try
            {
                if( size == 0 || count == 0 ) return 0;
                return stream.Read( buffer, 0, size * count );
            }
            catch
            {
                return 0;
            }
        }

        public static int fseek( FileStream stream, int offset, int origin )

        {
            try
            {
                SeekOrigin _origin = SeekOrigin.Begin;
                switch( origin )
                {
                case 0: // SEEK_SET
                    _origin = SeekOrigin.Begin;
                    break;
                case 1: // SEEK_CUR
                    _origin = SeekOrigin.Current;
                    break;
                case 2: // SEEK_END
                    _origin = SeekOrigin.End;
                    break;
                default:
                    return -1;
                }
                stream.Seek( offset, _origin );
                return 0;
            }
            catch
            {
                return -1;
            }
        }

        public static int fwrite( byte[] buffer, int size, int count, FileStream stream )

        {
            try
            {
                long spos = stream.Position;
                stream.Write( buffer, 0, size * count );
                long epos = stream.Position;
                return( (int)( epos - spos ) );
            }
            catch
            {
                return 0;
            }
        }

        public static int printf( string format, params object[] args )
        {
            string  buffer = string.Format( format, args );
            Console.Write( buffer );
            return buffer.Length;
        }

        public static int sprintf( ref string buffer, string format, params object[] args )
        {
            buffer = string.Format( format, args );
            return buffer.Length;
        }

        // TODO:use format parameter
        public static int sscanf( string buffer, string format, ref double argument )
        {
            try
            {
                argument = double.Parse( buffer );
                return 1;
            }
            catch
            {
                return 0;
            }
        }

        #endregion

        #region <stdlib.h>

        public static void exit( int status )
        {
            Environment.Exit( status );
        }

        public static void free( ref string memblock )

        {
            memblock = null;
        }

        public static void free( ref FileStream memblock )

        {
            memblock = null;
        }

        public static void free( ref byte[] memblock )

        {
            memblock = null;
        }

        public static void free( ref int[] memblock )

        {
            memblock = null;
        }

        public static void free( ref double[] memblock )

        {
            memblock = null;
        }

        public static void free( ref SHPHandle memblock )

        {
            memblock = null;
        }

        public static T[] realloc<T>( ref T[] pMem, int nNewSize )
        
        {
            Array.Resize( ref pMem, nNewSize );
            return pMem;
        }

        #endregion

        #region <string.h>

        public static int strcmp( string string1, string string2 )
        {
            return string.Compare( string1, string2 );
        }

        public static string strcpy( ref string strDst, string strSrc )
        {
            strDst = strSrc;
            return strDst;
        }

        public static int strlen( string str )
        {
            if( string.IsNullOrEmpty( str ) )
                return 0;
            else
                return str.Length;
        }

        #endregion

        #endregion
    }
}
