using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Silk.NET.Maths;

namespace Projekt_Beyblade.Camera
{
    internal class CameraDescriptor
    {
        private double DistanceToOrigin = 50;

        private double AngleToZYPlane = 0;

        private double AngleToZXPlane = 0.2;

        private const double DistanceScaleFactor = 1.1;

        private const double AngleChangeStepSize = Math.PI / 180 * 5;

        private Vector3D<float> _position = new Vector3D<float>(0, 0, 50);
        private Vector3D<float> _target = Vector3D<float>.Zero;
        private Vector3D<float> _upVector = new Vector3D<float>(0, 1, 0);

        public Vector3D<float> Position
        {
            get => _position;
            set => _position = value;

            /*
            get
            {
                return GetPointFromAngles(DistanceToOrigin, AngleToZYPlane, AngleToZXPlane);
            }
            */
        }

        public Vector3D<float> UpVector
        {
            get => _upVector;
            set => _upVector = value;

            /*
            get
            {
                return Vector3D.Normalize(GetPointFromAngles(DistanceToOrigin, AngleToZYPlane, AngleToZXPlane + Math.PI / 2));
            }
            */
        }

        public Vector3D<float> Target
        {
             get => _target;
             set => _target = value;
            /*
            get
            {
                // For the moment the camera is always pointed at the origin.
                return Vector3D<float>.Zero;
            }*/
        }

        public void SetExternalView()
        {
            DistanceToOrigin = 90;
            AngleToZYPlane = 0;
            AngleToZXPlane = 0.2;
            _position = GetPointFromAngles(DistanceToOrigin, AngleToZYPlane, AngleToZXPlane);
            _target = Vector3D<float>.Zero;
            _upVector = Vector3D.Normalize(GetPointFromAngles(DistanceToOrigin, AngleToZYPlane, AngleToZXPlane + Math.PI / 2));
        }

        public void SetInternalView(Vector3D<float> objPosition)
        {
            //      x - eltolja kicsit ferden
            Vector3D<float> offset = new Vector3D<float>(0, 20, 50);
            _position = objPosition + offset;
            _target = objPosition;
        }

        public double GetDistanceToOrigin()
        {
            return DistanceToOrigin;
        }

        public double GetAngleToZYPlane()
        {
            return AngleToZYPlane;
        }

        public double GetAngleToZXPlane()
        {
            return AngleToZXPlane;
        }

        public void IncreaseZXAngle()
        {
            AngleToZXPlane += AngleChangeStepSize;
        }

        public void DecreaseZXAngle()
        {
            AngleToZXPlane -= AngleChangeStepSize;
        }

        public void IncreaseZYAngle()
        {
            AngleToZYPlane += AngleChangeStepSize;

        }

        public void DecreaseZYAngle()
        {
            AngleToZYPlane -= AngleChangeStepSize;
        }

        public void IncreaseDistance()
        {
            DistanceToOrigin = DistanceToOrigin * DistanceScaleFactor;
        }

        public void DecreaseDistance()
        {
            DistanceToOrigin = DistanceToOrigin / DistanceScaleFactor;
        }

        private static Vector3D<float> GetPointFromAngles(double distanceToOrigin, double angleToMinZYPlane, double angleToMinZXPlane)
        {
            var x = distanceToOrigin * Math.Cos(angleToMinZXPlane) * Math.Sin(angleToMinZYPlane);
            var z = distanceToOrigin * Math.Cos(angleToMinZXPlane) * Math.Cos(angleToMinZYPlane);
            var y = distanceToOrigin * Math.Sin(angleToMinZXPlane);

            return new Vector3D<float>((float)x, (float)y, (float)z);
        }
    }
}

