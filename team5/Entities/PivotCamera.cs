﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace team5
{
    class PivotCamera : StaticCamera
    {
        public enum AIState{
            Turning,
            Waiting,
        };
        
        private const float EdgeWaitTime = 2;
        private const float RotationSpeed = 20;
        private readonly Vector2 extents = new Vector2(225, 315);
        private float EdgeTimer = 0;
        private float Velocity = RotationSpeed;
        private AIState State = AIState.Turning;
        
        public PivotCamera(Vector2 position, Game1 game) : base(position, game)
        {
            ViewCone.FromDegrees(extents.X, 32);
        }
        
        public override void Update(GameTime gameTime, Chunk chunk)
        {
            float dt = Game1.DeltaT;
            float direction, view;
            ViewCone.ToDegrees(out direction, out view);
            
            switch(State)
            {
                case AIState.Turning:
                    if(direction < extents.X || extents.Y < direction)
                    {
                        Velocity = 0;
                        EdgeTimer = EdgeWaitTime;
                        State = AIState.Waiting;
                    }
                    break;
                case AIState.Waiting:
                    EdgeTimer -= dt;
                    if(EdgeTimer <= 0)
                    {
                        State = AIState.Turning;
                        if(direction < extents.X)
                            Velocity = +RotationSpeed;
                        else
                            Velocity = -RotationSpeed;
                    }
                    break;
            }
            
            direction += Velocity*dt;
            ViewCone.FromDegrees(direction, view);
            base.Update(gameTime, chunk);
        }
    }
}