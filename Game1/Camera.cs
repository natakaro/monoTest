﻿using Game1.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1
{
    public class Camera
    {
        public enum Actions
        {
            Crouch,
            Jump,
            MoveForwards,
            MoveBackwards,
            StrafeRight,
            StrafeLeft,
            Run
        }

        public enum Posture
        {
            Standing,
            Crouching,
            Rising,
            Jumping
        };

        private Game Game;

        public const float DEFAULT_FOVX = 90.0f;
        public const float DEFAULT_ZNEAR = 0.1f;
        public const float DEFAULT_ZFAR = 1000.0f;

        private static Vector3 WORLD_X_AXIS = new Vector3(1.0f, 0.0f, 0.0f);
        private static Vector3 WORLD_Y_AXIS = new Vector3(0.0f, 1.0f, 0.0f);
        private static Vector3 WORLD_Z_AXIS = new Vector3(0.0f, 0.0f, 1.0f);
        private const float DEFAULT_ACCELERATION_X = 8.0f;
        private const float DEFAULT_ACCELERATION_Y = 8.0f;
        private const float DEFAULT_ACCELERATION_Z = 8.0f;
        private const float DEFAULT_VELOCITY_X = 1.0f;
        private const float DEFAULT_VELOCITY_Y = 1.0f;
        private const float DEFAULT_VELOCITY_Z = 1.0f;
        private const float DEFAULT_RUNNING_MULTIPLIER = 2.0f;
        private const float DEFAULT_MOUSE_SMOOTHING_SENSITIVITY = 0.5f;
        private const float DEFAULT_SPEED_ROTATION = 0.2f;
        private const float HEIGHT_MULTIPLIER_CROUCHING = 0.5f;
        private const int MOUSE_SMOOTHING_CACHE_SIZE = 10;

        private const float CAMERA_BOUNDS_MAX_X = 3715;
        private const float CAMERA_BOUNDS_MIN_X = 0;
        private const float CAMERA_BOUNDS_MAX_Z = 3200;
        private const float CAMERA_BOUNDS_MIN_Z = 0;


        private float fovx;
        private float aspectRatio;
        private float znear;
        private float zfar;
        private float accumHeadingDegrees;
        private float accumPitchDegrees;
        private float eyeHeightStanding;
        private float eyeHeightCrouching;
        private Vector3 eye;
        private Vector3 target;
        private Vector3 targetYAxis;
        private Vector3 xAxis;
        private Vector3 yAxis;
        private Vector3 zAxis;
        private Vector3 viewDir;
        private Vector3 acceleration;
        private Vector3 currentVelocity;
        private Vector3 velocity;
        private Vector3 velocityWalking;
        private Vector3 velocityRunning;
        private Quaternion orientation;
        
        public Matrix viewMatrix;
        public Matrix projMatrix;

        private bool forwardsPressed;
        private bool backwardsPressed;
        private bool strafeRightPressed;
        private bool strafeLeftPressed;
        private bool enableMouseSmoothing;
        private Posture posture;
        private int mouseIndex;
        private float rotationSpeed;
        private float mouseSmoothingSensitivity;
        private Vector2[] mouseMovement;
        private Vector2[] mouseSmoothingCache;
        private Vector2 smoothedMouseMovement;

        private Ray downwardRay;
        private Octree octree;

        private BoundingFrustum frustum;

        #region Public Methods

        public Camera(Game game)
        {
            this.Game = game;
            // Initialize camera state.
            fovx = DEFAULT_FOVX;
            znear = DEFAULT_ZNEAR;
            zfar = DEFAULT_ZFAR;
            accumHeadingDegrees = 0.0f;
            accumPitchDegrees = 0.0f;
            eyeHeightStanding = 0.0f;
            eyeHeightCrouching = 0.0f;
            eye = Vector3.Zero;
            target = Vector3.Zero;
            targetYAxis = Vector3.UnitY;
            xAxis = Vector3.UnitX;
            yAxis = Vector3.UnitY;
            zAxis = Vector3.UnitZ;
            viewDir = Vector3.Forward;
            acceleration = new Vector3(DEFAULT_ACCELERATION_X, DEFAULT_ACCELERATION_Y, DEFAULT_ACCELERATION_Z);
            velocityWalking = new Vector3(DEFAULT_VELOCITY_X, DEFAULT_VELOCITY_Y, DEFAULT_VELOCITY_Z);
            velocityRunning = velocityWalking * DEFAULT_RUNNING_MULTIPLIER;
            velocity = velocityWalking;
            orientation = Quaternion.Identity;
            viewMatrix = Matrix.Identity;
            posture = Posture.Standing;

            // Initialize mouse and keyboard input.
            enableMouseSmoothing = true;
            rotationSpeed = DEFAULT_SPEED_ROTATION;
            mouseSmoothingSensitivity = DEFAULT_MOUSE_SMOOTHING_SENSITIVITY;
            mouseSmoothingCache = new Vector2[MOUSE_SMOOTHING_CACHE_SIZE];
            mouseIndex = 0;
            mouseMovement = new Vector2[2];
            mouseMovement[0].X = 0.0f;
            mouseMovement[0].Y = 0.0f;
            mouseMovement[1].X = 0.0f;
            mouseMovement[1].Y = 0.0f;

            // Setup perspective projection matrix.
            Rectangle clientBounds = game.Window.ClientBounds;
            float aspect = (float)clientBounds.Width / (float)clientBounds.Height;
            Perspective(fovx, aspect, znear, zfar);

            frustum = new BoundingFrustum(ViewProjectionMatrix);
        }

        public void Initialize()
        {

            Rectangle clientBounds = Game.Window.ClientBounds;
            Mouse.SetPosition(clientBounds.Width / 2, clientBounds.Height / 2);
        }

        public void LookAt(Vector3 target)
        {
            LookAt(eye, target, yAxis);
        }

        public void LookAt(Vector3 eye, Vector3 target, Vector3 up)
        {
            this.eye = eye;
            this.target = target;

            zAxis = eye - target;
            zAxis.Normalize();

            viewDir.X = -zAxis.X;
            viewDir.Y = -zAxis.Y;
            viewDir.Z = -zAxis.Z;

            Vector3.Cross(ref up, ref zAxis, out xAxis);
            xAxis.Normalize();

            Vector3.Cross(ref zAxis, ref xAxis, out yAxis);
            yAxis.Normalize();
            xAxis.Normalize();

            viewMatrix.M11 = xAxis.X;
            viewMatrix.M21 = xAxis.Y;
            viewMatrix.M31 = xAxis.Z;
            Vector3.Dot(ref xAxis, ref eye, out viewMatrix.M41);
            viewMatrix.M41 = -viewMatrix.M41;

            viewMatrix.M12 = yAxis.X;
            viewMatrix.M22 = yAxis.Y;
            viewMatrix.M32 = yAxis.Z;
            Vector3.Dot(ref yAxis, ref eye, out viewMatrix.M42);
            viewMatrix.M42 = -viewMatrix.M42;

            viewMatrix.M13 = zAxis.X;
            viewMatrix.M23 = zAxis.Y;
            viewMatrix.M33 = zAxis.Z;
            Vector3.Dot(ref zAxis, ref eye, out viewMatrix.M43);
            viewMatrix.M43 = -viewMatrix.M43;

            viewMatrix.M14 = 0.0f;
            viewMatrix.M24 = 0.0f;
            viewMatrix.M34 = 0.0f;
            viewMatrix.M44 = 1.0f;

            accumPitchDegrees = MathHelper.ToDegrees((float)Math.Asin(viewMatrix.M23));
            accumHeadingDegrees = MathHelper.ToDegrees((float)Math.Atan2(viewMatrix.M13, viewMatrix.M33));

            Quaternion.CreateFromRotationMatrix(ref viewMatrix, out orientation);
            
        }

        /// <summary>
        /// Moves the camera by dx world units to the left or right; dy
        /// world units upwards or downwards; and dz world units forwards
        /// or backwards.
        /// </summary>
        /// <param name="dx">Distance to move left or right.</param>
        /// <param name="dy">Distance to move up or down.</param>
        /// <param name="dz">Distance to move forwards or backwards.</param>
        public void Move(float dx, float dy, float dz)
        {
            // Calculate the forwards direction. Can't just use the
            // camera's view direction as doing so will cause the camera to
            // move more slowly as the camera's view approaches 90 degrees
            // straight up and down.

            Vector3 forwards = Vector3.Normalize(Vector3.Cross(WORLD_Y_AXIS, xAxis));
            Vector3 movement = (xAxis * dx + WORLD_Y_AXIS * dy + forwards * dz);

            /*
            if(!octree.CameraIntersection(eye + new Vector3(movement.X, 0, 0) * 3, 5))
            {
                //dx = -dx/10;
                eye += xAxis * dx;
            }
            if (!octree.CameraIntersection(eye + new Vector3(0, movement.Y, 0) * 3, 5))
            {
                //dy = -dy/10;
                eye += WORLD_Y_AXIS * dy;
            }
            if (!octree.CameraIntersection(eye + new Vector3(0, 0, movement.Z) * 3, 5))
            {
                //dz = -dz/10;
                eye += forwards * dz;
            }
            */

            
            dx = octree.CameraIntersection(eye + new Vector3(movement.X, 0, 0)*3, 5) ? 0f : dx;
            dy = octree.CameraIntersection(eye + new Vector3(0, movement.Y, 0)*3, 5) ? 0f : dy;
            dz = octree.CameraIntersection(eye + new Vector3(0, 0, movement.Z)*3, 5) ? 0f : dz;
            
            eye += xAxis * dx;
            eye += forwards * dz;
            eye += WORLD_Y_AXIS * dy;

            Vector3 newPos = eye;
            
            if (eye.X > CAMERA_BOUNDS_MAX_X)
                newPos.X = CAMERA_BOUNDS_MAX_X;

            if (eye.X < CAMERA_BOUNDS_MIN_X)
                newPos.X = CAMERA_BOUNDS_MIN_X;
            /*
            //blokada wysokości - nie potrzebne
            if (eye.Y > CAMERA_BOUNDS_MAX_Y)
                newPos.Y = CAMERA_BOUNDS_MAX_Y;

            if (eye.Y < CAMERA_BOUNDS_MIN_Y)
                newPos.Y = CAMERA_BOUNDS_MIN_Y;
            */
            if (eye.Z > CAMERA_BOUNDS_MAX_Z)
                newPos.Z = CAMERA_BOUNDS_MAX_Z;

            if (eye.Z < CAMERA_BOUNDS_MIN_Z)
                newPos.Z = CAMERA_BOUNDS_MIN_Z;
            
            eye = newPos;

        Position = eye;
        }

        public void Perspective(float fovx, float aspect, float znear, float zfar)
        {
            this.fovx = fovx;
            this.aspectRatio = aspect;
            this.znear = znear;
            this.zfar = zfar;

            float aspectInv = 1.0f / aspect;
            float e = 1.0f / (float)Math.Tan(MathHelper.ToRadians(fovx) / 2.0f);
            float fovy = 2.0f * (float)Math.Atan(aspectInv / e);
            float xScale = 1.0f / (float)Math.Tan(0.5f * fovy);
            float yScale = xScale / aspectInv;

            projMatrix.M11 = xScale;
            projMatrix.M12 = 0.0f;
            projMatrix.M13 = 0.0f;
            projMatrix.M14 = 0.0f;

            projMatrix.M21 = 0.0f;
            projMatrix.M22 = yScale;
            projMatrix.M23 = 0.0f;
            projMatrix.M24 = 0.0f;

            projMatrix.M31 = 0.0f;
            projMatrix.M32 = 0.0f;
            projMatrix.M33 = (zfar + znear) / (znear - zfar);
            projMatrix.M34 = -1.0f;

            projMatrix.M41 = 0.0f;
            projMatrix.M42 = 0.0f;
            projMatrix.M43 = (2.0f * zfar * znear) / (znear - zfar);
            projMatrix.M44 = 0.0f;
        }

        public void Rotate(float headingDegrees, float pitchDegrees)
        {
            headingDegrees = -headingDegrees;
            pitchDegrees = -pitchDegrees;

            accumPitchDegrees += pitchDegrees;

            if (accumPitchDegrees > 90.0f)
            {
                pitchDegrees = 90.0f - (accumPitchDegrees - pitchDegrees);
                accumPitchDegrees = 90.0f;
            }

            if (accumPitchDegrees < -90.0f)
            {
                pitchDegrees = -90.0f - (accumPitchDegrees - pitchDegrees);
                accumPitchDegrees = -90.0f;
            }

            accumHeadingDegrees += headingDegrees;

            if (accumHeadingDegrees > 360.0f)
                accumHeadingDegrees -= 360.0f;

            if (accumHeadingDegrees < -360.0f)
                accumHeadingDegrees += 360.0f;

            float heading = MathHelper.ToRadians(headingDegrees);
            float pitch = MathHelper.ToRadians(pitchDegrees);
            Quaternion rotation = Quaternion.Identity;

            // Rotate the camera about the world Y axis.
            if (heading != 0.0f)
            {
                Quaternion.CreateFromAxisAngle(ref WORLD_Y_AXIS, heading, out rotation);
                Quaternion.Concatenate(ref rotation, ref orientation, out orientation);
            }

            // Rotate the camera about its local X axis.
            if (pitch != 0.0f)
            {
                Quaternion.CreateFromAxisAngle(ref WORLD_X_AXIS, pitch, out rotation);
                Quaternion.Concatenate(ref orientation, ref rotation, out orientation);
            }

            UpdateViewMatrix();
        }

        public void Update(GameTime gameTime, InputState state)
        {
            UpdateInput(state);
            UpdateCamera(gameTime, state);
            //frustum.Matrix = ViewProjectionMatrix;
        }

        /// <summary>
        /// Calculates the world transformation matrix for the weapon attached
        /// to the FirstPersonCamera. The weapon moves along with the camera.
        /// The offsets are to ensure the weapon is slightly in front of the
        /// camera and to one side.
        /// </summary>
        /// <param name="xOffset">How far to position the weapon left or right.</param>
        /// <param name="yOffset">How far to position the weapon up or down.</param>
        /// <param name="zOffset">How far to position the weapon in front or behind.</param>
        /// <returns>The weapon world transformation matrix.</returns>
        public Matrix WeaponWorldMatrix(float xOffset, float yOffset, float zOffset)
        {
            Vector3 weaponPos = eye;

            weaponPos += viewDir * zOffset;
            weaponPos += yAxis * yOffset;
            weaponPos += xAxis * xOffset;

            return Matrix.CreateRotationX(MathHelper.ToRadians(PitchDegrees))
                    * Matrix.CreateRotationY(MathHelper.ToRadians(HeadingDegrees))
                    * Matrix.CreateTranslation(weaponPos);
        }

        /// <summary>
        /// Calculates the world transformation matrix for the weapon attached
        /// to the FirstPersonCamera. The weapon moves along with the camera.
        /// The offsets are to ensure the weapon is slightly in front of the
        /// camera and to one side.
        /// </summary>
        /// <param name="xOffset">How far to position the weapon left or right.</param>
        /// <param name="yOffset">How far to position the weapon up or down.</param>
        /// <param name="zOffset">How far to position the weapon in front or behind.</param>
        /// <param name="scale">How much to scale the weapon.</param>
        /// <returns>The weapon world transformation matrix.</returns>
        public Matrix WeaponWorldMatrix(float xOffset, float yOffset, float zOffset, float scale)
        {
            Vector3 weaponPos = eye;

            weaponPos += viewDir * zOffset;
            weaponPos += yAxis * yOffset;
            weaponPos += xAxis * xOffset;

            return Matrix.CreateScale(scale)
                * Matrix.CreateRotationX(MathHelper.ToRadians(PitchDegrees))
                * Matrix.CreateRotationY(MathHelper.ToRadians(HeadingDegrees))
                * Matrix.CreateTranslation(weaponPos);
        }

        /// <summary>
        /// Returns a ray starting right from the center of the screen.
        /// </summary>
        public Ray GetMouseRay(Viewport viewport)
        {
            Vector2 mousePosition = Mouse.GetState().Position.ToVector2();
            Vector3 nearPoint = new Vector3(mousePosition, 0);
            Vector3 farPoint = new Vector3(mousePosition, 1);

            nearPoint = viewport.Unproject(nearPoint, projMatrix, viewMatrix, Matrix.Identity);
            farPoint = viewport.Unproject(farPoint, projMatrix, viewMatrix, Matrix.Identity);

            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();

            return new Ray(nearPoint, direction);
        }

        /// <summary>
        /// Returns a ray pointing straight down from the camera position.
        /// </summary>
        public Ray DownwardRay
        {
            get { return downwardRay; }
        }
        public Ray MovingRay()
        {
            Vector3 position = Position;
            Vector3 temp = currentVelocity/50;
            temp.Z = -temp.Z;
            temp.Y = 0;
            position += Vector3.Transform(temp, Matrix.CreateRotationY(MathHelper.ToRadians(HeadingDegrees)));
           
                    //Matrix.CreateRotationY(MathHelper.ToRadians(HeadingDegrees))
                    //* Matrix.CreateTranslation(weaponPos).;
            return new Ray(position, Vector3.Down);
        }

        public void Block()
        {
            currentVelocity = Vector3.Zero;
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Determines which way to move the camera based on player input.
        /// The returned values are in the range [-1,1].
        /// </summary>
        /// <param name="direction">The movement direction.</param>
        private void GetMovementDirection(out Vector3 direction, InputState state)
        {
            direction.X = 0.0f;
            direction.Y = 0.0f;
            direction.Z = 0.0f;

            if (state.IsKeyPressed(Keys.W))
            {
                if (!forwardsPressed)
                {
                    forwardsPressed = true;
                    currentVelocity.Z = 0.0f;
                }

                direction.Z += 1.0f;
            }
            else
            {
                forwardsPressed = false;
            }

            if (state.IsKeyPressed(Keys.S))
            {
                if (!backwardsPressed)
                {
                    backwardsPressed = true;
                    currentVelocity.Z = 0.0f;
                }

                direction.Z -= 1.0f;
            }
            else
            {
                backwardsPressed = false;
            }

            if (state.IsKeyPressed(Keys.D))
            {
                if (!strafeRightPressed)
                {
                    strafeRightPressed = true;
                    currentVelocity.X = 0.0f;
                }

                direction.X += 1.0f;
            }
            else
            {
                strafeRightPressed = false;
            }

            if (state.IsKeyPressed(Keys.A))
            {
                if (!strafeLeftPressed)
                {
                    strafeLeftPressed = true;
                    currentVelocity.X = 0.0f;
                }

                direction.X -= 1.0f;
            }
            else
            {
                strafeLeftPressed = false;
            }

            if (state.IsKeyPressed(Keys.LeftControl))
            {
                switch (posture)
                {
                    case Posture.Standing:
                        posture = Posture.Crouching;
                        direction.Y -= 1.0f;
                        currentVelocity.Y = 0.0f;
                        break;

                    case Posture.Crouching:
                        direction.Y -= 1.0f;
                        break;

                    case Posture.Rising:
                        // Finish rising before allowing another crouch.
                        direction.Y += 1.0f;
                        break;

                    default:
                        break;
                }
            }
            else
            {
                switch (posture)
                {
                    case Posture.Crouching:
                        posture = Posture.Rising;
                        direction.Y += 1.0f;
                        currentVelocity.Y = 0.0f;
                        break;

                    case Posture.Rising:
                        direction.Y += 1.0f;
                        break;

                    default:
                        break;
                }
            }

            if (state.IsNewKeyPress(Keys.Space))
            {
                switch (posture)
                {
                    case Posture.Standing:
                        posture = Posture.Jumping;
                        currentVelocity.Y = velocity.Y;
                        direction.Y += 1.0f;
                        break;

                    case Posture.Jumping:
                        direction.Y += 1.0f;
                        break;

                    default:
                        break;
                }
            }
            else
            {
                if (posture == Posture.Jumping)
                    direction.Y += 1.0f;
            }
        }

        /// <summary>
        /// Filters the mouse movement based on a weighted sum of mouse
        /// movement from previous frames.
        /// <para>
        /// For further details see:
        ///  Nettle, Paul "Smooth Mouse Filtering", flipCode's Ask Midnight column.
        ///  http://www.flipcode.com/cgi-bin/fcarticles.cgi?show=64462
        /// </para>
        /// </summary>
        /// <param name="x">Horizontal mouse distance from window center.</param>
        /// <param name="y">Vertice mouse distance from window center.</param>
        private void PerformMouseFiltering(float x, float y)
        {
            // Shuffle all the entries in the cache.
            // Newer entries at the front. Older entries towards the back.
            for (int i = mouseSmoothingCache.Length - 1; i > 0; --i)
            {
                mouseSmoothingCache[i].X = mouseSmoothingCache[i - 1].X;
                mouseSmoothingCache[i].Y = mouseSmoothingCache[i - 1].Y;
            }

            // Store the current mouse movement entry at the front of cache.
            mouseSmoothingCache[0].X = x;
            mouseSmoothingCache[0].Y = y;

            float averageX = 0.0f;
            float averageY = 0.0f;
            float averageTotal = 0.0f;
            float currentWeight = 1.0f;

            // Filter the mouse movement with the rest of the cache entries.
            // Use a weighted average where newer entries have more effect than
            // older entries (towards the back of the cache).
            for (int i = 0; i < mouseSmoothingCache.Length; ++i)
            {
                averageX += mouseSmoothingCache[i].X * currentWeight;
                averageY += mouseSmoothingCache[i].Y * currentWeight;
                averageTotal += 1.0f * currentWeight;
                currentWeight *= mouseSmoothingSensitivity;
            }

            // Calculate the new smoothed mouse movement.
            smoothedMouseMovement.X = averageX / averageTotal;
            smoothedMouseMovement.Y = averageY / averageTotal;
        }

        /// <summary>
        /// Averages the mouse movement over a couple of frames to smooth out
        /// the mouse movement.
        /// </summary>
        /// <param name="x">Horizontal mouse distance from window center.</param>
        /// <param name="y">Vertice mouse distance from window center.</param>
        private void PerformMouseSmoothing(float x, float y)
        {
            mouseMovement[mouseIndex].X = x;
            mouseMovement[mouseIndex].Y = y;

            smoothedMouseMovement.X = (mouseMovement[0].X + mouseMovement[1].X) * 0.5f;
            smoothedMouseMovement.Y = (mouseMovement[0].Y + mouseMovement[1].Y) * 0.5f;

            mouseIndex ^= 1;
            mouseMovement[mouseIndex].X = 0.0f;
            mouseMovement[mouseIndex].Y = 0.0f;
        }

        /// <summary>
        /// Dampens the rotation by applying the rotation speed to it.
        /// </summary>
        /// <param name="headingDegrees">Y axis rotation in degrees.</param>
        /// <param name="pitchDegrees">X axis rotation in degrees.</param>
        private void RotateSmoothly(float headingDegrees, float pitchDegrees)
        {
            headingDegrees *= rotationSpeed;
            pitchDegrees *= rotationSpeed;

            Rotate(headingDegrees, pitchDegrees);
        }

        private void UpdateCamera(GameTime gameTime, InputState state)
        {
            float elapsedTimeSec = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector3 direction = new Vector3();

            if (state.IsKeyPressed(Keys.LeftShift))
                velocity = velocityRunning;
            else
                velocity = velocityWalking;

            GetMovementDirection(out direction, state);

            RotateSmoothly(smoothedMouseMovement.X, smoothedMouseMovement.Y);
            UpdatePosition(ref direction, elapsedTimeSec);

            downwardRay = new Ray(Position, Vector3.Down);
        }

        private void UpdateInput(InputState state)
        {
            Rectangle clientBounds = Game.Window.ClientBounds;

            int centerX = clientBounds.Width / 2;
            int centerY = clientBounds.Height / 2;
            int deltaX = centerX - state.CurrentMouseState.X;
            int deltaY = centerY - state.CurrentMouseState.Y;

            Mouse.SetPosition(centerX, centerY);

            if (enableMouseSmoothing)
            {
                PerformMouseFiltering((float)deltaX, (float)deltaY);
                PerformMouseSmoothing(smoothedMouseMovement.X, smoothedMouseMovement.Y);
            }
            else
            {
                smoothedMouseMovement.X = (float)deltaX;
                smoothedMouseMovement.Y = (float)deltaY;
            }
        }

        /// <summary>
        /// Moves the camera based on player input.
        /// </summary>
        /// <param name="direction">Direction moved.</param>
        /// <param name="elapsedTimeSec">Elapsed game time.</param>
        private void UpdatePosition(ref Vector3 direction, float elapsedTimeSec)
        {
            if (currentVelocity.LengthSquared() != 0.0f)
            {
                // Only move the camera if the velocity vector is not of zero
                // length. Doing this guards against the camera slowly creeping
                // around due to floating point rounding errors.

                Vector3 displacement = (currentVelocity * elapsedTimeSec) +
                    (0.5f * acceleration * elapsedTimeSec * elapsedTimeSec);

                // Floating point rounding errors will slowly accumulate and
                // cause the camera to move along each axis. To prevent any
                // unintended movement the displacement vector is clamped to
                // zero for each direction that the camera isn't moving in.
                // Note that the UpdateVelocity() method will slowly decelerate
                // the camera's velocity back to a stationary state when the
                // camera is no longer moving along that direction. To account
                // for this the camera's current velocity is also checked.

                if (direction.X == 0.0f && (float)Math.Abs(currentVelocity.X) < 1e-6f)
                    displacement.X = 0.0f;

                if (direction.Y == 0.0f && (float)Math.Abs(currentVelocity.Y) < 1e-6f)
                    displacement.Y = 0.0f;

                if (direction.Z == 0.0f && (float)Math.Abs(currentVelocity.Z) < 1e-6f)
                    displacement.Z = 0.0f;

                Move(displacement.X, displacement.Y, displacement.Z);

                switch (posture)
                {
                    case Posture.Standing:
                        break;

                    case Posture.Crouching:
                        if (eye.Y < eyeHeightCrouching)
                            eye.Y = eyeHeightCrouching;
                        break;

                    case Posture.Rising:
                        if (eye.Y > eyeHeightStanding)
                        {
                            eye.Y = eyeHeightStanding;
                            posture = Posture.Standing;
                            currentVelocity.Y = 0.0f;
                        }
                        break;

                    case Posture.Jumping:
                        if (eye.Y < eyeHeightStanding)
                        {
                            eye.Y = eyeHeightStanding;
                            posture = Posture.Standing;
                            currentVelocity.Y = 0.0f;
                        }
                        break;
                }
            }

            // Continuously update the camera's velocity vector even if the
            // camera hasn't moved during this call. When the camera is no
            // longer being moved the camera is decelerating back to its
            // stationary state.

            UpdateVelocity(ref direction, elapsedTimeSec);
        }

        /// <summary>
        /// Updates the camera's velocity based on the supplied movement
        /// direction and the elapsed time (since this method was last
        /// called). The movement direction is the in the range [-1,1].
        /// </summary>
        /// <param name="direction">Direction moved.</param>
        /// <param name="elapsedTimeSec">Elapsed game time.</param>
        private void UpdateVelocity(ref Vector3 direction, float elapsedTimeSec)
        {
            if (direction.X != 0.0f)
            {
                // Camera is moving along the x axis.
                // Linearly accelerate up to the camera's max speed.

                currentVelocity.X += direction.X * acceleration.X * elapsedTimeSec;

                if (currentVelocity.X > velocity.X)
                    currentVelocity.X = velocity.X;
                else if (currentVelocity.X < -velocity.X)
                    currentVelocity.X = -velocity.X;
            }
            else
            {
                // Camera is no longer moving along the x axis.
                // Linearly decelerate back to stationary state.

                if (currentVelocity.X > 0.0f)
                {
                    if ((currentVelocity.X -= acceleration.X * elapsedTimeSec) < 0.0f)
                        currentVelocity.X = 0.0f;
                }
                else
                {
                    if ((currentVelocity.X += acceleration.X * elapsedTimeSec) > 0.0f)
                        currentVelocity.X = 0.0f;
                }
            }

            if (direction.Y != 0.0f)
            {
                // Camera is moving along the y axis. There are two cases here:
                // jumping and crouching. When jumping we're always applying a
                // negative acceleration to simulate the force of gravity.
                // However when crouching we apply a positive acceleration and
                // rely more on the direction.

                if (posture == Posture.Jumping)
                    currentVelocity.Y += direction.Y * -acceleration.Y * elapsedTimeSec;
                else
                    currentVelocity.Y += direction.Y * acceleration.Y * elapsedTimeSec;

                if (currentVelocity.Y > velocity.Y)
                    currentVelocity.Y = velocity.Y;
                else if (currentVelocity.Y < -velocity.Y)
                    currentVelocity.Y = -velocity.Y;
            }
            else
            {
                // Camera is no longer moving along the y axis.
                // Linearly decelerate back to stationary state.

                if (currentVelocity.Y > 0.0f)
                {
                    if ((currentVelocity.Y -= acceleration.Y * elapsedTimeSec) < 0.0f)
                        currentVelocity.Y = 0.0f;
                }
                else
                {
                    if ((currentVelocity.Y += acceleration.Y * elapsedTimeSec) > 0.0f)
                        currentVelocity.Y = 0.0f;
                }
            }

            if (direction.Z != 0.0f)
            {
                // Camera is moving along the z axis.
                // Linearly accelerate up to the camera's max speed.

                currentVelocity.Z += direction.Z * acceleration.Z * elapsedTimeSec;

                if (currentVelocity.Z > velocity.Z)
                    currentVelocity.Z = velocity.Z;
                else if (currentVelocity.Z < -velocity.Z)
                    currentVelocity.Z = -velocity.Z;
            }
            else
            {
                // Camera is no longer moving along the z axis.
                // Linearly decelerate back to stationary state.

                if (currentVelocity.Z > 0.0f)
                {
                    if ((currentVelocity.Z -= acceleration.Z * elapsedTimeSec) < 0.0f)
                        currentVelocity.Z = 0.0f;
                }
                else
                {
                    if ((currentVelocity.Z += acceleration.Z * elapsedTimeSec) > 0.0f)
                        currentVelocity.Z = 0.0f;
                }
            }
        }

        private void UpdateViewMatrix()
        {
            Matrix.CreateFromQuaternion(ref orientation, out viewMatrix);

            xAxis.X = viewMatrix.M11;
            xAxis.Y = viewMatrix.M21;
            xAxis.Z = viewMatrix.M31;

            yAxis.X = viewMatrix.M12;
            yAxis.Y = viewMatrix.M22;
            yAxis.Z = viewMatrix.M32;

            zAxis.X = viewMatrix.M13;
            zAxis.Y = viewMatrix.M23;
            zAxis.Z = viewMatrix.M33;

            viewMatrix.M41 = -Vector3.Dot(xAxis, eye);
            viewMatrix.M42 = -Vector3.Dot(yAxis, eye);
            viewMatrix.M43 = -Vector3.Dot(zAxis, eye);

            viewDir.X = -zAxis.X;
            viewDir.Y = -zAxis.Y;
            viewDir.Z = -zAxis.Z;

            frustum.Matrix = ViewProjectionMatrix;
        }

        #endregion

        #region Properties

        public Vector3 Acceleration
        {
            get { return acceleration; }
            set { acceleration = value; }
        }

        public Posture CurrentPosture
        {
            get { return posture; }
        }

        public Vector3 CurrentVelocity
        {
            get { return currentVelocity; }
        }

        public float CurrentY //w razie czego do wyjebania
        {
            get { return currentVelocity.Y; }
            set { currentVelocity.Y = currentVelocity.Y + value; }
        }

        public bool EnableMouseSmoothing
        {
            get { return enableMouseSmoothing; }
            set { enableMouseSmoothing = value; }
        }

        public float EyeHeightStanding
        {
            get { return eyeHeightStanding; }

            set
            {
                eyeHeightStanding = value;
                eyeHeightCrouching = value * HEIGHT_MULTIPLIER_CROUCHING;
                if(posture == Posture.Standing)//dodane
                {
                    eye.Y = eyeHeightStanding; //tylko to było normalnie, if'y itd. dodane
                }
                else if(posture == Posture.Crouching && CurrentVelocity.Y > velocity.Y)
                {
                    eye.Y = eyeHeightCrouching;//dodane
                }

                UpdateViewMatrix();
            }
        }

        public float HeadingDegrees
        {
            get { return -accumHeadingDegrees; }
        }

        public Quaternion Orientation
        {
            get { return orientation; }
        }

        public float PitchDegrees
        {
            get { return -accumPitchDegrees; }
        }

        public Vector3 Position
        {
            get { return eye; }

            set
            {
                eye = value;
                UpdateViewMatrix();
            }
        }

        public Matrix ProjectionMatrix
        {
            get { return projMatrix; }
        }

        public float RotationSpeed
        {
            get { return rotationSpeed; }
            set { rotationSpeed = value; }
        }

        public Vector3 VelocityWalking
        {
            get { return velocityWalking; }
            set { velocityWalking = value; }
        }

        public Vector3 VelocityRunning
        {
            get { return velocityRunning; }
            set { velocityRunning = value; }
        }

        public Vector3 ViewDirection
        {
            get { return viewDir; }
        }

        public Matrix ViewMatrix
        {
            get { return viewMatrix; }
        }

        public Matrix ViewProjectionMatrix
        {
            get { return viewMatrix * projMatrix; }
        }

        public Matrix worldMatrix
        {
            get { return Matrix.Invert(viewMatrix); }
        }

        public Vector3 XAxis
        {
            get { return xAxis; }
        }

        public Vector3 YAxis
        {
            get { return yAxis; }
        }

        public Vector3 ZAxis
        {
            get { return zAxis; }
        }

        public BoundingFrustum Frustum
        {
            get { return frustum; }
        }

        public Vector3[] FrustumCorners
        {
            get
            {
                Vector3[] farFrustumCornersVS = new Vector3[4];
                Vector3[] frustumCornersWS = Frustum.GetCorners();
                for (int i = 0; i < 4; i++)
                {
                    farFrustumCornersVS[i] = Vector3.Transform(frustumCornersWS[i + 4], ViewMatrix);
                }
                return farFrustumCornersVS;
            }
        }

        public float FarZ
        {
            get { return zfar; }
        }

        public float NearZ
        {
            get { return znear; }
        }

        public Octree Octree
        {
            get { return octree; }
            set { octree = value; }
        }
        #endregion
    }
}

