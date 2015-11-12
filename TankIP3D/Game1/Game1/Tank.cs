#region File Description
//-----------------------------------------------------------------------------
// Tank.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
#endregion

namespace Game1
{
    /// <summary>
    /// Helper class for drawing a tank model with animated wheels and turret.
    /// </summary>
    public class Tank
    {
        #region Fields


        // The XNA framework Model object that we are going to display.
        Model tankModel;
        //  Matrizes do modelo
        public Matrix world; //  Posiciona o cubo "sobre" o plano
        public Matrix view;
        public Matrix projection;
        // device onde ser� desenhado o tanque
        private GraphicsDevice device;
        private Vector3 direcao,vetorBase,target;
        public Vector3 position;
        public VertexPositionNormalTexture[] vertices;
        public int larguraMapa;
        public Vector3 newNormal;
        public float newAltura;
        float yaw, pitch, roll, rotacaoY, velocidade;
        Matrix rotacao;
        // Shortcut references to the bones that we are going to animate.
        // We could just look these up inside the Draw method, but it is more
        // efficient to do the lookups while loading and cache the results.
        ModelBone leftBackWheelBone;
        ModelBone rightBackWheelBone;
        ModelBone leftFrontWheelBone;
        ModelBone rightFrontWheelBone;
        ModelBone leftSteerBone;
        ModelBone rightSteerBone;
        ModelBone turretBone;
        ModelBone cannonBone;
        ModelBone hatchBone;


        // Store the original transform matrix for each animating bone.
        Matrix leftBackWheelTransform;
        Matrix rightBackWheelTransform;
        Matrix leftFrontWheelTransform;
        Matrix rightFrontWheelTransform;
        Matrix leftSteerTransform;
        Matrix rightSteerTransform;
        Matrix turretTransform;
        Matrix cannonTransform;
        Matrix hatchTransform;

        
        // Array holding all the bone transform matrices for the entire model.
        // We could just allocate this locally inside the Draw method, but it
        // is more efficient to reuse a single array, as this avoids creating
        // unnecessary garbage.
        Matrix[] boneTransforms;


        // Current animation positions.
        float wheelRotationValue;
        float steerRotationValue;
        float turretRotationValue;
        float cannonRotationValue;
        float hatchRotationValue;


        #endregion

        #region Properties


        /// <summary>
        /// Gets or sets the wheel rotation amount.
        /// </summary>
        public float WheelRotation
        {
            get { return wheelRotationValue; }
            set { wheelRotationValue = value; }
        }


        /// <summary>
        /// Gets or sets the steering rotation amount.
        /// </summary>
        public float SteerRotation
        {
            get { return steerRotationValue; }
            set { steerRotationValue = value; }
        }


        /// <summary>
        /// Gets or sets the turret rotation amount.
        /// </summary>
        public float TurretRotation
        {
            get { return turretRotationValue; }
            set { turretRotationValue = value; }
        }


        /// <summary>
        /// Gets or sets the cannon rotation amount.
        /// </summary>
        public float CannonRotation
        {
            get { return cannonRotationValue; }
            set { cannonRotationValue = value; }
        }


        /// <summary>
        /// Gets or sets the entry hatch rotation amount.
        /// </summary>
        public float HatchRotation
        {
            get { return hatchRotationValue; }
            set { hatchRotationValue = value; }
        }


        #endregion

        public Tank(GraphicsDevice graphicsDevice, VertexPositionNormalTexture[] vert, int larguraMapa)
        {
            //world = Matrix.CreateScale(0.01f);
            velocidade = 0.1f;
            device = graphicsDevice;
            position = new Vector3(10, 12, 10);
            vertices = vert;
            this.larguraMapa = larguraMapa;
            vetorBase = new Vector3(1, 0, 0);
            direcao = vetorBase;
            target = position + direcao;
            //world = Matrix.CreateRotationY(MathHelper.ToRadians(90)) * Matrix.CreateScale(.01f) * Matrix.CreateTranslation(position);
            world =  Matrix.CreateScale(0.01f) * Matrix.CreateTranslation(position);
            
        }

        /// <summary>
        /// Loads the tank model.
        /// </summary>
        public void LoadContent(ContentManager content )
        {
            // Load the tank model from the ContentManager.
            tankModel = content.Load<Model>("tank");
            

            // Look up shortcut references to the bones we are going to animate.
            leftBackWheelBone = tankModel.Bones["l_back_wheel_geo"];
            rightBackWheelBone = tankModel.Bones["r_back_wheel_geo"];
            leftFrontWheelBone = tankModel.Bones["l_front_wheel_geo"];
            rightFrontWheelBone = tankModel.Bones["r_front_wheel_geo"];
            leftSteerBone = tankModel.Bones["l_steer_geo"];
            rightSteerBone = tankModel.Bones["r_steer_geo"];
            turretBone = tankModel.Bones["turret_geo"];
            cannonBone = tankModel.Bones["canon_geo"];
            hatchBone = tankModel.Bones["hatch_geo"];

            // Store the original transform matrix for each animating bone.
            leftBackWheelTransform = leftBackWheelBone.Transform;
            rightBackWheelTransform = rightBackWheelBone.Transform;
            leftFrontWheelTransform = leftFrontWheelBone.Transform;
            rightFrontWheelTransform = rightFrontWheelBone.Transform;
            leftSteerTransform = leftSteerBone.Transform;
            rightSteerTransform = rightSteerBone.Transform;
            turretTransform = turretBone.Transform;
            cannonTransform = cannonBone.Transform;
            hatchTransform = hatchBone.Transform;

            // Allocate the transform matrix array.
            boneTransforms = new Matrix[tankModel.Bones.Count];
            
            //  Define as matrizes de proje��o e view
            Viewport viewport = device.Viewport;
            float aspectRatio = (float)viewport.Width / (float)viewport.Height;      
            
            view = Matrix.CreateLookAt(new Vector3(0, 10, 10),Vector3.Zero,Vector3.Up);
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,aspectRatio,1,100);

            
        }

        public void Update()
        {
            //position = world.Translation;
            //view = cameraView;
            //projection = cameraProjection;
            HandleTankInput(0.5f);
            //UpdateInput();
        }


        /// <summary>
        /// Draws the tank model, using the current animation settings.
        /// </summary>
        public void Draw(Matrix cameraView, Matrix cameraProjection)
        {
            findNormal();
            // Set the world matrix as the root transform of the model.

            tankModel.Root.Transform = world; 
            //tankModel.Root.Transform =  Matrix.CreateScale(0.01f) * Matrix.CreateTranslation(position);
            

            // Calculate matrices based on the current animation position.
            Matrix wheelRotation = Matrix.CreateRotationX(wheelRotationValue);
            Matrix steerRotation = Matrix.CreateRotationY(steerRotationValue);
            Matrix turretRotation = Matrix.CreateRotationY(turretRotationValue);
            Matrix cannonRotation = Matrix.CreateRotationX(cannonRotationValue);
            Matrix hatchRotation = Matrix.CreateRotationX(hatchRotationValue);

            // Apply matrices to the relevant bones.
            leftBackWheelBone.Transform = wheelRotation * leftBackWheelTransform;
            rightBackWheelBone.Transform = wheelRotation * rightBackWheelTransform;
            leftFrontWheelBone.Transform = wheelRotation * leftFrontWheelTransform;
            rightFrontWheelBone.Transform = wheelRotation * rightFrontWheelTransform;
            leftSteerBone.Transform = steerRotation * leftSteerTransform;
            rightSteerBone.Transform = steerRotation * rightSteerTransform;
            turretBone.Transform = turretRotation * turretTransform;
            cannonBone.Transform = cannonRotation * cannonTransform;
            hatchBone.Transform = hatchRotation * hatchTransform;

            // Look up combined bone matrices for the entire model.
            tankModel.CopyAbsoluteBoneTransformsTo(boneTransforms);

            view = cameraView;
            projection = cameraProjection;

            // Draw the model.
            foreach (ModelMesh mesh in tankModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = boneTransforms[mesh.ParentBone.Index];
                    effect.View = view;
                    effect.Projection = projection;
                    
                    effect.EnableDefaultLighting();
                }  
                mesh.Draw();
            }
        }

        public void findNormal()
        {
            //A e B sao vetores superiores, C e D sao os vetores inferiores
            //A-----------B
            //C-----------D

            int xA, zA, xB, zB, xC, zC, xD, zD;
            //int A,B,C,D;
            float yA = 0, yB = 0, yC = 0, yD = 0;
            Vector3 normalA, normalB, normalC, normalD;

            
            xA = (int)this.position.X;
            zA = (int)this.position.Z;

            xB = xA + 1;
            zB = zA;

            xC = xA;
            zC = zA + 1;

            xD = xB;
            zD = zC;

            
            //encontrar altura para o tanque
            yA = vertices[xA * larguraMapa + zA].Position.Y;
            yB = vertices[xB * larguraMapa + zB].Position.Y;
            yC = vertices[xC * larguraMapa + zC].Position.Y;
            yD = vertices[xD * larguraMapa + zD].Position.Y;
            //encontrar valor de normal de cada vertice
            normalA = vertices[xA * larguraMapa + zA].Normal;
            normalB = vertices[xB * larguraMapa + zB].Normal;
            normalC = vertices[xC * larguraMapa + zC].Normal;
            normalD = vertices[xD * larguraMapa + zD].Normal;

            //calcular novo vector.up
            Vector3 normalAB, normalCD;

            normalAB = (1-(this.position.X - xA)) * normalA + (this.position.X - xA) * normalB;
            normalCD = (1 - (this.position.X - xC)) * normalC + (this.position.X - xC) * normalD;

            //this.world.Up 
            newNormal = (1 - (this.position.Z - zA)) * normalAB + (this.position.Z - zA) * normalCD;


            //calcular nova altura da tanque
            float yAB, yCD, cameraY;

            yAB = (1 - (this.position.X - xA)) * yA + (this.position.X - xA) * yB;
            yCD = (1 - (this.position.X - xC)) * yC + (this.position.X - xC) * yD;
            newAltura = (1 - (this.position.Z - zA)) * yAB + (this.position.Z - zA) * yCD;
            
            //cameraY = (1 - (this.posicao.Z - zA)) * yAB + (this.posicao.Z - zA) * yCD;
            //return (cameraY + 1);
        }

        private void HandleTankInput(float time)
        {
            KeyboardState currentKeyboardState = Keyboard.GetState();

            //  Roda as rodas to tanque
            this.WheelRotation = time * 5;

            //  Move torre (s� at� 90 graus)
            if (currentKeyboardState.IsKeyDown(Keys.Left))
            {
                if (this.TurretRotation < 1.6f)
                    this.TurretRotation += 0.1f;
            }
            if (currentKeyboardState.IsKeyDown(Keys.Right))
            {
                if (this.TurretRotation > -1.6f)
                    this.TurretRotation -= 0.1f;
            }

            //  Move canh�o (sem atirar 90 graus nem no pr�prio tanque)
            if (currentKeyboardState.IsKeyDown(Keys.Up))
            {
                if (this.CannonRotation > -0.8f)
                    this.CannonRotation -= 0.1f;

            }
            if (currentKeyboardState.IsKeyDown(Keys.Down))
            {
                if (this.CannonRotation < 0.2f)
                    this.CannonRotation += 0.1f;
            }

            //  Abre e fecha porta
            if (currentKeyboardState.IsKeyDown(Keys.PageUp))
            {
                this.HatchRotation = -1;
            }
            if (currentKeyboardState.IsKeyDown(Keys.PageDown))
            {
                this.HatchRotation = 0;
            }

            if (currentKeyboardState.IsKeyDown(Keys.A))
            {
                this.world *= Matrix.CreateTranslation(new Vector3(0.0f, 0.0f, -0.1f));
            }

            if (currentKeyboardState.IsKeyDown(Keys.D))
            {

                position += direcao * velocidade;
                
            }
            position.Y = newAltura;
            rotacao = Matrix.CreateRotationY(MathHelper.ToRadians(-90)) * Matrix.CreateRotationY(MathHelper.ToRadians(rotacaoY));
            direcao = Vector3.Transform(vetorBase, rotacao);
            world = Matrix.CreateScale(0.01f) * rotacao * Matrix.CreateTranslation(position);
            Vector3 newRigth = Vector3.Cross(newNormal, direcao);
            Matrix rotacaoUp = Matrix.CreateWorld(position, Vector3.Cross(newNormal,newRigth), newNormal);
            world = Matrix.CreateScale(0.01f) * rotacaoUp;
        }

        public void UpdateInput()
        {

            KeyboardState keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Right))
            {
                rotacaoY -= 1f;

            }
            if (keyboardState.IsKeyDown(Keys.Left))
            {
                rotacaoY += 1f;

            }
            if (keyboardState.IsKeyDown(Keys.Up))
            {
                position = position + direcao * velocidade;
            }
            if (keyboardState.IsKeyDown(Keys.Down))
            {
                position = position - direcao * velocidade;
            }
            position.Y = newAltura;
            rotacao = Matrix.CreateRotationY(MathHelper.ToRadians(rotacaoY));
            direcao = Vector3.Transform(vetorBase, rotacao);
            world = rotacao * Matrix.CreateTranslation(position) * Matrix.CreateScale(0.01f);


        }

        //rotacao = Matrix.CreateFromYawPitchRoll(yaw, 0, pitch);
        //        worldMatrix = rotacao;
        //        direcao = Vector3.Transform(vetorBase, rotacao);
        //        target = posicao + direcao;
        //        view = Matrix.CreateLookAt(posicao, target, Vector3.Up);

         //rotacao = Matrix.CreateRotationY(MathHelper.ToRadians(rotacaoY));
         //   direcao = Vector3.Transform(vetorBase, rotacao);
         //   worldMatrix = rotacao * Matrix.CreateTranslation(posicao);

    }
}