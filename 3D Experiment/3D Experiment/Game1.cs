using System;
using System.Collections.Generic;
using CoreXNA;
using Experiment.ParticleSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace WindowsGame1
{
    public struct VertexPositionColorNormal
    {
        public Vector3 Position;
        public Color Color;
        public Vector3 Normal;

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(sizeof(float) * 3 + 4, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
        );
    }

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        short[][] indices;
        VertexPositionColorNormal[][] vertices;

        BasicEffect basicEffect;
        Matrix world = Matrix.CreateTranslation(0, 0, 0);
        Matrix view = Matrix.CreateLookAt(new Vector3(0, 0, 3), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
        Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 800f / 480f, 0.01f, 100f);
        double angle = MathHelper.ToRadians(0);

        List<Mesh> ListMesh;

        ParticleSystem LedorSystem;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            ListMesh = new List<Mesh>();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            basicEffect = new BasicEffect(GraphicsDevice);

            LedorSystem = new ParticleSystem("Sans titre", 10000, 1, BlendState.NonPremultiplied, false, Content, GraphicsDevice);
            LedorSystem.SetViewProjection(
            Matrix.CreateLookAt(
                new Vector3(50 * (float)Math.Sin(angle), 2, 50 * (float)Math.Cos(angle)),
                new Vector3(0, 0, 0),
                Vector3.UnitY),
            projection);

            var sphere1 = Solids.Sphere(new SphereOptions { Radius = 1, Center = new Vector3(-0.5f, 0, 0), SolidColor = Color.White });
            var sphere2 = Solids.Sphere(new SphereOptions { Radius = 1, Center = new Vector3(0.5f, 0, 0), SolidColor = Color.Red });
            var sphere1XNA = sphere1.Substract(sphere2);

            ListMesh = Convertor.csgToMeshesWithCache(sphere1XNA);

            vertices = new VertexPositionColorNormal[ListMesh.Count][];
            indices = new short[ListMesh.Count][];

            for (int M = 0; M < ListMesh.Count; ++M)
            {

                Mesh ActiveMesh = ListMesh[M];
                vertices[M] = new VertexPositionColorNormal[ActiveMesh.vertices.Count];

                for (int V = 0; V < ActiveMesh.vertices.Count; ++V)
                {
                    vertices[M][V] = new VertexPositionColorNormal()
                    {
                        Position = ActiveMesh.vertices[V],
                        Color = Color.FromNonPremultiplied((int)ActiveMesh.colors[V][0],
                        (int)ActiveMesh.colors[V][1],
                        (int)ActiveMesh.colors[V][2],
                        (int)ActiveMesh.colors[V][3]),
                        Normal = new Vector3()
                    };
                }


                indices[M] = new short[ActiveMesh.triangles.Count * 3];

                for (int T = 0; T < ActiveMesh.triangles.Count; ++T)
                {

                    indices[M][T * 3] = (short)ActiveMesh.triangles[T][0];
                    indices[M][T * 3 + 1] = (short)ActiveMesh.triangles[T][1];
                    indices[M][T * 3 + 2] = (short)ActiveMesh.triangles[T][2];
                }

                CalculateNormals(vertices[M], indices[M]);
            }
        }

        private void CalculateNormals(VertexPositionColorNormal[] vertices, short[] indices)
        {
            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal = new Vector3(0, 0, 0);

            for (int i = 0; i < indices.Length / 3; i++)
            {
                int index1 = indices[i * 3];
                int index2 = indices[i * 3 + 1];
                int index3 = indices[i * 3 + 2];

                Vector3 side1 = vertices[index1].Position - vertices[index3].Position;
                Vector3 side2 = vertices[index1].Position - vertices[index2].Position;
                Vector3 normal = Vector3.Cross(side1, side2);

                vertices[index1].Normal += normal;
                vertices[index2].Normal += normal;
                vertices[index3].Normal += normal;
            }

            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal.Normalize();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            LedorSystem.AddParticle(new Vector3(0, 50, 0));

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            angle += 0.01f;
            view = Matrix.CreateLookAt(
                new Vector3(5 * (float)Math.Sin(angle), -2, 5 * (float)Math.Cos(angle)),
                new Vector3(0, 0, 0),
                Vector3.UnitY);

            LedorSystem.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            basicEffect.World = world;
			basicEffect.View = view;
			basicEffect.Projection = projection;
			basicEffect.VertexColorEnabled = true;
			basicEffect.LightingEnabled = true; // turn on the lighting subsystem.
			basicEffect.DirectionalLight0.Enabled = true;
			basicEffect.DirectionalLight0.DiffuseColor = new Vector3 (1, 1, 1); // a red light
			basicEffect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3 (1, 1, 1));  // coming along the x-axis
			basicEffect.DirectionalLight0.SpecularColor = new Vector3 (0, 1, 0); // with green highlights

			for (int M = 0; M < ListMesh.Count; ++M) {

				RasterizerState rasterizerState = new RasterizerState ();
				rasterizerState.CullMode = CullMode.None;
				GraphicsDevice.RasterizerState = rasterizerState;

				foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes) {
					pass.Apply ();
					GraphicsDevice.DrawUserIndexedPrimitives (PrimitiveType.TriangleList, vertices[M], 0, vertices[M].Length, indices[M], 0, indices[M].Length / 3, VertexPositionColorNormal.VertexDeclaration);

				}
			}

            //LedorSystem.Draw(GraphicsDevice);

            base.Draw(gameTime);
        }
    }
}
