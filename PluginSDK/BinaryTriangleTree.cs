using System;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace WorldWind
{
	/// <summary>
	/// Summary description for BinaryTriangleTree.
	/// </summary>
	// BinaryTriangleTree class - handle mesh optimization (PM)
	public class BinaryTriangleTree
	{
		CustomVertex.PositionTextured[] _elevatedVertices;
		int _vertexDensity;
		int _verticeDensity;
		int _margin;
		double _layerRadius;
		int _maxVariance;
		BinaryTriangle[] _treeList;
		int _treeListLength;
		int _terrainFaceCount;
		short nw, sw, ne, se;

		public int TriangleCount;
		public short[] Indices;
		
		public BinaryTriangleTree(
			CustomVertex.PositionTextured[] ElevatedVertices, 
			int VertexDensity,  // must be a power of 2
			int Margin, 
			double LayerRadius)
		{
			this._elevatedVertices = ElevatedVertices;
			this._vertexDensity = VertexDensity;
			this._margin = Margin;
			this._layerRadius = LayerRadius;
			this._verticeDensity = this._margin + this._vertexDensity + this._margin + 1;
			// indices to four corners vertices of terrain (inside walls/margin)
			this.nw = (short)((this._margin * this._verticeDensity) + this._margin);
			this.sw = (short)(nw + (short)(this._vertexDensity * this._verticeDensity));
			this.ne = (short)(nw + (short)(this._vertexDensity));
			this.se = (short)(sw + (short)(this._vertexDensity));
			
		}

		public void BuildTree(int MaxVariance)
		{
			this._maxVariance = MaxVariance;
			this._treeList = new BinaryTriangle[this._vertexDensity * this._vertexDensity * 8]; // find real formula ! sum(Math.Pow(2,x), x=1, x=n+1)
			// init tree with two big triangles (middle vertice is right corner)
			this._treeList[0] = new BinaryTriangle(this.ne, this.nw, this.sw);
			this._treeList[1] = new BinaryTriangle(this.sw, this.se, this.ne);
			this._treeList[0].bn = 1; // cross link bottom neighbours
			this._treeList[1].bn = 0;
			this._treeListLength = 2;
			// dig it!
			BuildFace(0);
			BuildFace(1);
			this._terrainFaceCount = FaceCount();
		}

		private void BuildFace(short f)
		{
			if(this._treeList[f].lc != -1) // face already has childs
			{
				BuildFace(this._treeList[f].lc); 
				BuildFace(this._treeList[f].rc);
			}
			else
			{
				if(FaceVariance(this._treeList[f].i1, this._treeList[f].i2, this._treeList[f].i3) > this._maxVariance)
				{	// if face not close enough to terrain, split it and dig
					SplitFace(f);
					BuildFace(this._treeList[f].lc); 
					BuildFace(this._treeList[f].rc);
				}
			}
		}

		private void SplitFace(short f) 
		{
			if(this._treeList[f].bn != -1)		// Check bottom neighbour first
			{
				if(this._treeList[this._treeList[f].bn].bn != f) // if not symetric (same level)
				{
					SplitFace(this._treeList[f].bn);	// split bottom neighbour
				}
				SplitFaceForReal(f);					// now split face and bot. neighbour
				SplitFaceForReal(this._treeList[f].bn);
				this._treeList[this._treeList[f].lc].rn = this._treeList[this._treeList[f].bn].rc; // cross link diamond
				this._treeList[this._treeList[f].rc].ln = this._treeList[this._treeList[f].bn].lc;
				this._treeList[this._treeList[this._treeList[f].bn].lc].rn = this._treeList[f].rc;
				this._treeList[this._treeList[this._treeList[f].bn].rc].ln = this._treeList[f].lc;
			}
			else								// no bottom neighbour, no diamond to take care of
			{
				SplitFaceForReal(f);
			}
		}

		private void SplitFaceForReal(short f)
		{
			// find vertice in middle of hypothenuse
			short mid_hyp_indice = MidHypVerticeIndice(this._treeList[f].i1, this._treeList[f].i3);
			// Create two child faces
			this._treeList[this._treeListLength] = new BinaryTriangle(this._treeList[f].i2, mid_hyp_indice, this._treeList[f].i1); // right child
			this._treeList[this._treeListLength + 1] = new BinaryTriangle(this._treeList[f].i3, mid_hyp_indice, this._treeList[f].i2); // left child
			this._treeList[f].rc = (short)this._treeListLength;
			this._treeList[f].lc = (short)(this._treeListLength + 1);
			this._treeListLength += 2;
			this._treeList[this._treeList[f].lc].ln = this._treeList[f].rc; // lc.ln -> rc
			this._treeList[this._treeList[f].rc].rn = this._treeList[f].lc; // rc.rn -> lc
			// left neighbour links
			this._treeList[this._treeList[f].lc].bn = this._treeList[f].ln;
			if(this._treeList[f].ln != -1)
			{
				if(this._treeList[this._treeList[f].ln].bn == f)
				{
					this._treeList[this._treeList[f].ln].bn = this._treeList[f].lc;
				}
				else
				{
					if(this._treeList[this._treeList[f].ln].ln == f)
					{
						this._treeList[this._treeList[f].ln].ln = this._treeList[f].lc;
					}
					else
					{
						this._treeList[this._treeList[f].ln].rn = this._treeList[f].lc;
					}
				}
			}
			// right neighbour links
			this._treeList[this._treeList[f].rc].bn = this._treeList[f].rn;
			if(this._treeList[f].rn != -1)
			{
				if(this._treeList[this._treeList[f].rn].bn == f)
				{
					this._treeList[this._treeList[f].rn].bn = this._treeList[f].rc;
				}
				else
				{
					if(this._treeList[this._treeList[f].rn].rn == f)
					{
						this._treeList[this._treeList[f].rn].rn = this._treeList[f].rc;
					}
					else
					{
						this._treeList[this._treeList[f].rn].ln = this._treeList[f].rc;
					}
				}
			}
		}

		private short MidHypVerticeIndice(short v1, short v3)
		{
			// find vertice in middle of hypothenuse - midway i1 and i3
			short i1 = (short)Math.Floor((float)(v1 / this._verticeDensity));
			short j1 = (short)(v1 % (short)this._verticeDensity);
			short i3 = (short)Math.Floor((float)(v3 / this._verticeDensity));
			short j3 = (short)(v3 % (short)this._verticeDensity);
			short ih = (short)(i1 + ((i3 - i1) / 2));
			short jh = (short)(j1 + ((j3 - j1) / 2));
			short mid_hyp_indice = (short)((ih * (short)this._verticeDensity) + jh);
			return mid_hyp_indice;
		}

		private int FaceCount() 
		{
			int tot = 0;
			for(short i = 0; i < this._treeListLength; i++) 
			{
				if(this._treeList[i].rc == -1) tot++; // no child is to be drawn
			}
			return tot;
		}

		private double FaceVariance(short v1, short v2, short v3)
		{
			double MaxVar = 0;
			if(Math.Abs(v1 - v2) == 1 || Math.Abs(v3 - v2) == 1) 
			{
				MaxVar = 0; // minimal face, no variance
			}
			else
			{
				// find vertice in middle of hypothenuse
				short mid_hyp_indice = MidHypVerticeIndice(v1, v3);
				// find real elevation in middle of hypothenuse
				CustomVertex.PositionTextured vh = this._elevatedVertices[mid_hyp_indice];
				Vector3 v = MathEngine.CartesianToSpherical(vh.X, vh.Y, vh.Z);
				double real = v.X - this._layerRadius;
				// find extrapolated elevation in middle hyp.
				float xe = (this._elevatedVertices[v1].X + this._elevatedVertices[v3].X) / 2;
				float ye = (this._elevatedVertices[v1].Y + this._elevatedVertices[v3].Y) / 2;
				float ze = (this._elevatedVertices[v1].Z + this._elevatedVertices[v3].Z) / 2;
				v = MathEngine.CartesianToSpherical(xe, ye, ze);
				double extrapolated = v.X - this._layerRadius;
				// variance Note: could be done w/out MathEngine by computing raw cartesian distance
				MaxVar = real - extrapolated; 
				// recurse for potential childs until unit face
				MaxVar = Math.Max(MaxVar, FaceVariance(v2, mid_hyp_indice, v1));
				MaxVar = Math.Max(MaxVar, FaceVariance(v3, mid_hyp_indice, v2));
			}
			return MaxVar;
		}

		public void BuildIndices()
		{
			// how many faces total with walls/margin ?
			int MarginFaces = (this._margin + this._vertexDensity) * this._margin * 2 * 4;
			int TotFaces = MarginFaces + this._terrainFaceCount;
			this.Indices = new short[TotFaces * 3];
			int idx = 0;
			// build terrain
			for(short i = 0; i < this._treeListLength; i++) 
			{
				if(this._treeList[i].rc == -1) // no child: is to be drawn
				{
					this.Indices[idx] = this._treeList[i].i1;
					this.Indices[idx+1] = this._treeList[i].i2;
					this.Indices[idx+2] = this._treeList[i].i3;
					idx += 3;
				}
			}
			// build walls/margin if any
			if(this._margin != 0) 
			{
				short i1, i2;
				// North wall
				i1 = this.nw;
				for(i2 = (short)(this.nw + 1); i2 <= this.ne; i2++)
				{
					if(VerticeIsUsedInTerrain(i2))
					{
						this.Indices[idx] = i1;
						this.Indices[idx+1] = i2;
						this.Indices[idx+2] = (short)(i2 - (short)this._verticeDensity);
						idx += 3;
						this.Indices[idx] = (short)(i2 - (short)this._verticeDensity);
						this.Indices[idx+1] = (short)(i1 - (short)this._verticeDensity);
						this.Indices[idx+2] = i1;
						idx += 3;
						i1 = i2;					
					}
				}
				// South wall
				i1 = this.sw;
				for(i2 = (short)(this.sw + 1); i2 <= this.se; i2++)
				{
					if(VerticeIsUsedInTerrain(i2))
					{
						this.Indices[idx] = i2;
						this.Indices[idx+1] = i1;
						this.Indices[idx+2] = (short)(i1 + (short)this._verticeDensity);
						idx += 3;
						this.Indices[idx] = (short)(i1 + (short)this._verticeDensity);
						this.Indices[idx+1] = (short)(i2 + (short)this._verticeDensity);
						this.Indices[idx+2] = i2;
						idx += 3;
						i1 = i2;					
					}
				}
				// West wall
				i1 = this.nw;
				for(i2 = (short)(this.nw + (short)this._verticeDensity); i2 <= this.sw; i2 += (short)this._verticeDensity)
				{
					if(VerticeIsUsedInTerrain(i2))
					{
						this.Indices[idx] = i2;
						this.Indices[idx+1] = i1;
						this.Indices[idx+2] = (short)(i1 - 1);
						idx += 3;
						this.Indices[idx] = (short)(i1 - 1);
						this.Indices[idx+1] = (short)(i2 - 1);
						this.Indices[idx+2] = i2;
						idx += 3;
						i1 = i2;					
					}
				}
				// East wall
				i1 = this.ne;
				for(i2 = (short)(this.ne + (short)this._verticeDensity); i2 <= this.se; i2 += (short)this._verticeDensity)
				{
					if(VerticeIsUsedInTerrain(i2))
					{
						this.Indices[idx] = i1;
						this.Indices[idx+1] = i2;
						this.Indices[idx+2] = (short)(i2 + 1);
						idx += 3;
						this.Indices[idx] = (short)(i2 + 1);
						this.Indices[idx+1] = (short)(i1 + 1);
						this.Indices[idx+2] = i1;
						idx += 3;
						i1 = i2;					
					}
				}

				/*
				for(short i = (short)this._margin; i < this._verticeDensity - 1 - this._margin; i++)
				{
					// north wall
					this.Indices[idx] = i;
					this.Indices[idx+1] = (short)(i + (short)this._verticeDensity);
					this.Indices[idx+2] = (short)(i + 1);
					idx += 3;
					this.Indices[idx] = (short)(i + 1);
					this.Indices[idx+1] = (short)(i + (short)this._verticeDensity);
					this.Indices[idx+2] = (short)(i + (short)this._verticeDensity + 1);
					idx += 3;
					// south wall
					this.Indices[idx] = (short)(i + (short)(this._verticeDensity * (this._margin + this._vertexDensity)));
					this.Indices[idx+1] = (short)(this.Indices[idx] + (short)this._verticeDensity);
					this.Indices[idx+2] = (short)(this.Indices[idx] + 1);
					idx += 3;
					this.Indices[idx] = (short)(i + (short)(this._verticeDensity * (this._margin + this._vertexDensity) + 1));
					this.Indices[idx+1] = (short)(this.Indices[idx] + (short)(this._verticeDensity - 1));
					this.Indices[idx+2] = (short)(this.Indices[idx] + (short)this._verticeDensity);
					idx += 3;
				}
				for(short i = (short)this._margin; i < this._verticeDensity - 1 - this._margin; i++)
				{
					// west wall
					this.Indices[idx] = (short)(i * (short)this._verticeDensity);
					this.Indices[idx+1] = (short)((i + 1) * (short)this._verticeDensity);
					this.Indices[idx+2] = (short)(this.Indices[idx] + 1);
					idx += 3;
					this.Indices[idx] = (short)((i * (short)this._verticeDensity) + 1);
					this.Indices[idx+1] = (short)(this.Indices[idx] + (short)(this._verticeDensity - 1));
					this.Indices[idx+2] = (short)(this.Indices[idx] + (short)this._verticeDensity);
					idx += 3;
					// east wall
					this.Indices[idx] = (short)((i * (short)this._verticeDensity) + (this._margin + this._vertexDensity));
					this.Indices[idx+1] = (short)(this.Indices[idx] + (short)this._verticeDensity);
					this.Indices[idx+2] = (short)(this.Indices[idx] + 1);
					idx += 3;
					this.Indices[idx] = (short)((i * (short)this._verticeDensity) + (this._margin + this._vertexDensity + 1));
					this.Indices[idx+1] = (short)(this.Indices[idx] + (short)(this._verticeDensity - 1));
					this.Indices[idx+2] = (short)(this.Indices[idx] + (short)this._verticeDensity);
					idx += 3; 
				}
				*/
			}

		}

		private bool VerticeIsUsedInTerrain(short v)
		{
			for(int i = 0; i < this.Indices.Length; i++)
			{
				if (this.Indices[i] == v) return true;
			}
			return false;
		}
	}

	public class BinaryTriangle
	{
		public short i1, i2, i3; // indices to _elevatedVertices
		public short lc, rc;	 // indices to left and right childs in _treeList
		public short ln, rn, bn; // indices of left, right and bottom neighbours in _treeList

		public BinaryTriangle(short vertice1, short vertice2, short vertice3)
		{
			this.i1 = vertice1;
			this.i2 = vertice2;
			this.i3 = vertice3;
			this.lc = -1; // no left/right child
			this.rc = -1;
			this.ln = -1; // no neighbours
			this.rn = -1;
			this.bn = -1;
		}
	}
}
