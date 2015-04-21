using System;
using System.Text;
using System.Runtime.InteropServices;

namespace MapTools
{
	/// <summary>
	/// .NET Framework wrapper for Shapefile C Library V1.2.10
	/// </summary>
	/// <remarks>
	/// Shapefile C Library is (c) 1998 Frank Warmerdam.  .NET wrapper provided by David Gancarz.  
	/// Please send error reports or other suggestions regarding this wrapper class to:
	/// dgancarz@cfl.rr.com or david.gancarz@cityoforlando.net
	/// </remarks>
 
	public class ShapeLib
	{

		/// <summary>
		/// Shape type enumeration
		/// </summary>
		public enum ShapeType
		{
			/// <summary>Shape with no geometric data</summary>
			NullShape = 0,			
			/// <summary>2D point</summary>
			Point = 1,		
			/// <summary>2D polyline</summary>
			PolyLine = 3,			
			/// <summary>2D polygon</summary>
			Polygon = 5,		
			/// <summary>Set of 2D points</summary>
			MultiPoint = 8,	
			/// <summary>3D point</summary>
			PointZ = 11,		
			/// <summary>3D polyline</summary>
			PolyLineZ = 13,		
			/// <summary>3D polygon</summary>
			PolygonZ = 15,	
			/// <summary>Set of 3D points</summary>
			MultiPointZ = 18,	
			/// <summary>3D point with measure</summary>
			PointM = 21,		
			/// <summary>3D polyline with measure</summary>
			PolyLineM = 23,		
			/// <summary>3D polygon with measure</summary>
			PolygonM = 25,	
			/// <summary>Set of 3d points with measures</summary>
			MultiPointM = 28,	
			/// <summary>Collection of surface patches</summary>
			MultiPatch = 31
		}

		/// <summary>
		/// Part type enumeration - everything but ShapeType.MultiPatch just uses PartType.Ring.
		/// </summary>
		public enum PartType
		{
			/// <summary>
			/// Linked strip of triangles, where every vertex (after the first two) completes a new triangle.
			/// A new triangle is always formed by connecting the new vertex with its two immediate predecessors.
			/// </summary>
			TriangleStrip = 0,	
			/// <summary>
			/// A linked fan of triangles, where every vertex (after the first two) completes a new triangle.
			/// A new triangle is always formed by connecting the new vertex with its immediate predecessor 
			/// and the first vertex of the part.
			/// </summary>
			TriangleFan = 1,	
			/// <summary>The outer ring of a polygon</summary>
			OuterRing = 2,	
			/// <summary>The first ring of a polygon</summary>
			InnerRing = 3,	
			/// <summary>The outer ring of a polygon of an unspecified type</summary>
			FirstRing = 4,	
			/// <summary>A ring of a polygon of an unspecified type</summary>
			Ring = 5
		}
		
		/// <summary>
		/// SHPObject - represents on shape (without attributes) read from the .shp file.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		public class SHPObject 
		{	
			///<summary>Shape type as a ShapeType enum</summary>	
			public ShapeType shpType;	
			///<summary>Shape number (-1 is unknown/unassigned)</summary>	
			public int nShapeId;	
			///<summary>Number of parts (0 implies single part with no info)</summary>	
			public int nParts;	
			///<summary>Pointer to int array of part start offsets, of size nParts</summary>	
			public IntPtr paPartStart;
			///<summary>Pointer to PartType array (PartType.Ring if not ShapeType.MultiPatch) of size nParts</summary>	
			public IntPtr paPartType;	
			///<summary>Number of vertices</summary>	
			public int nVertices;	
			///<summary>Pointer to double array containing X coordinates</summary>	
			public IntPtr padfX;	
			///<summary>Pointer to double array containing Y coordinates</summary>		
			public IntPtr padfY;	
			///<summary>Pointer to double array containing Z coordinates (all zero if not provided)</summary>	
			public IntPtr padfZ;	
			///<summary>Pointer to double array containing Measure coordinates(all zero if not provided)</summary>	
			public IntPtr padfM;	
			///<summary>Bounding rectangle's min X</summary>	
			public double dfXMin;	
			///<summary>Bounding rectangle's min Y</summary>	
			public double dfYMin;	
			///<summary>Bounding rectangle's min Z</summary>	
			public double dfZMin;	
			///<summary>Bounding rectangle's min M</summary>	
			public double dfMMin;	
			///<summary>Bounding rectangle's max X</summary>	
			public double dfXMax;	
			///<summary>Bounding rectangle's max Y</summary>	
			public double dfYMax;	
			///<summary>Bounding rectangle's max Z</summary>	
			public double dfZMax;	
			///<summary>Bounding rectangle's max M</summary>	
			public double dfMMax;
		}

		/// <summary>
		/// The SHPOpen() function should be used to establish access to the two files for 
		/// accessing vertices (.shp and .shx). Note that both files have to be in the indicated 
		/// directory, and must have the expected extensions in lower case. The returned SHPHandle 
		/// is passed to other access functions, and SHPClose() should be invoked to recover 
		/// resources, and flush changes to disk when complete.
		/// </summary>
		/// <param name="szShapeFile">The name of the layer to access.  This can be the name of either 
		/// the .shp or the .shx file or can just be the path plus the basename of the pair.</param>
		/// <param name="szAccess">The fopen() style access string. At this time only "rb" (read-only binary) 
		/// and "rb+" (read/write binary) should be used.</param>
		/// <returns>IntPtr</returns>
		[DllImport("shapelib.dll", CharSet=CharSet.Ansi)]
		public static extern IntPtr SHPOpen(string szShapeFile, string szAccess);

		/// <summary>
		/// The SHPCreate() function will create a new .shp and .shx file of the desired type.
		/// </summary>
		/// <param name="szShapeFile">The name of the layer to access. This can be the name of either 
		/// the .shp or the .shx file or can just be the path plus the basename of the pair.</param>
		/// <param name="shpType">The type of shapes to be stored in the newly created file. 
		/// It may be either ShapeType.Point, ShapeType.PolyLine, ShapeType.Polygon or ShapeType.MultiPoint.</param>
		/// <returns>IntPtr</returns>
		[DllImport("shapelib.dll", CharSet=CharSet.Ansi)]
		public static extern IntPtr SHPCreate(string szShapeFile, ShapeType shpType);

		/// <summary>
		/// The SHPGetInfo() function retrieves various information about shapefile as a whole. 
		/// The bounds are read from the file header, and may be inaccurate if the file was 
		/// improperly generated.
		/// </summary>
		/// <param name="hSHP">The handle previously returned by SHPOpen() or SHPCreate()</param>
		/// <param name="pnEntities">A pointer to an integer into which the number of 
		/// entities/structures should be placed. May be NULL.</param>
		/// <param name="pshpType">A pointer to an integer into which the ShapeType of this file 
		/// should be placed. Shapefiles may contain either ShapeType.Point, ShapeType.PolyLine, ShapeType.Polygon or 
		/// ShapeType.MultiPoint entities. This may be NULL.</param>
		/// <param name="adfMinBound">The X, Y, Z and M minimum values will be placed into this 
		/// four entry array. This may be NULL. </param>
		/// <param name="adfMaxBound">The X, Y, Z and M maximum values will be placed into this 
		/// four entry array. This may be NULL.</param>
		/// <returns>void</returns>
		[DllImport("shapelib.dll", CharSet=CharSet.Ansi)]
		public static extern void SHPGetInfo(IntPtr hSHP, ref int pnEntities, 
			ref ShapeType pshpType,  double[] adfMinBound, double[] adfMaxBound);

		/// <summary>
		/// The SHPReadObject() call is used to read a single structure, or entity from the shapefile. 
		/// See the definition of the SHPObject structure for detailed information on fields of a SHPObject. 
		/// </summary>
		/// <param name="hSHP">The handle previously returned by SHPOpen() or SHPCreate().</param>
		/// <param name="iShape">The entity number of the shape to read. Entity numbers are between 0 
		/// and nEntities-1 (as returned by SHPGetInfo()).</param>
		/// <returns>SHPObject</returns>
		/// <remarks>
		/// SHPObject's returned from SHPReadObject() should be deallocated with SHPDestroyShape(). 
		/// SHPReadObject() will return NULL if an illegal iShape value is requested. 
		/// Note that the bounds placed into the SHPObject are those read from the file, and may not be correct. 
		/// For points the bounds are generated from the single point since bounds aren't normally provided 
		/// for point types. Generally the shapes returned will be of the type of the file as a whole. 
		/// However, any file may also contain type ShapeType.NullShape shapes which will have no geometry. 
		/// Generally speaking applications should skip rather than preserve them, as they usually 
		/// represented interactively deleted shapes.
		/// </remarks>
		[DllImport("shapelib.dll", CharSet=CharSet.Ansi)]
		public static extern SHPObject SHPReadObject(IntPtr hSHP, int iShape);

		/// <summary>
		/// The SHPWriteObject() call is used to write a single structure, or entity to the shapefile. 
		/// See the definition of the SHPObject structure for detailed information on fields of a SHPObject.
		/// </summary>
		/// <param name="hSHP">The handle previously returned by SHPOpen("r+") or SHPCreate().</param>
		/// <param name="iShape">The entity number of the shape to write. 
		/// A value of -1 should be used for new shapes. </param>
		/// <param name="psObject">The shape to write to the file. This should have been created with SHPCreateObject(), 
		/// or SHPCreateSimpleObject().</param>
		/// <returns>int</returns>
		[DllImport("shapelib.dll", CharSet=CharSet.Ansi)]
		public static extern int SHPWriteObject(IntPtr hSHP, int iShape, SHPObject psObject);

		/// <summary>
		/// This function should be used to deallocate the resources associated with a SHPObject 
		/// when it is no longer needed, including those created with SHPCreateSimpleObject(), 
		/// SHPCreateObject() and returned from SHPReadObject().
		/// </summary>
		/// <param name="psObject">The object to deallocate.</param>
		/// <returns>void</returns>
		[DllImport("shapelib.dll", CharSet=CharSet.Ansi)]
		public static extern void SHPDestroyObject(SHPObject psObject);

		/// <summary>
		/// This function will recompute the extents of this shape, replacing the existing values 
		/// of the dfXMin, dfYMin, dfZMin, dfMMin, dfXMax, dfYMax, dfZMax, and dfMMax values based 
		/// on the current set of vertices for the shape. This function is automatically called by 
		/// SHPCreateObject() but if the vertices of an existing object are altered it should be 
		/// called again to fix up the extents.
		/// </summary>
		/// <param name="psObject">An existing shape object to be updated in place.</param>
		/// <returns>void</returns>
		[DllImport("shapelib.dll", CharSet=CharSet.Ansi)]
		public static extern void SHPComputeExtents(SHPObject psObject);

		/// <summary>
		/// The SHPCreateObject() function allows for the creation of objects (shapes). 
		/// This is normally used so that the SHPObject can be passed to SHPWriteObject() 
		/// to write it to the file.
		/// </summary>
		/// <param name="shpType">The ShapeType of the object to be created, such as ShapeType.Point, or ShapeType.Polygon.</param>
		/// <param name="nShapeId">The shapeid to be recorded with this shape.</param>
		/// <param name="nParts">The number of parts for this object. If this is zero for PolyLine, 
		/// or Polygon type objects, a single zero valued part will be created internally.</param>
		/// <param name="panPartStart">The list of zero based start vertices for the rings 
		/// (parts) in this object. The first should always be zero. This may be NULL if nParts is 0.</param>
		/// <param name="paPartType">The type of each of the parts. This is only meaningful for MultiPatch files. 
		/// For all other cases this may be NULL, and will be assumed to be PartType.Ring.</param>
		/// <param name="nVertices">The number of vertices being passed in padfX, padfY, and padfZ. </param>
		/// <param name="adfX">An array of nVertices X coordinates of the vertices for this object.</param>
		/// <param name="adfY">An array of nVertices Y coordinates of the vertices for this object.</param>
		/// <param name="adfZ">An array of nVertices Z coordinates of the vertices for this object. 
		/// This may be NULL in which case they are all assumed to be zero.</param>
		/// <param name="adfM">An array of nVertices M (measure values) of the vertices for this object. 
		/// This may be NULL in which case they are all assumed to be zero.</param>
		/// <returns>SHPObject</returns>
		/// <remarks>
		/// The SHPDestroyObject() function should be used to free 
		/// resources associated with an object allocated with SHPCreateObject(). This function 
		/// computes a bounding box for the SHPObject from the given vertices.
		/// </remarks>
		[DllImport("shapelib.dll", CharSet=CharSet.Ansi)]
		public static extern SHPObject SHPCreateObject(ShapeType shpType, int nShapeId,
			int nParts, int[] panPartStart, PartType[] paPartType,
			int nVertices, double[] adfX, double[] adfY,
			double[] adfZ, double[] adfM );

		/// <summary>
		/// The SHPCreateSimpleObject() function allows for the convenient creation of simple objects. 
		/// This is normally used so that the SHPObject can be passed to SHPWriteObject() to write it 
		/// to the file. The simple object creation API assumes an M (measure) value of zero for each vertex. 
		/// For complex objects (such as polygons) it is assumed that there is only one part, and that it 
		/// is of the default type (PartType.Ring). Use the SHPCreateObject() function for more sophisticated 
		/// objects. 
		/// </summary>
		/// <param name="shpType">The ShapeType of the object to be created, such as ShapeType.Point, or ShapeType.Polygon.</param>
		/// <param name="nVertices">The number of vertices being passed in padfX, padfY, and padfZ.</param>
		/// <param name="adfX">An array of nVertices X coordinates of the vertices for this object.</param>
		/// <param name="adfY">An array of nVertices Y coordinates of the vertices for this object.</param>
		/// <param name="adfZ">An array of nVertices Z coordinates of the vertices for this object. 
		/// This may be NULL in which case they are all assumed to be zero.</param>
		/// <returns>SHPObject</returns>
		/// <remarks>
		/// The SHPDestroyObject() function should be used to free resources associated with an 
		/// object allocated with SHPCreateSimpleObject().
		/// </remarks>
		[DllImport("shapelib.dll", CharSet=CharSet.Ansi)]
		public static extern SHPObject SHPCreateSimpleObject(ShapeType shpType, int nVertices, 
			double[] adfX, double[] adfY, double[] adfZ);

		/// <summary>
		/// The SHPClose() function will close the .shp and .shx files, and flush all outstanding header 
		/// information to the files. It will also recover resources associated with the handle. 
		/// After this call the hSHP handle cannot be used again.
		/// </summary>
		/// <param name="hSHP">The handle previously returned by SHPOpen() or SHPCreate().</param>
		/// <returns>void</returns>
		[DllImport("shapelib.dll", CharSet=CharSet.Ansi)]
		public static extern void SHPClose(IntPtr hSHP);

		/// <summary>
		/// Translates a ShapeType.* constant into a named shape type (Point, PointZ, Polygon, etc.)
		/// </summary>
		/// <param name="shpType">ShapeType enum</param>
		/// <returns>string</returns>
		[DllImport("shapelib.dll", CharSet=CharSet.Ansi)]
		public static extern string SHPTypeName(ShapeType shpType);

		/// <summary>
		/// Translates a PartType enum into a named part type (Ring, Inner Ring, etc.)
		/// </summary>
		/// <param name="partType">PartType enum</param>
		/// <returns>string</returns>
		[DllImport("shapelib.dll", CharSet=CharSet.Ansi)]
		public static extern string SHPPartTypeName (PartType partType);

		/* -------------------------------------------------------------------- */
		/*      Shape quadtree indexing API.                                    */
		/* -------------------------------------------------------------------- */

		/// <summary>
		/// Creates a quadtree index
		/// </summary>
		/// <param name="hSHP"></param>
		/// <param name="nDimension"></param>
		/// <param name="nMaxDepth"></param>
		/// <param name="adfBoundsMin"></param>
		/// <param name="adfBoundsMax"></param>
		/// <returns>IntPtr</returns>
		[DllImport("shapelib.dll", CharSet=CharSet.Ansi)]
		public static extern IntPtr SHPCreateTree(IntPtr hSHP, int nDimension, int nMaxDepth, 
			double[] adfBoundsMin, double[] adfBoundsMax);

		/// <summary>
		/// Releases resources associated with quadtree
		/// </summary>
		/// <param name="hTree"></param>
		/// <returns>void</returns>
		[DllImport("shapelib.dll", CharSet=CharSet.Ansi)]
		public static extern void SHPDestroyTree(IntPtr hTree);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="hTree"></param>
		/// <param name="psObject"></param>
		/// <returns>int</returns>
		[DllImport("shapelib.dll", CharSet=CharSet.Ansi)]
		public static extern int SHPTreeAddShapeId(IntPtr hTree, SHPObject psObject);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="hTree"></param>
		/// <returns>void</returns>
		[DllImport("shapelib.dll", CharSet=CharSet.Ansi)]
		public static extern void SHPTreeTrimExtraNodes(IntPtr hTree);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="hTree"></param>
		/// <param name="adfBoundsMin"></param>
		/// <param name="adfBoundsMax"></param>
		/// <param name="pnShapeCount"></param>
		/// <returns>IntPtr</returns>
		[DllImport("shapelib.dll", CharSet=CharSet.Ansi)]
		public static extern IntPtr SHPTreeFindLikelyShapes(IntPtr hTree,	
			double[] adfBoundsMin, double[] adfBoundsMax, ref int pnShapeCount);
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="adfBox1Min"></param>
		/// <param name="adfBox1Max"></param>
		/// <param name="adfBox2Min"></param>
		/// <param name="adfBox2Max"></param>
		/// <param name="nDimension"></param>
		/// <returns>int</returns>
		[DllImport("shapelib.dll", CharSet=CharSet.Ansi)]
		public static extern int SHPCheckBoundsOverlap(double[] adfBox1Min, double[] adfBox1Max, 
			double[] adfBox2Min, double[] adfBox2Max, int nDimension);

		/// <summary>
		/// xBase field type enumeration
		/// </summary>
		public enum DBFFieldType 
		{	
			///<summary>String data type</summary> 
			FTString,	
			///<summary>Integer data type</summary>
			FTInteger,	
			///<summary>Double data type</summary> 
			FTDouble,	
			///<summary>Logical data type</summary>
			FTLogical,	
			///<summary>Invalid data type</summary>
			FTInvalid	
		};

		/// <summary>
		/// The DBFOpen() function should be used to establish access to an existing xBase format table file. 
		/// The returned DBFHandle is passed to other access functions, and DBFClose() should be invoked 
		/// to recover resources, and flush changes to disk when complete. The DBFCreate() function should 
		/// called to create new xBase files. As a convenience, DBFOpen() can be called with the name of a 
		/// .shp or .shx file, and it will figure out the name of the related .dbf file.
		/// </summary>
		/// <param name="szDBFFile">The name of the xBase (.dbf) file to access.</param>
		/// <param name="szAccess">The fopen() style access string. At this time only "rb" (read-only binary) 
		/// and "rb+" (read/write binary) should be used.</param>
		/// <returns>IntPtr</returns>
		[DllImport("shapelib.dll", CharSet=CharSet.Ansi)]
		public static extern IntPtr DBFOpen (string szDBFFile, string szAccess);

		/// <summary>
		/// The DBFCreate() function creates a new xBase format file with the given name, 
		/// and returns an access handle that can be used with other DBF functions. 
		/// The newly created file will have no fields, and no records. 
		/// Fields should be added with DBFAddField() before any records add written. 
		/// </summary>
		/// <param name="szDBFFile">The name of the xBase (.dbf) file to create.</param>
		/// <returns>IntPtr</returns>
		[DllImport("shapelib.dll", CharSet=CharSet.Ansi)]
		public static extern IntPtr DBFCreate (string szDBFFile);

		/// <summary>
		/// The DBFGetFieldCount() function returns the number of fields currently defined 
		/// for the indicated xBase file. 
		/// </summary>
		/// <param name="hDBF">The access handle for the file to be queried, as returned by 
		/// DBFOpen(), or DBFCreate().</param>
		/// <returns>int</returns>
		[DllImport("shapelib.dll", CharSet=CharSet.Ansi)]
		public static extern int DBFGetFieldCount (IntPtr hDBF);

		/// <summary>
		/// The DBFGetRecordCount() function returns the number of records that exist on the xBase 
		/// file currently. Note that with shape files one xBase record exists for each shape in the 
		/// .shp/.shx files.
		/// </summary>
		/// <param name="hDBF">The access handle for the file to be queried, as returned by 
		/// DBFOpen(), or DBFCreate().</param>
		/// <returns>int</returns>
		[DllImport("shapelib.dll", CharSet=CharSet.Ansi)]
		public static extern int DBFGetRecordCount (IntPtr hDBF);

		/// <summary>
		/// The DBFAddField() function is used to add new fields to an existing xBase file opened with DBFOpen(), 
		/// or created with DBFCreate(). Note that fields can only be added to xBase files with no records, 
		/// though this is limitation of this API, not of the file format. Returns the field number of the 
		/// new field, or -1 if the addition of the field failed
		/// </summary>
		/// <param name="hDBF">The access handle for the file to be updated, as returned by DBFOpen(), 
		/// or DBFCreate().</param>
		/// <param name="szFieldName">The name of the new field. At most 11 character will be used. 
		/// In order to use the xBase file in some packages it may be necessary to avoid some special 
		/// characters in the field names such as spaces, or arithmetic operators.</param>
		/// <param name="eType">One of FTString, FTInteger, FTLogical, or FTDouble in order to establish the 
		/// type of the new field. Note that some valid xBase field types cannot be created such as date fields.</param>
		/// <param name="nWidth">The width of the field to be created. For FTString fields this establishes 
		/// the maximum length of string that can be stored. For FTInteger this establishes the number of 
		/// digits of the largest number that can be represented. For FTDouble fields this in combination 
		/// with the nDecimals value establish the size, and precision of the created field.</param>
		/// <param name="nDecimals">The number of decimal places to reserve for FTDouble fields. 
		/// For all other field types this should be zero. For instance with nWidth=7, and nDecimals=3 
		/// numbers would be formatted similarly to `123.456'.</param>
		/// <returns>int</returns>
		[DllImport("shapelib.dll", CharSet=CharSet.Ansi)]
		public static extern int DBFAddField (IntPtr hDBF, string szFieldName, 
			DBFFieldType eType, int nWidth, int nDecimals);

		/// <summary>
		/// The DBFGetFieldInfo() returns the type of the requested field, which is one of the DBFFieldType 
		/// enumerated values. As well, the field name, and field width information can optionally be returned. 
		/// The field type returned does not correspond one to one with the xBase field types. 
		/// For instance the xBase field type for Date will just be returned as being FTInteger. 
		/// </summary>
		/// <param name="hDBF">The access handle for the file to be queried, as returned by DBFOpen(), 
		/// or DBFCreate().</param>
		/// <param name="iField">The field to be queried. This should be a number between 0 and n-1, 
		/// where n is the number fields on the file, as returned by DBFGetFieldCount().</param>
		/// <param name="szFieldName">If this pointer is not NULL the name of the requested field 
		/// will be written to this location. The pszFieldName buffer should be at least 12 character 
		/// is size in order to hold the longest possible field name of 11 characters plus a terminating 
		/// zero character.</param>
		/// <param name="pnWidth">If this pointer is not NULL, the width of the requested field will be 
		/// returned in the int pointed to by pnWidth. This is the width in characters. </param>
		/// <param name="pnDecimals">If this pointer is not NULL, the number of decimal places precision 
		/// defined for the field will be returned. This is zero for integer fields, or non-numeric fields.</param>
		/// <returns>DBFFieldType</returns>
		[DllImport("shapelib.dll", CharSet=CharSet.Ansi)]
		public static extern DBFFieldType DBFGetFieldInfo (IntPtr hDBF, int iField, 
			StringBuilder szFieldName, ref int pnWidth, ref int pnDecimals);

		/// <summary>
		/// Returns the index of the field matching this name, or -1 on failure. 
		/// The comparison is case insensitive. However, lengths must match exactly.
		/// </summary>
		/// <param name="hDBF">The access handle for the file to be queried, as returned 
		/// by DBFOpen(), or DBFCreate().</param>
		/// <param name="szFieldName">Name of the field to search for.</param>
		/// <returns>int</returns>
		[DllImport("shapelib.dll", CharSet=CharSet.Ansi)]
		public static extern int DBFGetFieldIndex (IntPtr hDBF, string szFieldName);

		/// <summary>
		/// The DBFReadIntegerAttribute() will read the value of one field and return it as an integer. 
		/// </summary>
		/// <param name="hDBF">The access handle for the file to be queried, as returned by 
		/// DBFOpen(), or DBFCreate().</param>
		/// <param name="iShape">The record number (shape number) from which the field value should be read.</param>
		/// <param name="iField">The field within the selected record that should be read.</param>
		/// <returns>int</returns>
		/// <remarks>
		/// This can be used even with FTString fields, though the returned value will be zero if not 
		/// interpretable as a number.
		/// </remarks>
		[DllImport("shapelib.dll", CharSet=CharSet.Ansi)]
		public static extern int DBFReadIntegerAttribute (IntPtr hDBF, int iShape, int iField);

		/// <summary>
		/// The DBFReadDoubleAttribute() will read the value of one field and return it as a double. 
		/// </summary>
		/// <param name="hDBF">The access handle for the file to be queried, as returned by 
		/// DBFOpen(), or DBFCreate().</param>
		/// <param name="iShape">The record number (shape number) from which the field value should be read.</param>
		/// <param name="iField">The field within the selected record that should be read.</param>
		/// <returns>double</returns>
		/// <remarks>
		/// This can be used even with FTString fields, though the returned value will be zero if not 
		/// interpretable as a number.
		/// </remarks>
		[DllImport("shapelib.dll", CharSet=CharSet.Ansi)]
		public static extern double DBFReadDoubleAttribute (IntPtr hDBF, int iShape, int iField);

		/// <summary>
		/// The DBFReadStringAttribute() will read the value of one field and return it as a string. 
		/// </summary>
		/// <param name="hDBF">The access handle for the file to be queried, as returned by 
		/// DBFOpen(), or DBFCreate().</param>
		/// <param name="iShape">The record number (shape number) from which the field value should be read.</param>
		/// <param name="iField">The field within the selected record that should be read.</param>
		/// <returns>string</returns>
		/// <remarks>
		/// This function may be used on any field type (including FTInteger and FTDouble) and will 
		/// return the string representation stored in the .dbf file. The returned pointer is to an 
		/// internal buffer which is only valid untill the next DBF function call. It's contents may 
		/// be copied with normal string functions such as strcpy(), or strdup(). If the 
		/// TRIM_DBF_WHITESPACE macro is defined in shapefil.h (it is by default) then all leading and 
		/// trailing space (ASCII 32) characters will be stripped before the string is returned.
		/// </remarks>
		[DllImport("shapelib.dll", CharSet=CharSet.Ansi)]
		public static extern string DBFReadStringAttribute (IntPtr hDBF, int iShape, int iField);

		[DllImport("shapelib.dll", CharSet=CharSet.Ansi, EntryPoint="DBFReadLogicalAttribute")]
		private static extern string _DBFReadLogicalAttribute (IntPtr hDBF, int iShape, int iField);
		/// <summary>
		/// The DBFReadLogicalAttribute() will read the value of one field and return it as a boolean. 
		/// </summary>
		/// <param name="hDBF">The access handle for the file to be queried, as returned by 
		/// DBFOpen(), or DBFCreate().</param>
		/// <param name="iShape">The record number (shape number) from which the field value should be read.</param>
		/// <param name="iField">The field within the selected record that should be read.</param>
		/// <returns>bool</returns>
		/// <remarks>
		/// This can be used with FTString fields, in which case it returns TRUE if the string="T";
		/// otherwise it returns FALSE.
		/// </remarks>
		public static bool DBFReadLogicalAttribute (IntPtr hDBF, int iShape, int iField)
		{
			return (_DBFReadLogicalAttribute(hDBF, iShape, iField)=="T");
		}

		/// <summary>
		/// This function will return TRUE if the indicated field is NULL valued otherwise FALSE. 
		/// </summary>
		/// <param name="hDBF">The access handle for the file to be queried, as returned by 
		/// DBFOpen(), or DBFCreate().</param>
		/// <param name="iShape">The record number (shape number) from which the field value should be read.</param>
		/// <param name="iField">The field within the selected record that should be read.</param>
		/// <returns>int</returns>
		/// <remarks>
		/// Note that NULL fields are represented in the .dbf file as having all spaces in the field. 
		/// Reading NULL fields will result in a value of 0.0 or an empty string with the other 
		/// DBFRead*Attribute() functions.
		/// </remarks>
		[DllImport("shapelib.dll", CharSet=CharSet.Ansi)]
		public static extern int DBFIsAttributeNULL (IntPtr hDBF, int iShape, int iField);

		/// <summary>
		/// The DBFWriteIntegerAttribute() function is used to write a value to a numeric field 
		/// (FTInteger, or FTDouble). If the write succeeds the value TRUE will be returned, 
		/// otherwise FALSE will be returned. If the value is too large to fit in the field, 
		/// it will be truncated and FALSE returned.
		/// </summary>
		/// <param name="hDBF">The access handle for the file to be written, as returned by 
		/// DBFOpen(), or DBFCreate().</param>
		/// <param name="iShape">The record number (shape number) to which the field value should be written.</param>
		/// <param name="iField">The field within the selected record that should be written.</param>
		/// <param name="nFieldValue">The integer value that should be written.</param>
		/// <returns>int</returns>
		[DllImport("shapelib.dll", CharSet=CharSet.Ansi)]
		public static extern int DBFWriteIntegerAttribute (IntPtr hDBF, int iShape, 
			int iField, int nFieldValue);

		/// <summary>
		/// The DBFWriteDoubleAttribute() function is used to write a value to a numeric field 
		/// (FTInteger, or FTDouble). If the write succeeds the value TRUE will be returned, 
		/// otherwise FALSE will be returned. If the value is too large to fit in the field, 
		/// it will be truncated and FALSE returned.
		/// </summary>
		/// <param name="hDBF">The access handle for the file to be written, as returned by 
		/// DBFOpen(), or DBFCreate().</param>
		/// <param name="iShape">The record number (shape number) to which the field value should be written.</param>
		/// <param name="iField">The field within the selected record that should be written.</param>
		/// <param name="dFieldValue">The floating point value that should be written.</param>
		/// <returns>int</returns>
		[DllImport("shapelib.dll", CharSet=CharSet.Ansi)]
		public static extern int DBFWriteDoubleAttribute (IntPtr hDBF, int iShape, 
			int iField, double dFieldValue);

		/// <summary>
		/// The DBFWriteStringAttribute() function is used to write a value to a string field (FString). 
		/// If the write succeeds the value TRUE willbe returned, otherwise FALSE will be returned. 
		/// If the value is too large to fit in the field, it will be truncated and FALSE returned.
		/// </summary>
		/// <param name="hDBF">The access handle for the file to be written, as returned by 
		/// DBFOpen(), or DBFCreate().</param>
		/// <param name="iShape">The record number (shape number) to which the field value should be written.</param>
		/// <param name="iField">The field within the selected record that should be written.</param>
		/// <param name="szFieldValue">The string to be written to the field.</param>
		/// <returns>int</returns>
		[DllImport("shapelib.dll", CharSet=CharSet.Ansi)]
		public static extern int DBFWriteStringAttribute (IntPtr hDBF, int iShape, 
			int iField, string szFieldValue);

		/// <summary>
		/// The DBFWriteNULLAttribute() function is used to clear the indicated field to a NULL value. 
		/// In the .dbf file this is represented by setting the entire field to spaces. If the write 
		/// succeeds the value TRUE will be returned, otherwise FALSE will be returned.
		/// </summary>
		/// <param name="hDBF">The access handle for the file to be written, as returned by 
		/// DBFOpen(), or DBFCreate().</param>
		/// <param name="iShape">The record number (shape number) to which the field value should be written.</param>
		/// <param name="iField">The field within the selected record that should be written.</param>
		/// <returns>int</returns>
		[DllImport("shapelib.dll", CharSet=CharSet.Ansi)]
		public static extern int DBFWriteNULLAttribute (IntPtr hDBF, int iShape, int iField);

		[DllImport("shapelib.dll", CharSet=CharSet.Ansi, EntryPoint="DBFWriteLogicalAttribute")]
		private static extern int _DBFWriteLogicalAttribute (IntPtr hDBF, int iShape, 
			int iField, char lFieldValue);
		/// <summary>
		/// The DBFWriteLogicalAttribute() function is used to write a boolean value to a logical field 
		/// (FTLogical). If the write succeeds the value TRUE will be returned, 
		/// otherwise FALSE will be returned.
		/// </summary>
		/// <param name="hDBF">The access handle for the file to be written, as returned by 
		/// DBFOpen(), or DBFCreate().</param>
		/// <param name="iShape">The record number (shape number) to which the field value should be written.</param>
		/// <param name="iField">The field within the selected record that should be written.</param>
		/// <param name="bFieldValue">The boolean value to be written to the field.</param>
		/// <returns>int</returns>
		public static int DBFWriteLogicalAttribute (IntPtr hDBF, int iShape, int iField, bool bFieldValue)
		{
			if (bFieldValue)
				return _DBFWriteLogicalAttribute(hDBF, iShape, iField, 'T');
			else
				return _DBFWriteLogicalAttribute(hDBF, iShape, iField, 'F');
		}

		/// <summary>
		/// Reads the attribute fields of a record.
		/// </summary>
		/// <param name="hDBF">The access handle for the file to be queried, as returned by 
		/// DBFOpen(), or DBFCreate().</param>
		/// <param name="hEntity">The entity (record) number to be read</param>
		/// <returns>IntPtr</returns>
		[DllImport("shapelib.dll", CharSet=CharSet.Ansi)]
		public static extern IntPtr DBFReadTuple (IntPtr hDBF, int hEntity);

		/// <summary>
		/// Writes an attribute record to the file.
		/// </summary>
		/// <param name="hDBF">The access handle for the file to be queried, as returned by 
		/// DBFOpen(), or DBFCreate().</param>
		/// <param name="hEntity">The zero-based entity (record) number to be written.  If hEntity equals 
		/// the number of records a new record is appended.</param>
		/// <param name="pRawTuple">Pointer to the tuple to be written</param>
		/// <returns>int</returns>
		[DllImport("shapelib.dll", CharSet=CharSet.Ansi)]
		public static extern int DBFWriteTuple (IntPtr hDBF, int hEntity, IntPtr pRawTuple);

		/// <summary>
		/// Copies the data structure of an xBase file to another xBase file.  
		/// Data are not copied.  Use Read/WriteTuple functions to selectively copy data.
		/// </summary>
		/// <param name="hDBF">The access handle for the file to be queried, as returned by 
		/// DBFOpen(), or DBFCreate().</param>
		/// <param name="szFilename">The name of the xBase (.dbf) file to create.</param>
		/// <returns>IntPtr</returns>
		[DllImport("shapelib.dll", CharSet=CharSet.Ansi)]
		public static extern IntPtr DBFCloneEmpty (IntPtr hDBF, string szFilename);

		/// <summary>
		/// The DBFClose() function will close the indicated xBase file (opened with DBFOpen(), 
		/// or DBFCreate()), flushing out all information to the file on disk, and recovering 
		/// any resources associated with having the file open. The file handle (hDBF) should not 
		/// be used again with the DBF API after calling DBFClose().
		/// </summary>
		/// <param name="hDBF">The access handle for the file to be closed.</param>
		/// <returns>void</returns>
		[DllImport("shapelib.dll", CharSet=CharSet.Ansi)]
		public static extern void DBFClose (IntPtr hDBF);

		
		/// <summary>
		/// This function returns the DBF type code of the indicated field.
		/// </summary>
		/// <param name="hDBF">The access handle for the file.</param>
		/// <param name="iField">The field index to query.</param>
		/// <returns>sbyte</returns>
		/// <remarks>
		/// Return value will be one of:
		/// <list type="bullet">
		/// <item><term>C</term><description>String</description></item>
		/// <item><term>D</term><description>Date</description></item>
		/// <item><term>F</term><description>Float</description></item>
		/// <item><term>N</term><description>Numeric, with or without decimal</description></item>
		/// <item><term>L</term><description>Logical</description></item>
		/// <item><term>M</term><description>Memo: 10 digits .DBT block ptr</description></item>
		/// <item><term> </term><description>field out of range</description></item>
		/// </list>
		/// </remarks>
		[DllImport("shapelib.dll", CharSet=CharSet.Ansi)]
		public static extern sbyte DBFGetNativeFieldType (IntPtr hDBF, int iField);

		/// <summary>
		/// private constructor:  no instantiation needed or permitted
		/// </summary>
		private ShapeLib(){} 
	}
}
