using System;
using System.Globalization;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using WorldWind;

namespace WorldWind.Camera
{
	/// <summary>
	/// Camera base class (simple camera)
	/// </summary>
	public class CameraBase
	{
		protected short _terrainElevation;
		protected double _worldRadius;

		protected Angle _latitude;
		protected Angle _longitude;
		protected Angle _heading;
		protected Angle _tilt;
		protected Angle _bank;
		protected double _distance; // Distance from eye to target
		protected double _altitude; // Altitude above sea level
		//protected Quaternion _orientation;
		protected Quaternion4d m_Orientation;

		protected Frustum _viewFrustum = new Frustum();
		protected Angle _fov = World.Settings.cameraFov;

		protected Vector3 _position;

		protected static readonly Angle minTilt  = Angle.FromDegrees(0.0);
		protected static readonly Angle maxTilt = Angle.FromDegrees(85.0);
		protected static readonly double minimumAltitude = 10;
		protected static double maximumAltitude = double.MaxValue;

		protected Matrix m_ProjectionMatrix; // Projection matrix used in last render.
		protected Matrix m_ViewMatrix; // View matrix used in last render.
		protected Matrix m_WorldMatrix = Matrix.Identity;

		protected Angle viewRange;
		protected Angle trueViewRange;
		protected Viewport viewPort;

		protected int lastStepZoomTickCount;
		static Point3d cameraUpVector = new Point3d(0,0,1);
        public Point3d ReferenceCenter = new Point3d(0,0,0);

		// Camera Reset variables
		static int lastResetTime; // Used by Reset() to keep track type of reset.
		const int DoubleTapDelay = 3000; // Double tap max time (ms)

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Camera.CameraBase"/> class.
		/// </summary>
		/// <param name="targetPosition"></param>
		/// <param name="radius">Planet's radius in meters</param>
		public CameraBase( Vector3 targetPosition, double radius ) 
		{
			this._worldRadius = radius;
			this._distance = 2*_worldRadius;
			this._altitude = this._distance;
			maximumAltitude = 20 * _worldRadius;
		//	this._orientation = MathEngine.EulerToQuaternion(0,0,0);
			this.m_Orientation = Quaternion4d.EulerToQuaternion(0,0,0);
		}

		public Viewport Viewport
		{
			get
			{
				return viewPort;
			}
		}

		public Matrix ViewMatrix
		{
			get
			{
				return m_ViewMatrix;
			}
		}

		public Matrix ProjectionMatrix
		{
			get
			{
				return m_ProjectionMatrix;
			}
		}
		public Matrix WorldMatrix
		{
			get
			{
				return m_WorldMatrix;
			}
		}

		public bool IsPointGoto
		{
			get { return World.Settings.cameraIsPointGoto; }
			set { World.Settings.cameraIsPointGoto = value; }
		}

		public virtual Angle Latitude
		{
			get{ return this._latitude; }
		}

		public virtual Angle Longitude
		{
			get { return this._longitude; }
		}

		public virtual Angle Tilt
		{
			get { return _tilt; }
			set
			{
				if (value > maxTilt)
					value  = maxTilt;
				else if (value < minTilt)
					value = minTilt;

				_tilt = value;
				ComputeAltitude(_distance, _tilt);
				if(_altitude < _terrainElevation*World.Settings.VerticalExaggeration+minimumAltitude)
				{
					_altitude = _terrainElevation*World.Settings.VerticalExaggeration+minimumAltitude;
					// TODO:
					ComputeTilt(_altitude, _distance);
				}
			}
		}

		public virtual Angle Bank
		{
			get { return _bank; }
			set
			{
				if(Angle.IsNaN(value))
					return;

				_bank = value;
			}
		}

		public virtual Angle Heading
		{
			get { return this._heading; }
			set { this._heading = value; }
		}

		public virtual Quaternion4d CurrentOrientation
		{
		//	get { return this._orientation; }	
		//	set { this._orientation = value; }
			get { return m_Orientation; }	
			set { m_Orientation = value; }
		}

		/// <summary>
		/// Altitude above sea level (meters)
		/// </summary>
		public virtual double Altitude
		{
			get { return this._altitude; }
			set
			{
				TargetAltitude = value;
			}
		}

		/// <summary>
		/// Altitude above terrain (meters)
		/// </summary>
		public virtual double AltitudeAboveTerrain
		{
			get { return this._altitude - _terrainElevation; }
		}

		/// <summary>
		/// Target altitude above sea level (meters) (after travel)
		/// </summary>
		public virtual double TargetAltitude
		{
			get { return this._altitude; }
			set
			{
				if(value < _terrainElevation*World.Settings.VerticalExaggeration + minimumAltitude)
					value = _terrainElevation*World.Settings.VerticalExaggeration + minimumAltitude;
				if(value > maximumAltitude)
					value = maximumAltitude;
				this._altitude = value;
				ComputeDistance(_altitude, _tilt);
			}
		}

		public virtual short TerrainElevation
		{
			get { return this._terrainElevation; }
			set { this._terrainElevation = value; }
		}

		public virtual Angle ViewRange
		{
			get 
			{
				return viewRange;
			}
		}

		/// <summary>
		/// Angle from horizon - center earth - horizon in opposite directon
		/// </summary>
		public virtual Angle TrueViewRange
		{
			get 
			{
				return trueViewRange;
			}
		}

		/// <summary>
		/// Camera position (World XYZ coordinates)
		/// </summary>
		public virtual Vector3 Position
		{
			get{ return this._position; }
		}

		/// <summary>
		/// The planet's radius in meters
		/// </summary>
		public virtual double WorldRadius
		{
			get { return this._worldRadius;	}
			set { this._worldRadius = value; }
		}

		public Vector3 EyeDiff = new Vector3();

		float curCameraElevation = 0;
		float targetCameraElevation = 0;

		public static Point3d LookFrom = new Point3d();
		public static Point3d relCameraPos = new Point3d();

		public virtual void ComputeAbsoluteMatrices()
		{
			m_absoluteWorldMatrix = Matrix.Identity;
			
			float aspectRatio =  (float)viewPort.Width / viewPort.Height;
			float zNear = (float)this._altitude*0.1f;
			double distToCenterOfPlanet = (this._altitude + this.WorldRadius);
			double tangentalDistance  = Math.Sqrt( distToCenterOfPlanet*distToCenterOfPlanet - _worldRadius*_worldRadius);
			
			m_absoluteProjectionMatrix = Matrix.PerspectiveFovRH( (float)_fov.Radians, aspectRatio, zNear, (float)tangentalDistance );

			m_absoluteViewMatrix = Matrix.LookAtRH(
				MathEngine.SphericalToCartesian(
				_latitude.Degrees,
				_longitude.Degrees,
				WorldRadius),
				Vector3.Empty,
				new Vector3(0,0,1));

			m_absoluteViewMatrix *= Matrix.RotationYawPitchRoll(
				0,
				(float)-_tilt.Radians,
				(float)this._heading.Radians);
			m_absoluteViewMatrix *= Matrix.Translation(0,0,(float)(-this._distance - curCameraElevation));
			m_absoluteViewMatrix *= Matrix.RotationZ((float)this._bank.Radians);
		}

		public virtual void ComputeViewMatrix()
		{
			ComputeAbsoluteMatrices();

			if(World.Settings.ElevateCameraLookatPoint)
			{
                int minStep = 10;
				targetCameraElevation = TerrainElevation * World.Settings.VerticalExaggeration;
                float stepToTarget = targetCameraElevation - curCameraElevation;
                if (Math.Abs(stepToTarget) > minStep)
                {
                    float step = 0.05f * stepToTarget;
                    if (Math.Abs(step) < minStep) step = step > 0 ? minStep : -minStep;
                    curCameraElevation = curCameraElevation + step;
                }
                else curCameraElevation = targetCameraElevation;                
			}
			else
			{
				curCameraElevation = 0;
			}
			
            // needs to be double precsion
            double radius = WorldRadius + curCameraElevation;
            double radCosLat = radius*Math.Cos(_latitude.Radians);
            LookFrom = new Point3d(radCosLat * Math.Cos(_longitude.Radians),
                                radCosLat * Math.Sin(_longitude.Radians),
                                radius * Math.Sin(_latitude.Radians));

			// this constitutes a local tri-frame hovering above the sphere		
            Point3d zAxis = LookFrom.normalize(); // on sphere the normal vector and position vector are the same
	        Point3d xAxis = Point3d.cross(cameraUpVector,zAxis).normalize();
	        Point3d yAxis = Point3d.cross(zAxis, xAxis);

            ReferenceCenter = MathEngine.SphericalToCartesianD(
				Angle.FromRadians(Convert.ToSingle(_latitude.Radians)),
				Angle.FromRadians(Convert.ToSingle(_longitude.Radians)),
				WorldRadius);
			
			// Important step !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // In order to use single precsion rendering, we need to define a local frame (i.e. center of center tile, etc.)
            // Vector3d LocalCenter should be defined & initialized in the CameraBase class
            // Each time the camera moves, a new local center could be defined
            // The local center also has to be subtracted from all the terrain vertices!!!!
	        relCameraPos = LookFrom - ReferenceCenter;

	        // Important step: construct the single precision m_ViewMatrix by hand

            // We can build the m_ViewMatrix by hand
            m_ViewMatrix.M11 = (float)xAxis.X;
            m_ViewMatrix.M21 = (float)xAxis.Y;
            m_ViewMatrix.M31 = (float)xAxis.Z;

            m_ViewMatrix.M12 = (float)yAxis.X;
            m_ViewMatrix.M22 = (float)yAxis.Y;
            m_ViewMatrix.M32 = (float)yAxis.Z;

            m_ViewMatrix.M13 = (float)zAxis.X;
            m_ViewMatrix.M23 = (float)zAxis.Y;
            m_ViewMatrix.M33 = (float)zAxis.Z;

            m_ViewMatrix.M41 = -(float)(xAxis.X*relCameraPos.X + xAxis.Y*relCameraPos.Y + xAxis.Z*relCameraPos.Z);
			m_ViewMatrix.M42 = -(float)(yAxis.X*relCameraPos.X + yAxis.Y*relCameraPos.Y + yAxis.Z*relCameraPos.Z);
			m_ViewMatrix.M43 = -(float)(zAxis.X*relCameraPos.X + zAxis.Y*relCameraPos.Y + zAxis.Z*relCameraPos.Z);
            
            m_ViewMatrix.M14 = (float)0.0;
            m_ViewMatrix.M24 = (float)0.0;
            m_ViewMatrix.M34 = (float)0.0;
            m_ViewMatrix.M44 = (float)1.0;
			
			double cameraDisplacement = _distance;
			if(cameraDisplacement < targetCameraElevation + minimumAltitude)
				cameraDisplacement = targetCameraElevation + minimumAltitude;

	        m_ViewMatrix *= Matrix.RotationYawPitchRoll(
				0,
				(float)-_tilt.Radians,
				(float)_heading.Radians);
			m_ViewMatrix *= Matrix.Translation(0,0,(float)(-cameraDisplacement + curCameraElevation));
			m_ViewMatrix *= Matrix.RotationZ((float)_bank.Radians);
			
			// Extract camera position
			Matrix cam = Matrix.Invert(m_absoluteViewMatrix);

			_position = new Vector3(cam.M41, cam.M42, cam.M43);
		}

		/// <summary>
		/// Field of view (degrees)
		/// </summary>
		public virtual Angle Fov
		{
			get{ return this._fov; }
			set
			{
				if(value > World.Settings.cameraFovMax)
					value = World.Settings.cameraFovMax;
				if(value < World.Settings.cameraFovMin)
					value = World.Settings.cameraFovMin;
				this._fov = value;
			}
		}

		/// <summary>
		/// Distance to target position on ground.
		/// </summary>
		public virtual double Distance
		{
			get
			{
				// TODO: Accurate altitude / distance calculations
				return _distance - _terrainElevation;
			}
			set
			{
				TargetDistance = value;
			}
		}

		/// <summary>
		/// Distance to target position on ground (after traveling to target)
		/// </summary>
		public virtual double TargetDistance
		{
			get
			{
				return _distance;
			}
			set
			{
				if(value < minimumAltitude)
					value = minimumAltitude;
				if(value > maximumAltitude)
					value = maximumAltitude;
				_distance = value;
				ComputeAltitude(_distance, _tilt);
			}
		}

		public virtual Frustum ViewFrustum
		{
			get { return this._viewFrustum; }
		}

		public virtual void Update(Device device)
		{
			viewPort = device.Viewport;

			Point3d p = Quaternion4d.QuaternionToEuler(m_Orientation);

			if(!double.IsNaN(p.Y))
				this._latitude.Radians = p.Y;
			if(!double.IsNaN(p.X))
				this._longitude.Radians = p.X;
			if(!double.IsNaN(p.Z))
				this._heading.Radians = p.Z;

			ComputeProjectionMatrix(viewPort);
			ComputeViewMatrix();
			device.Transform.Projection = m_ProjectionMatrix;
			device.Transform.View = m_ViewMatrix;
			device.Transform.World = m_WorldMatrix;
	
			ViewFrustum.Update(
				Matrix.Multiply(m_absoluteWorldMatrix,
				Matrix.Multiply(m_absoluteViewMatrix, m_absoluteProjectionMatrix)));

			// Old view range (used in quadtile logic)
			double factor = (this._altitude) / this._worldRadius;
			if(factor > 1)
				viewRange = Angle.FromRadians(Math.PI);
			else
				viewRange = Angle.FromRadians(Math.Abs(Math.Asin((this._altitude) / this._worldRadius))*2);

			// True view range 
			if(factor < 1)
				trueViewRange = Angle.FromRadians(Math.Abs(Math.Asin((this._distance) / this._worldRadius))*2);
			else
				trueViewRange = Angle.FromRadians(Math.PI);	
	
			World.Settings.cameraAltitudeMeters = Altitude;
			World.Settings.cameraLatitude = _latitude;
			World.Settings.cameraLongitude = _longitude;
			World.Settings.cameraHeading = _heading;
			World.Settings.cameraTilt = _tilt;
		}

		/// <summary>
		/// Resets the camera settings
		/// Two consecutive resets closer than DoubleTapDelay ms apart performs a full reset.
		/// </summary>
		public virtual void Reset()
		{
			Fov = World.Settings.cameraFov;

			int curTime = Environment.TickCount;
			if(curTime-lastResetTime < DoubleTapDelay)
			{
				// Was already reset (step 1) - do a full reset
				if (Angle.IsNaN(_tilt))
					_tilt.Radians = 0;
				if (Angle.IsNaN(_heading))
					_heading.Radians = 0;
				if (Angle.IsNaN(_bank))
					_bank.Radians = 0;
				this.SetPosition(double.NaN,double.NaN,0,2*this._worldRadius,0,0);
			}
			else
			{
				// Reset direction, tilt & bank
				this.SetPosition(double.NaN,double.NaN,0,double.NaN,0,0);
			}
			lastResetTime = curTime;
		}

		/// <summary>
		/// Sets camera position.
		/// </summary>
		/// <param name="lat">Latitude in decimal degrees</param>
		/// <param name="lon">Longitude in decimal degrees</param>
		/// <param name="heading">Heading in decimal degrees</param>
		/// <param name="_altitude">Altitude in meters</param>
		/// <param name="tilt">Tilt in decimal degrees</param>
		public virtual void PointGoto(double lat, double lon )
		{
			if(!World.Settings.cameraIsPointGoto)
				return;

			SetPosition( lat,lon,double.NaN,double.NaN,double.NaN,double.NaN );
		}

		/// <summary>
		/// Sets camera position.
		/// </summary>
		/// <param name="lat">Latitude in decimal degrees</param>
		/// <param name="lon">Longitude in decimal degrees</param>
		/// <param name="heading">Heading in decimal degrees</param>
		/// <param name="_altitude">Altitude in meters</param>
		/// <param name="tilt">Tilt in decimal degrees</param>
		public virtual void PointGoto(Angle lat, Angle lon )
		{
			if(!World.Settings.cameraIsPointGoto)
				return;

			SetPosition( lat.Degrees,lon.Degrees,double.NaN,double.NaN,double.NaN,double.NaN );
		}

		/// <summary>
		/// Sets camera position.
		/// </summary>
		/// <param name="lat">Latitude in decimal degrees</param>
		/// <param name="lon">Longitude in decimal degrees</param>
		/// <param name="heading">Heading in decimal degrees</param>
		/// <param name="_altitude">Altitude in meters</param>
		/// <param name="tilt">Tilt in decimal degrees</param>
		public virtual void SetPosition(double lat, double lon )
		{
			SetPosition( lat,lon,0,double.NaN,0,0 );
		}

		/// <summary>
		/// Sets camera position.
		/// </summary>
		/// <param name="lat">Latitude in decimal degrees</param>
		/// <param name="lon">Longitude in decimal degrees</param>
		/// <param name="heading">Heading in decimal degrees</param>
		/// <param name="_altitude">Altitude above ground level in meters</param>
		/// <param name="tilt">Tilt in decimal degrees</param>
		public virtual void SetPosition(double lat, double lon, double heading, double _altitude, double tilt )
		{
			SetPosition( lat,lon,heading,_altitude,tilt,0);
		}

		/// <summary>
		/// Sets camera position.
		/// </summary>
		/// <param name="lat">Latitude in decimal degrees</param>
		/// <param name="lon">Longitude in decimal degrees</param>
		/// <param name="heading">Heading in decimal degrees</param>
		/// <param name="_altitude">Altitude above ground level in meters</param>
		/// <param name="tilt">Tilt in decimal degrees</param>
		/// <param name="bank">Camera bank (roll) in decimal degrees</param>
		public virtual void SetPosition(double lat, double lon, double heading, double _altitude, double tilt, double bank)
		{
			if(double.IsNaN(lat)) lat = this._latitude.Degrees;
			if(double.IsNaN(lon)) lon = this._longitude.Degrees;
			if(double.IsNaN(heading)) heading = this._heading.Degrees;
			if(double.IsNaN(bank))  bank = this._bank.Degrees;

			m_Orientation = Quaternion4d.EulerToQuaternion(
				MathEngine.DegreesToRadians(lon),
				MathEngine.DegreesToRadians(lat),
				MathEngine.DegreesToRadians(heading));

			Point3d p = Quaternion4d.QuaternionToEuler(m_Orientation);
			
			_latitude.Radians = p.Y;
			_longitude.Radians = p.X;
			_heading.Radians = p.Z;

			if(!double.IsNaN(tilt)) 
				Tilt = Angle.FromDegrees(tilt);
			if(!double.IsNaN(_altitude)) 
				this.Altitude = _altitude;
			this.Bank = Angle.FromDegrees(bank);
		}
			
		Matrix m_absoluteViewMatrix = Matrix.Identity;
		Matrix m_absoluteWorldMatrix = Matrix.Identity;
		Matrix m_absoluteProjectionMatrix = Matrix.Identity;
		
		public Matrix AbsoluteViewMatrix
		{
			get{ return m_absoluteViewMatrix; }
		}
		
		public Matrix AbsoluteWorldMatrix
		{
			get{ return m_absoluteWorldMatrix; }
		}

		public Matrix AbsoluteProjectionMatrix
		{
			get{ return m_absoluteProjectionMatrix; }
		}

		/// <summary>
		/// Calculates latitude/longitude for given screen coordinate.
		/// </summary>
		public virtual void PickingRayIntersection(
			int screenX, 
			int screenY,
			out Angle latitude,
			out Angle longitude)
		{
			Vector3 v1 = new Vector3();
			v1.X = screenX;
			v1.Y = screenY;
			v1.Z = viewPort.MinZ;
			v1.Unproject(viewPort, m_absoluteProjectionMatrix, m_absoluteViewMatrix, m_absoluteWorldMatrix);

			Vector3 v2 = new Vector3();
			v2.X = screenX;
			v2.Y = screenY;
			v2.Z = viewPort.MaxZ;
			v2.Unproject(viewPort, m_absoluteProjectionMatrix, m_absoluteViewMatrix, m_absoluteWorldMatrix);

			Point3d p1 = new Point3d(v1.X, v1.Y, v1.Z);
			Point3d p2 = new Point3d(v2.X, v2.Y, v2.Z);

			double a = (p2.X - p1.X) * (p2.X - p1.X) + (p2.Y - p1.Y) * (p2.Y - p1.Y) + (p2.Z - p1.Z) * (p2.Z - p1.Z);
			double b = 2.0*((p2.X - p1.X)*(p1.X) + (p2.Y - p1.Y)*(p1.Y) + (p2.Z - p1.Z)*(p1.Z));
			double c = p1.X*p1.X + p1.Y*p1.Y + p1.Z*p1.Z - _worldRadius * _worldRadius;

			double discriminant = b*b - 4 * a * c;
			if(discriminant <= 0)
			{
				latitude = Angle.NaN;
				longitude = Angle.NaN;
				return;
			}

			//	float t0 = ((-1.0f) * b + (float)Math.Sqrt(b*b - 4 * a * c)) / (2*a);
			double t1 = ((-1.0) * b - Math.Sqrt(b*b - 4 * a * c)) / (2*a);

			//	Vector3 i0 = new Vector3(p1.X + t0*(p2.X - p1.X), p1.Y + t0*(p2.Y - p1.Y), p1.Z + t0 *(p2.Z - p1.Z));
			Point3d i1 = new Point3d(p1.X + t1*(p2.X - p1.X), p1.Y + t1*(p2.Y - p1.Y), p1.Z + t1 *(p2.Z - p1.Z));

			//	Vector3 i0t = MathEngine.CartesianToSpherical(i0.X, i0.Y, i0.Z);
			Point3d i1t = MathEngine.CartesianToSphericalD(i1.X, i1.Y, i1.Z);
			Point3d mousePointer = i1t;

			latitude = Angle.FromRadians(mousePointer.Y);
			longitude = Angle.FromRadians(mousePointer.Z);
		}

		/// <summary>
		///  Calculates the projection transformation matrix, which transforms 3-D camera or 
		///  view space coordinates into 2-D screen coordinates.
		/// </summary>
		protected virtual void ComputeProjectionMatrix(Viewport viewport)
		{
			float aspectRatio =  (float)viewport.Width / viewport.Height;
			float zNear = (float)this._altitude*0.1f;
			double distToCenterOfPlanet = (this._altitude + this.WorldRadius);
			double tangentalDistance  = Math.Sqrt( distToCenterOfPlanet*distToCenterOfPlanet - _worldRadius*_worldRadius);
            if (tangentalDistance < 1000000)
                tangentalDistance = 1000000;
			m_ProjectionMatrix = Matrix.PerspectiveFovRH( (float)_fov.Radians, aspectRatio, zNear, (float)tangentalDistance );
		}

		public virtual void RotationYawPitchRoll(Angle yaw, Angle pitch, Angle roll)
		{
		//	this._orientation *= MathEngine.EulerToQuaternion(yaw.Radians, pitch.Radians, roll.Radians);
		//	Vector3 v = MathEngine.QuaternionToEuler(this._orientation);
			
		//	if(!double.IsNaN(v.Y))
		//		this._latitude.Radians = v.Y;
		//	if(!double.IsNaN(v.X))
		//		this._longitude.Radians = v.X;
		//	if(Math.Abs(roll.Radians)>Single.Epsilon)
		//		this._heading.Radians = v.Z;


			m_Orientation = Quaternion4d.EulerToQuaternion(yaw.Radians, pitch.Radians, roll.Radians) * m_Orientation;

			Point3d p = Quaternion4d.QuaternionToEuler(m_Orientation);
			if(!double.IsNaN(p.Y))
				_latitude.Radians = p.Y;
			if(!double.IsNaN(p.X))
				_longitude.Radians = p.X;
			if(Math.Abs(roll.Radians) > double.Epsilon)
				_heading.Radians = p.Z;
		}

		/// <summary>
		/// Digital zoom (keyboard/mouse wheel style)
		/// </summary>
		/// <param name="ticks">Positive value for zoom in, negative for zoom out.</param>
		public virtual void ZoomStepped( float ticks )
		{
			int currentTickCount = Environment.TickCount;

			double factor = World.Settings.cameraZoomStepFactor;
			if(factor<0)
				factor = 0;
			if (factor > 1)
				factor = 1;

			double minTime = 50;  // <= 50ms: fastest
			double maxTime = 250; // >=250ms: slowest
			double time = currentTickCount - lastStepZoomTickCount;
			if (time<minTime)
				time = minTime;
			double multiplier = 1-Math.Abs( (time-minTime)/maxTime ); // Range: 1 .. 2
			if(multiplier<0)
				multiplier=0;

			multiplier= multiplier * World.Settings.cameraZoomAcceleration;
			double mulfac = Math.Pow(1 - factor, multiplier+1 );
			mulfac = Math.Pow(mulfac, Math.Abs(ticks));

			if (ticks > 0)
				TargetDistance *= mulfac;
			else
				TargetDistance /= mulfac;

			lastStepZoomTickCount = currentTickCount;
		}

		/// <summary>
		/// Zoom camera in/out (distance) 
		/// </summary>
		/// <param name="percent">Positive value = zoom in, negative=out</param>
		public virtual void Zoom(float percent)
		{
			if(percent>0)
				TargetDistance /= 1.0f + percent;
			else
				TargetDistance *= 1.0f - percent;
		}

		/// <summary>
		/// Pan the camera using delta values
		/// </summary>
		/// <param name="lat">Latitude offset</param>
		/// <param name="lon">Longitude offset</param>
		public virtual void Pan(Angle lat, Angle lon)
		{
			if(Angle.IsNaN(lat)) lat = this._latitude;
			if(Angle.IsNaN(lon)) lon = this._longitude;
			lat += _latitude;
			lon += _longitude;

		//	this._orientation = MathEngine.EulerToQuaternion(
		//		lon.Radians,
		//		lat.Radians,
		//		_heading.Radians);

			m_Orientation = Quaternion4d.EulerToQuaternion(
				lon.Radians, lat.Radians, _heading.Radians);

			Point3d p = Quaternion4d.QuaternionToEuler(m_Orientation);

		//	Vector3 v = MathEngine.QuaternionToEuler(this._orientation);
		//	if(!double.IsNaN(v.Y))
		//	{
		//		this._latitude.Radians = v.Y;
		//		this._longitude.Radians = v.X;
		//	}

			if(!double.IsNaN(p.Y))
			{
				_latitude.Radians = p.Y;
				_longitude.Radians = p.X;
			}
		}
	
		protected void ComputeDistance( double altitude, Angle tilt )
		{
			double cos = Math.Cos(Math.PI-tilt.Radians);
			double x = _worldRadius*cos;
			double hyp = _worldRadius+altitude;
			double y = Math.Sqrt(_worldRadius*_worldRadius*cos*cos+hyp*hyp-_worldRadius*_worldRadius);
			double res = x-y;
			if(res<0)
				res = x+y;
			_distance = res;
		}

		protected void ComputeAltitude( double distance, Angle tilt )
		{
			double dfromeq = Math.Sqrt(_worldRadius*_worldRadius + distance*distance - 
				2 * _worldRadius*distance*Math.Cos(Math.PI-tilt.Radians));
			double alt = dfromeq - _worldRadius;
			if(alt<minimumAltitude + _terrainElevation*World.Settings.VerticalExaggeration)
				alt = minimumAltitude + _terrainElevation*World.Settings.VerticalExaggeration;
			else if(alt>maximumAltitude)
				alt = maximumAltitude;
			_altitude = alt;
		}

		protected void ComputeTilt(double alt, double distance )
		{
			double a = _worldRadius+alt;
			double b = distance;
			double c = _worldRadius;
			_tilt.Radians = Math.Acos((a*a+b*b-c*c)/(2*a*b));
		}

		/// <summary>
		/// Projects a point from world to screen coordinates.
		/// </summary>
		/// <param name="point">Point in world space</param>
		/// <returns>Point in screen space</returns>
		public Vector3 Project( Vector3 point )
		{
			point.Project( viewPort, m_ProjectionMatrix, m_ViewMatrix, m_WorldMatrix);
			return point;
		}

		public override string ToString()
		{
			string res = string.Format(CultureInfo.InvariantCulture,
				"Altitude: {6:f0}m\nView Range: {0}\nHeading: {1}\nTilt: {2}\nFOV: {7}\nPosition: ({3}, {4} @ {5:f0}m)",
				ViewRange, _heading, _tilt,
				_latitude,_longitude, _distance, _altitude, _fov);
			return res;
		}
		
		/// <summary>
		/// Gets the visible bounding box for the application in degrees.
		/// </summary>
		/// <returns>An array of Angles in minx.miny,maxx, maxy order</returns>
		public static Angle[] getViewBoundingBox()
		{
			/// TODO: Correct the ViewRange for non-square windows. Is is accurate horizontally
			/// but not vertically.
			Angle[] bbox = new Angle[4];
			
			/// HACK: need to deal with startup of World (nothing is instantiated yet)
			if (DrawArgs.Camera !=null)
			{
				Angle lat = DrawArgs.Camera.Latitude;
				Angle lon = DrawArgs.Camera.Longitude;
				Angle vr  = DrawArgs.Camera.ViewRange;
				
				Angle North = lat + (0.5 * vr);
				Angle South = lat - (0.5 * vr);
				Angle East  = lon + (0.5 * vr);
				Angle West  = lon - (0.5 * vr);
				
				//minX(West), minY(South), maxX(East), MaxY(North)
				bbox[0] = West; bbox[1] = South; 
				bbox[2] = East; bbox[3] = North;
			}
			else
			{
				bbox[0] = Angle.FromDegrees(-180.0); bbox[1]= Angle.FromDegrees(-90.0);
				bbox[2] = Angle.FromDegrees(180.0);  bbox[3]= Angle.FromDegrees(90.0);
			}
			return bbox; 
		}
	}
}
