/******************************************************************************
 * shptreedump.cs
 *
 * Project:  MonoShapelib
 * Purpose:  Mainline for creating and dumping an ASCII representation of
 *           a quadtree.
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
        static void Usage()

        {
            c.printf( "mshptreedump [-maxdepth n] [-search xmin ymin xmax ymax]\n"
                    + "            [-v] shp_file\n" );
            c.exit( 1 );
        }

        public static int Main( string[] args )

        {
            SHPHandle   hSHP;
            SHPTree psTree;
            int     nExpandShapes = 0;
            int     nMaxDepth = 0;
            int     nDoSearch = 0;
            double[] adfSearchMin = new double[4], adfSearchMax = new double[4];


            /* -------------------------------------------------------------------- */
            /*      Consume flags.                                                  */
            /* -------------------------------------------------------------------- */
            while( args.Length > 0 )
            {
                if( c.strcmp(args[0],"-v") == 0 )
                {
                    nExpandShapes = 1;
                    string[] argsTemp = new string[args.Length - 1];
                    for( int i = 1; i < argsTemp.Length; i++ )
                    {
                        argsTemp[i - 1] = args[i];
                    }
                    args = argsTemp;
                }
                else if( c.strcmp(args[0],"-maxdepth") == 0 && args.Length > 1 )
                {
                    nMaxDepth = c.atoi(args[1]);
                    string[] argsTemp = new string[args.Length - 2];
                    for( int i = 2; i < argsTemp.Length; i++ )
                    {
                        argsTemp[i - 2] = args[i];
                    }
                    args = argsTemp;
                }
                else if( c.strcmp(args[0],"-search") == 0 && args.Length > 4 )
                {
                    nDoSearch = 1;

                    adfSearchMin[0] = c.atof(args[1]);
                    adfSearchMin[1] = c.atof(args[2]);
                    adfSearchMax[0] = c.atof(args[3]);
                    adfSearchMax[1] = c.atof(args[4]);

                    adfSearchMin[2] = adfSearchMax[2] = 0.0;
                    adfSearchMin[3] = adfSearchMax[3] = 0.0;

                    if( adfSearchMin[0] > adfSearchMax[0]
                        || adfSearchMin[1] > adfSearchMax[1] )
                    {
                        c.printf( "Min greater than max in search criteria.\n" );
                        Usage();
                    }
                    
                    string[] argsTemp = new string[args.Length - 5];
                    for( int i = 5; i < argsTemp.Length; i++ )
                    {
                        argsTemp[i - 5] = args[i];
                    }
                    args = argsTemp;
                }
                else
                    break;
            }

            /* -------------------------------------------------------------------- */
            /*      Display a usage message.                                        */
            /* -------------------------------------------------------------------- */
            if( args.Length < 1 )
            {
                Usage();
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
            /*      Build a quadtree structure for this file.                       */
            /* -------------------------------------------------------------------- */
            psTree = SHPTree.Create( hSHP, 2, nMaxDepth, null, null );

            /* -------------------------------------------------------------------- */
            /*      Trim unused nodes from the tree.                                */
            /* -------------------------------------------------------------------- */
            psTree.TrimExtraNodes();
                
            /* -------------------------------------------------------------------- */
            /*      Dump tree by recursive descent.                                 */
            /* -------------------------------------------------------------------- */
            if( nDoSearch == 0 )
                SHPTreeNodeDump( psTree, psTree.psRoot, "", nExpandShapes );

            /* -------------------------------------------------------------------- */
            /*      or do a search instead.                                         */
            /* -------------------------------------------------------------------- */
            else
                SHPTreeNodeSearchAndDump( psTree, adfSearchMin, adfSearchMax );

            /* -------------------------------------------------------------------- */
            /*      cleanup                                                         */
            /* -------------------------------------------------------------------- */
            psTree.Destroy();

            hSHP.Close();

//#ifdef USE_DBMALLOC
//          malloc_dump(2);
//#endif

            //c.exit( 0 );
            return 0;
        }

        static void EmitCoordinate( double[] padfCoord, int nDimension )

        {
            string   pszFormat;
            
            if( c.fabs(padfCoord[0]) < 180 && c.fabs(padfCoord[1]) < 180 )
                pszFormat = "{0:F9}";
            else
                pszFormat = "{0:F2}";

            c.printf( pszFormat, padfCoord[0] );
            c.printf( "," );
            c.printf( pszFormat, padfCoord[1] );

            if( nDimension > 2 )
            {
                c.printf( "," );
                c.printf( pszFormat, padfCoord[2] );
            }
            if( nDimension > 3 )
            {
                c.printf( "," );
                c.printf( pszFormat, padfCoord[3] );
            }
        }

        static void EmitShape( SHPObject psObject, string pszPrefix,
                               int nDimension )

        {
            int     i;
            
            c.printf( "{0}( Shape\n", pszPrefix );
            c.printf( "{0}  ShapeId = {1}\n", pszPrefix, psObject.nShapeId );

            c.printf( "{0}  Min = (", pszPrefix );
            double[]    adfMin = new double[] { psObject.dfXMin, psObject.dfYMin,
                                                psObject.dfZMin, psObject.dfMMin };
            EmitCoordinate( adfMin, nDimension );
            c.printf( ")\n" );
            
            c.printf( "{0}  Max = (", pszPrefix );
            double[]    adfMax = new double[] { psObject.dfXMax, psObject.dfYMax,
                                                psObject.dfZMax, psObject.dfMMax };
            EmitCoordinate( adfMax, nDimension );
            c.printf( ")\n" );

            for( i = 0; i < psObject.nVertices; i++ )
            {
                double[]    adfVertex = new double[4];
                
                c.printf( "{0}  Vertex[{1}] = (", pszPrefix, i );

                adfVertex[0] = psObject.padfX[i];
                adfVertex[1] = psObject.padfY[i];
                adfVertex[2] = psObject.padfZ[i];
                adfVertex[3] = psObject.padfM[i];
                
                EmitCoordinate( adfVertex, nDimension );
                c.printf( ")\n" );
            }
           c.printf( "{0})\n", pszPrefix );
        }

        /// <summary>
        /// Dump a tree node in a readable form.
        /// </summary>
        static void SHPTreeNodeDump( SHPTree psTree,
                                     SHPTreeNode psTreeNode,
                                     string pszPrefix,
                                     int nExpandShapes )

        {
            string  szNextPrefix = null;
            int     i;

            c.strcpy( ref szNextPrefix, pszPrefix );
            if( c.strlen(pszPrefix) < 150 - 3 )
                c.strcat( ref szNextPrefix, "  " );

            c.printf( "{0}( SHPTreeNode\n", pszPrefix );

            /* -------------------------------------------------------------------- */
            /*      Emit the bounds.                                                */
            /* -------------------------------------------------------------------- */
            c.printf( "{0}  Min = (", pszPrefix );
            EmitCoordinate( psTreeNode.adfBoundsMin, psTree.nDimension );
            c.printf( ")\n" );
            
            c.printf( "{0}  Max = (", pszPrefix );
            EmitCoordinate( psTreeNode.adfBoundsMax, psTree.nDimension );
            c.printf( ")\n" );

            /* -------------------------------------------------------------------- */
            /*      Emit the list of shapes on this node.                           */
            /* -------------------------------------------------------------------- */
            if( nExpandShapes != 0 )
            {
                c.printf( "{0}  Shapes({1}):\n", pszPrefix, psTreeNode.nShapeCount );
                for( i = 0; i < psTreeNode.nShapeCount; i++ )
                {
                    SHPObject   psObject;

                    psObject = psTree.hSHP.ReadObject( psTreeNode.panShapeIds[i] );
                    c.assert( psObject != null );
                    if( psObject != null )
                    {
                        EmitShape( psObject, szNextPrefix, psTree.nDimension );
                    }

                    psObject = null;
                }
            }
            else
            {
                c.printf( "{0}  Shapes({1}): ", pszPrefix, psTreeNode.nShapeCount );
                for( i = 0; i < psTreeNode.nShapeCount; i++ )
                {
                    c.printf( "{0} ", psTreeNode.panShapeIds[i] );
                }
                c.printf( "\n" );
            }

            /* -------------------------------------------------------------------- */
            /*      Emit subnodes.                                                  */
            /* -------------------------------------------------------------------- */
            for( i = 0; i < psTreeNode.nSubNodes; i++ )
            {
                if( psTreeNode.apsSubNode[i] != null )
                    SHPTreeNodeDump( psTree, psTreeNode.apsSubNode[i],
                                     szNextPrefix, nExpandShapes );
            }
            
            c.printf( "{0})\n", pszPrefix );

            return;
        }

        static void SHPTreeNodeSearchAndDump( SHPTree hTree,
                                              double[] padfBoundsMin,
                                              double[] padfBoundsMax )

        {
            int[]   panHits;
            int     nShapeCount, i;

            /* -------------------------------------------------------------------- */
            /*      Perform the search for likely candidates.  These are shapes     */
            /*      that fall into a tree node whose bounding box intersects our    */
            /*      area of interest.                                               */
            /* -------------------------------------------------------------------- */
            panHits = hTree.FindLikelyShapes( padfBoundsMin, padfBoundsMax,
                                               out nShapeCount );

            /* -------------------------------------------------------------------- */
            /*      Read all of these shapes, and establish whether the shape's     */
            /*      bounding box actually intersects the area of interest.  Note    */
            /*      that the bounding box could intersect the area of interest,     */
            /*      and the shape itself still not cross it but we don't try to     */
            /*      address that here.                                              */
            /* -------------------------------------------------------------------- */
            for( i = 0; i < nShapeCount; i++ )
            {
                SHPObject   psObject;

                psObject = hTree.hSHP.ReadObject( panHits[i] );
                if( psObject == null )
                    continue;

                double[]    adfMin = new double[] { psObject.dfXMin, psObject.dfYMin,
                                                    psObject.dfZMin, psObject.dfMMin };
                double[]    adfMax = new double[] { psObject.dfXMax, psObject.dfYMax,
                                                    psObject.dfZMax, psObject.dfMMax };
                if( !SHPTree.CheckBoundsOverlap( padfBoundsMin, padfBoundsMax,
                                            adfMin,
                                            adfMax,
                                            hTree.nDimension ) )
                {
                    c.printf( "Shape {0:D}: not in area of interest, but fetched.\n",
                            panHits[i] );
                }
                else
                {
                    c.printf( "Shape {0:D}: appears to be in area of interest.\n",
                            panHits[i] );
                }

                psObject = null;
            }

            if( nShapeCount == 0 )
                c.printf( "No shapes found in search.\n" );
        }
    }
}