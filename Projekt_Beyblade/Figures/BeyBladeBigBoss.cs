using Silk.NET.Maths;
using Silk.NET.OpenGL;
using System;

namespace Projekt_Beyblade.Figures
{
    internal class BeyBladeBigBoss
    {
        public GlObjectWithTexture beyBlade { get; set; }

        public Vector3D<float> speed { get; set; }

        public float timer { get; set; }

        public bool gata { get; set; } // el a meg vagy sem, -> gata meghalt

        public BeyBladeBigBoss(GL Gl, string texture)
        {
            float[] faceColor = { 1f, 0f, 0f, 1.0f }; // voroses szin
            // float[] faceColor = { 0f, 0f, 0f, 0f };

            beyBlade = ObjResourceReader.CreateObjectWithTexture(Gl, faceColor, "beyblade.obj", texture); // objektum texturaval

            var random = new Random();

            timer = (float)(random.NextDouble() * 1 + 3);

            if (random.NextDouble() > 0.5) // ket kulonbozo iranyunk lesz
            {
                speed = new Vector3D<float>(-2.5f, 0f, -2.5f);
            }
            else
            {
                speed = new Vector3D<float>(2.5f, 0f, 2.5f);
            }

            // kezdeti pozicio
            float x = (float)(random.NextDouble() * (100 - (-100)) + (-100));
            float z = (float)(random.NextDouble() * (100 - (-100)) + (-100));

            beyBlade.Position = new Vector3D<float>(x, 0f, z);

            gata = false;
        }

        public void UpdatePosition(float deltaTime)
        {
            // folyton mozognak
            var random = new Random();
            beyBlade.Position += speed * deltaTime * 10;

            timer -= deltaTime;
            if (timer <= 0)
            {
                timer = (float)(random.NextDouble() * 1 + 5);
                speed = new Vector3D<float>(-speed.X, speed.Y, -speed.Z);
            }
        }

        public void ReleaseBeyBlade()
        {
            beyBlade.ReleaseGlObject(); // felszabaditas
        }

    }
}
