﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Game1
{
    class CameraTank
    {
        Vector3 posicao, direcao, target;
        float velocidade, time;
        float grausPorPixel = MathHelper.ToRadians(20) / 100;
        float diferencaX, diferencaY;
        float yaw, pitch, strafe;
        Vector3 vetorBase;
        public Matrix view, worldMatrix, projection;

        Matrix rotacao;

        VertexPositionNormalTexture[] vertices;
        int alturaMapa;
        MouseState posicaoRatoInicial;
        

        public CameraTank(GraphicsDeviceManager graphics, VertexPositionNormalTexture[] vertices, int alturaMapa, Vector3 posicaoTank, Matrix worldTank,Matrix tankView)
        {
            this.alturaMapa = alturaMapa;
            velocidade = 0.5f;
            vetorBase = new Vector3(1, 0, 0);
            this.vertices = vertices;
            posicao = new Vector3(1, findAltura(), 1);
            //posicao = posicaoTank;
            this.posicao = posicaoTank + new Vector3(0, 10, -20);
            direcao = vetorBase;
            worldMatrix = Matrix.Identity;
            float aspectRatio = (float)graphics.GraphicsDevice.Viewport.Width / graphics.GraphicsDevice.Viewport.Height;
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), aspectRatio, 0.1f, 1000.0f);
            Mouse.SetPosition(graphics.GraphicsDevice.Viewport.Height / 2, graphics.GraphicsDevice.Viewport.Width / 2);
            posicaoRatoInicial = Mouse.GetState();
            this.frente();
            //updateCamera(posicaoTank,worldTank,tankView);
        }

        //surface follow
        // metodo para descobrir os quatro vertices em redor da camara
        public float findAltura()
        {
            //A e B sao vertices superiores, C e D sao os vertices inferiores
            //A-----------B
            //C-----------D
            int xA, zA, xB, zB, xC, zC, xD, zD;
            float yA = 0, yB = 0, yC = 0, yD = 0;
            xA = (int)this.posicao.X;
            zA = (int)this.posicao.Z;

            xB = xA + 1;
            zB = zA;

            xC = xA;
            zC = zA + 1;

            xD = xB;
            zD = zC;

            //encontrar valor de Y de cada vertice

            yA = vertices[xA * alturaMapa + zA].Position.Y;
            yB = vertices[xB * alturaMapa + zB].Position.Y;
            yC = vertices[xC * alturaMapa + zC].Position.Y;
            yD = vertices[xD * alturaMapa + zD].Position.Y;



            //calcular nova altura da camara
            float yAB, yCD, cameraY;

            yAB = (1 - (this.posicao.X - xA)) * yA + (this.posicao.X - xA) * yB;
            yCD = (1 - (this.posicao.X - xC)) * yC + (this.posicao.X - xC) * yD;
            cameraY = (1 - (this.posicao.Z - zA)) * yAB + (this.posicao.Z - zA) * yCD;
            return (cameraY + 4);
        }

        //surface follow end

        //Movimento da camara
        public void frente()
        {
            posicao.Y = findAltura();
            //time = gameTime.ElapsedGameTime.Milliseconds;
            posicao = posicao + velocidade * direcao;
            target = posicao + direcao;//posicao + direcao;

        }

        public void moverTras(GameTime gameTime)
        {
            posicao.Y = findAltura();
            time = gameTime.ElapsedGameTime.Milliseconds;
            posicao = posicao - velocidade * direcao;
            target = posicao + direcao;//posicao + direcao;
        }


        public void rodarEsquerda(GameTime gameTime)
        {
            time = gameTime.ElapsedGameTime.Milliseconds;
            //yaw = yaw + velocidade;//(yaw + velocidade);
            yaw -= diferencaX * grausPorPixel;
        }

        public void rodarDireita(GameTime gameTime)
        {
            time = gameTime.ElapsedGameTime.Milliseconds;
            //yaw = yaw - velocidade;//(yaw - velocidade);
            yaw -= diferencaX * grausPorPixel;
        }

        public void rodarCima(GameTime gameTime)
        {
            time = gameTime.ElapsedGameTime.Milliseconds;
            //pitch = pitch + 0.01f;
            pitch -= diferencaY * grausPorPixel;
        }

        public void rodarBaixo(GameTime gameTime)
        {
            time = gameTime.ElapsedGameTime.Milliseconds;
            //pitch = pitch - 0.01f;
            pitch -= diferencaY * grausPorPixel;
        }

        public void strafeEsquerda(GameTime gameTime, float strafe)
        {
            posicao.Y = findAltura();
            time = gameTime.ElapsedGameTime.Milliseconds;
            this.strafe = strafe + velocidade * time;
            posicao = posicao - velocidade * Vector3.Cross(direcao, Vector3.Up);

            target = posicao + direcao;

        }

        public void strafeDireita(GameTime gameTime, float strafe)
        {
            posicao.Y = findAltura();
            time = gameTime.ElapsedGameTime.Milliseconds;
            this.strafe = strafe + velocidade * time;
            posicao = posicao + velocidade * Vector3.Cross(direcao, Vector3.Up);

            target = posicao + direcao;

        }

        public void UpdateInput(GameTime gameTime, GraphicsDeviceManager graphics, Vector3 posicaoTank, Matrix worldTank)
        {

            verificarLimites();
            KeyboardState kb = Keyboard.GetState();

            if (kb.IsKeyDown(Keys.W))
            {
                this.frente();
                view = Matrix.CreateLookAt(posicao, target, Vector3.Up);
            }
            if (kb.IsKeyDown(Keys.S))
            {
                this.moverTras(gameTime);
                view = Matrix.CreateLookAt(posicao, target, Vector3.Up);
            }
            if (kb.IsKeyDown(Keys.Q))
            {
                this.strafeEsquerda(gameTime, 0.08f);
                view = Matrix.CreateLookAt(posicao, target, Vector3.Up);
            }
            if (kb.IsKeyDown(Keys.E))
            {
                this.strafeDireita(gameTime, 0.08f);
                view = Matrix.CreateLookAt(posicao, target, Vector3.Up);
            }


            MouseState mouseState = Mouse.GetState();
            if (mouseState != posicaoRatoInicial)
            {
                diferencaX = mouseState.Position.X - posicaoRatoInicial.Position.X;
                diferencaY = mouseState.Position.Y - posicaoRatoInicial.Position.Y;
                if (mouseState.X < posicaoRatoInicial.X)
                {
                    this.rodarEsquerda(gameTime);
                }
                if (mouseState.X > posicaoRatoInicial.X || kb.IsKeyDown(Keys.Right))
                {
                    this.rodarDireita(gameTime);
                }
                if (mouseState.Y > posicaoRatoInicial.Y || kb.IsKeyDown(Keys.Down))
                {
                    this.rodarBaixo(gameTime);

                }
                if (mouseState.Y < posicaoRatoInicial.Y || kb.IsKeyDown(Keys.Up))
                {
                    this.rodarCima(gameTime);
                }
                try
                {
                    Mouse.SetPosition(graphics.GraphicsDevice.Viewport.Height / 2, graphics.GraphicsDevice.Viewport.Width / 2);
                }
                catch (Exception e)
                { }
                //updateCamera(posicaoTank, worldTank);
                
                
            }


        }

        public void updateCamera( Vector3 posicaoTank, Matrix worldTank, Matrix viewTank, Tank tank)
        {

            //rotacao = Matrix.CreateFromYawPitchRoll(yaw, 0, pitch);
            KeyboardState kb = Keyboard.GetState();
            if (kb.IsKeyDown(Keys.D) )
            {
                //rotacao += Matrix.CreateRotationY(MathHelper.ToRadians( tank.rotacaoY));
                //this.posicao.Z -= 2;
                //this.posicao.Y += 5;
            }
            if (kb.IsKeyDown(Keys.A))
            {
                //rotacao -= Matrix.CreateRotationY(MathHelper.ToRadians(-tank.rotacaoY));
                //this.posicao.Z -= 2;
                //this.posicao.Y += 5;
            }
            if(  kb.IsKeyDown(Keys.S) || kb.IsKeyDown(Keys.W))
            {

            }
            //worldMatrix = rotacao;
            
            //direcao = Vector3.Transform(vetorBase, rotacao);
            //target = posicao + direcao;
            //Matrix rotate = Matrix.CreateRotationY(MathHelper.ToRadians(tank.rotacaoY));
            //posicao = Vector3.Transform(posicao, rotate);
            ////this.posicao = posicaoTank + new Vector3(0, 10, -20);
            //worldMatrix = worldTank;
            //view = Matrix.CreateLookAt(posicao, posicaoTank, Vector3.Up);
            
            //this.posicao.Y = posicaoTank.Y;
            Vector3 offset = new Vector3(0,20,-20);
            rotacao = Matrix.CreateRotationY(MathHelper.ToRadians(tank.rotacaoY));
            Vector3 transformOffset = Vector3.Transform(offset, rotacao);
            posicao = transformOffset + posicaoTank;
            
            view = Matrix.CreateLookAt(posicao, posicaoTank,Vector3.Up);

           
        }

        public void verificarLimites()
        {
            //verificar se esta fora do terreno
            if (this.posicao.X - 1 < 0)
            {
                this.posicao.X += 0.5f;
            }
            if (this.posicao.Z - 1 < 0)
            {
                this.posicao.Z += 0.5f;
            }
            if (this.posicao.X + 1 > 127)
            {
                this.posicao.X -= 0.5f;
            }
            if (this.posicao.Z + 1 > 127)
            {
                this.posicao.Z -= 0.5f;
            }
        }
    }
}
//vector3.outerproduct -> dir*normal=andarlado
//vector3.transform


//surface follow
//usar o xyz do terreno no cpu para usar na posicao da camara
//usar uma matriz para guardar as coordenadas dos vertices para usar na camara
//float alturas[]alturas
//procurar a posicao da camara no array, y=alturas[z*larguraTerreno+x]+offset
//camara raranente vai estar em cima de um vertice
//esta num sitio algures entre 4 vertices
//para descobrir os vertices fazer um cast para int das corrdenadas da camara. do x e do z
//interpolacao bilinear
//fazer primeiro uma dimensao x e depois noutra em z
//a*---------------------x---------b*
//yab
//x -valor de y>
//c*---------------------x---------d*
//ycb
//a coordenada x de a=cast para int do x da camara
//z vvertc a=int z da camara
//xb=xa+1
//zb=za
//ya
//yb
//yc
//yd
//Yab=(1-(X-Xa))Ya+(x-Xa)Ya
//Ycd=(1-(X-Xc))Yc+(x-Xc)Yd
//Y=(1-(z-za))Yab+(z-za)Ycd
//camara nao pode sair do terreno

// para a camara rodar mais suavemente
//dif x
//grausporpixel=10graus/100
//yaw+=difx*grauporpixel
//createfromyawpitchroll(mathhelper.toradians(yaw)


