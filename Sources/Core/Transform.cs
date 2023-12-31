﻿using System;
using System.Numerics;

namespace GLSample.Core
{
    public class Transform
    {
        public Vector3 Position { get; set; } = Vector3.Zero;
        public Quaternion Rotation { get; set; } = Quaternion.Identity;
        public Vector3 Scale { get; set; } = Vector3.One;

        public Vector3 EulerAngles
        {
            set
            {
                value *= MathF.PI / 180f;
                Rotation = Quaternion.CreateFromYawPitchRoll(value.Y, value.X, value.Z);
            }
        }

        public Matrix4x4 LocalToWorldMatrix => 
                        Matrix4x4.CreateTranslation(Position) *
                        Matrix4x4.CreateFromQuaternion(Rotation) *
                        Matrix4x4.CreateScale(Scale);
    }
}
