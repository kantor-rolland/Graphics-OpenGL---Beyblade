using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Projekt_Beyblade.Figures;

namespace Projekt_Beyblade.Figures.ArrangementModels
{
    internal class BladeArrangementModel
    {

        private double Time { get; set; } = 0;
        public double SliceRotationAngle { get; private set; } = 0; // szog
        public bool IsRotating { get; private set; } = true; // porog e 

        public void StartRotation()
        {
            IsRotating = true;
            SliceRotationAngle = 0;
        }

        public void StopRotation()
        {
            IsRotating = false;
        }

        public void UpdateRotation(double deltaTime)
        {
            if (IsRotating)
            {
                Time += deltaTime; // eltetl ido
                SliceRotationAngle = Time * 10;
            }
        }

    }
}
