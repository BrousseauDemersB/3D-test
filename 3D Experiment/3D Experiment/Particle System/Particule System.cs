using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Experiment.ParticleSystem
{
    public class ParticleSystem
    {
        #region Fields

        private ParticleSystemSettings Settings;
        private Vector2 ParticleSize;

        // Shortcuts for accessing frequently changed effect parameters.
        private EffectParameter EffectViewParameter;
        private EffectParameter EffectProjectionParameter;
        private EffectParameter EffectViewportScaleParameter;
        private EffectParameter EffectTimeParameter;
        private EffectParameterCollection Parameters;
        public bool UseAlphaBlend;

        // Settings class controls the appearance and animation of this particle system.
        // Custom effect for drawing particles. This computes the particle
        // animation entirely in the vertex shader: no per-particle CPU work required!
        private Effect ParticleEffect;
        // An array of particles, treated as a circular queue.
        private ParticleVertex[] ArrayParticles;
        // A vertex buffer holding our particles. This contains the same data as
        // the particles array, but copied across to where the GPU can access it.
        private DynamicVertexBuffer VertexBuffer;
        // Index buffer turns sets of four vertices into particle quads (pairs of triangles).
        private IndexBuffer IndexBuffer;
        // The particles array and vertex buffer are treated as a circular queue.
        // Initially, the entire contents of the array are free, because no particles
        // are in use. When a new particle is created, this is allocated from the
        // beginning of the array. If more than one particle is created, these will
        // always be stored in a consecutive block of array elements. Because all
        // particles last for the same amount of time, old particles will always be
        // removed in order from the start of this active particle region, so the
        // active and free regions will never be intermingled. Because the queue is
        // circular, there can be times when the active particle region wraps from the
        // end of the array back to the start. The queue uses modulo arithmetic to
        // handle these cases. For instance with a four entry queue we could have:
        //
        //      0
        //      1 - first active particle
        //      2
        //      3 - first free particle
        //
        // In this case, particles 1 and 2 are active, while 3 and 4 are free.
        // Using modulo arithmetic we could also have:
        //
        //      0
        //      1 - first free particle
        //      2
        //      3 - first active particle
        //
        // Here, 3 and 0 are active, while 1 and 2 are free.
        //
        // But wait! The full story is even more complex.
        //
        // When we create a new particle, we add them to our managed particles array.
        // We also need to copy this new data into the GPU vertex buffer, but we don't
        // want to do that straight away, because setting new data into a vertex buffer
        // can be an expensive operation. If we are going to be adding several particles
        // in a single frame, it is faster to initially just store them in our managed
        // array, and then later upload them all to the GPU in one single call. So our
        // queue also needs a region for storing new particles that have been added to
        // the managed array but not yet uploaded to the vertex buffer.
        //
        // Another issue occurs when old particles are retired. The CPU and GPU run
        // asynchronously, so the GPU will often still be busy drawing the previous
        // frame while the CPU is working on the next frame. This can cause a
        // synchronization problem if an old particle is retired, and then immediately
        // overwritten by a new one, because the CPU might try to change the contents
        // of the vertex buffer while the GPU is still busy drawing the old data from
        // it. Normally the graphics driver will take care of this by waiting until
        // the GPU has finished drawing inside the VertexBuffer.SetData call, but we
        // don't want to waste time waiting around every time we try to add a new
        // particle! To avoid this delay, we can specify the SetDataOptions.NoOverwrite
        // flag when we write to the vertex buffer. This basically means "I promise I
        // will never try to overwrite any data that the GPU might still be using, so
        // you can just go ahead and update the buffer straight away". To keep this
        // promise, we must avoid reusing vertices immediately after they are drawn.
        //
        // So in total, our queue contains four different regions:
        //
        // Vertices between firstActiveParticle and firstNewParticle are actively
        // being drawn, and exist in both the managed particles array and the GPU
        // vertex buffer.
        //
        // Vertices between firstNewParticle and firstFreeParticle are newly created,
        // and exist only in the managed particles array. These need to be uploaded
        // to the GPU at the start of the next draw call.
        //
        // Vertices between firstFreeParticle and firstRetiredParticle are free and
        // waiting to be allocated.
        //
        // Vertices between firstRetiredParticle and firstActiveParticle are no longer
        // being drawn, but were drawn recently enough that the GPU could still be
        // using them. These need to be kept around for a few more frames before they
        // can be reallocated.
        private int FirstActiveParticle;
        private int FirstNewParticle;
        private int FirstFreeParticle;
        private int FirstRetiredParticle;
        // Store the current time, in seconds.
        private float CurrentTime;
        // Count how many times Draw has been called. This is used to know
        // when it is safe to retire old particles back into the free list.
        private int DrawCounter;

        #endregion

        public ParticleSystem(string TextureName, int MaxParticles, int NumberOfImages, BlendState BlendState, bool RotateTowardCamera, ContentManager Content, GraphicsDevice GraphicsDevice)
        {
            ParticleSystemSettings NewParticleSystemSettings = new ParticleSystemSettings(TextureName, MaxParticles, NumberOfImages, BlendState, RotateTowardCamera);
            Init(NewParticleSystemSettings, Content, GraphicsDevice);
        }

        private void Init(ParticleSystemSettings Settings, ContentManager Content, GraphicsDevice GraphicsDevice)
        {
            this.Settings = Settings;
            UseAlphaBlend = false;

            // Allocate the particle array, and fill in the corner fields (which never change).
            ArrayParticles = new ParticleVertex[Settings.MaxParticles * 4];
            for (int i = 0; i < Settings.MaxParticles; i++)
            {
                ArrayParticles[i * 4 + 0].UV = new Vector2(0, 0);
                ArrayParticles[i * 4 + 1].UV = new Vector2(1, 0);
                ArrayParticles[i * 4 + 2].UV = new Vector2(1, 1);
                ArrayParticles[i * 4 + 3].UV = new Vector2(0, 1);
            }
            Effect effect = Content.Load<Effect>("Shaders/Particle shader 3D");
            // If we have several particle systems, the content manager will return
            // a single shared effect instance to them all. But we want to preconfigure
            // the effect with parameters that are specific to this particular
            // particle system. By cloning the effect, we prevent one particle system
            // from stomping over the parameter settings of another.
            ParticleEffect = effect.Clone();
            Parameters = ParticleEffect.Parameters;
            // Look up shortcuts for parameters that change every frame.
            EffectViewParameter = Parameters["View"];
            EffectProjectionParameter = Parameters["Projection"];
            EffectViewportScaleParameter = Parameters["ViewportScale"];
            EffectTimeParameter = Parameters["CurrentTime"];
            // Set the values of parameters that do not change.
            Parameters["NumberOfImages"].SetValue(Settings.NumberOfImages);
            Parameters["RotateTowardCamera"].SetValue(Settings.RotateTowardCamera ? 1f : 0);
            // Load the particle texture, and set it onto the effect.
            Texture2D sprBackground = Content.Load<Texture2D>(Settings.TextureName);

            this.ParticleSize = new Vector2((sprBackground.Width / Settings.NumberOfImages) * 0.5f, sprBackground.Height * 0.5f);
            Parameters["Size"].SetValue(ParticleSize);
            Parameters["t0"].SetValue(sprBackground);

            // Create a dynamic vertex buffer.
            VertexBuffer = new DynamicVertexBuffer(GraphicsDevice, ParticleVertex.VertexDeclaration,
                                                   Settings.MaxParticles * 4, BufferUsage.WriteOnly);
            // Create and populate the index buffer.
            ushort[] ArrayIndex = new ushort[Settings.MaxParticles * 6];
            for (int i = 0; i < Settings.MaxParticles; i++)
            {
                ArrayIndex[i * 6 + 0] = (ushort)(i * 4 + 0);
                ArrayIndex[i * 6 + 1] = (ushort)(i * 4 + 1);
                ArrayIndex[i * 6 + 2] = (ushort)(i * 4 + 2);
                ArrayIndex[i * 6 + 3] = (ushort)(i * 4 + 0);
                ArrayIndex[i * 6 + 4] = (ushort)(i * 4 + 2);
                ArrayIndex[i * 6 + 5] = (ushort)(i * 4 + 3);
            }

            IndexBuffer = new IndexBuffer(GraphicsDevice, typeof(ushort), ArrayIndex.Length, BufferUsage.WriteOnly);
            IndexBuffer.SetData(ArrayIndex);
        }

        public void Update(GameTime gameTime)
        {
            if (gameTime == null)
                throw new ArgumentNullException("gameTime");
            CurrentTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            RetireActiveParticles();
            FreeRetiredParticles();
            // If we let our timer go on increasing for ever, it would eventually
            // run out of floating point precision, at which point the particles
            // would render incorrectly. An easy way to prevent this is to notice
            // that the time value doesn't matter when no particles are being drawn,
            // so we can reset it back to zero any time the active queue is empty.
            if (FirstActiveParticle == FirstFreeParticle)
                CurrentTime = 0;
            if (FirstRetiredParticle == FirstActiveParticle)
                DrawCounter = 0;
        }

        public void SetTexture(Texture2D ActiveTexture)
        {
            Parameters["t0"].SetValue(ActiveTexture);
        }

        public void SetViewProjection(Matrix View, Matrix Projection)
        {
            EffectViewParameter.SetValue(View);
            EffectProjectionParameter.SetValue(Projection);
        }

        /// <summary>
        /// Helper for checking when active particles have reached the end of
        /// their life. It moves old particles from the active area of the queue
        /// to the retired section.
        /// </summary>
        void RetireActiveParticles()
        {
            while (FirstActiveParticle != FirstNewParticle)
            {
                // Is this particle old enough to retire?
                // We multiply the active particle index by four, because each
                // particle consists of a quad that is made up of four vertices.
                float particleAge = CurrentTime - ArrayParticles[FirstActiveParticle * 4].CreationTime;
                float particleDuration = ArrayParticles[FirstActiveParticle * 4].EndTime;
                if (particleAge < particleDuration)
                    break;
                // Remember the time at which we retired this particle.
                ArrayParticles[FirstActiveParticle * 4].CreationTime = DrawCounter;
                // Move the particle from the active to the retired queue.
                FirstActiveParticle++;
                if (FirstActiveParticle >= Settings.MaxParticles)
                    FirstActiveParticle = 0;
            }
        }

        /// <summary>
        /// Helper for checking when retired particles have been kept around long
        /// enough that we can be sure the GPU is no longer using them. It moves
        /// old particles from the retired area of the queue to the free section.
        /// </summary>
        void FreeRetiredParticles()
        {
            while (FirstRetiredParticle != FirstActiveParticle)
            {
                // Has this particle been unused long enough that
                // the GPU is sure to be finished with it?
                // We multiply the retired particle index by four, because each
                // particle consists of a quad that is made up of four vertices.
                int age = DrawCounter - (int)ArrayParticles[FirstRetiredParticle * 4].CreationTime;
                // The GPU is never supposed to get more than 2 frames behind the CPU.
                // We add 1 to that, just to be safe in case of buggy drivers that
                // might bend the rules and let the GPU get further behind.
                if (age < 3)
                    break;

                // Move the particle from the retired to the free queue.
                FirstRetiredParticle++;
                if (FirstRetiredParticle >= Settings.MaxParticles)
                    FirstRetiredParticle = 0;
            }
        }

        public void Draw(GraphicsDevice GraphicsDevice)
        {
            GraphicsDevice device = GraphicsDevice;
            // Restore the vertex buffer contents if the graphics device was lost.
            if (VertexBuffer.IsContentLost)
            {
                VertexBuffer.SetData(ArrayParticles);
            }
            // If there are any particles waiting in the newly added queue,
            // we'd better upload them to the GPU ready for drawing.
            if (FirstNewParticle != FirstFreeParticle)
            {
                AddNewParticlesToVertexBuffer();
            }
            // If there are any active particles, draw them now!
            if (FirstActiveParticle != FirstFreeParticle)
            {
                device.RasterizerState = RasterizerState.CullNone;
                device.BlendState = Settings.BlendState;
                if (UseAlphaBlend)
                    device.DepthStencilState = DepthStencilState.DepthRead;
                else
                    device.DepthStencilState = DepthStencilState.Default;
                // Set an effect parameter describing the viewport size. This is
                // needed to convert particle sizes into screen space point sizes.
                EffectViewportScaleParameter.SetValue(new Vector2(0.5f / device.Viewport.AspectRatio, -0.5f));
                // Set an effect parameter describing the current time. All the vertex
                // shader particle animation is keyed off this value.
                EffectTimeParameter.SetValue(CurrentTime);
                // Set the particle vertex and index buffer.
                device.SetVertexBuffer(VertexBuffer);
                device.Indices = IndexBuffer;
                // Activate the particle effect.
                foreach (EffectPass pass in ParticleEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    if (FirstActiveParticle < FirstFreeParticle)
                    {
                        // If the active particles are all in one consecutive range,
                        // we can draw them all in a single call.
                        device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0,
                                                     FirstActiveParticle * 4, (FirstFreeParticle - FirstActiveParticle) * 4,
                                                     FirstActiveParticle * 6, (FirstFreeParticle - FirstActiveParticle) * 2);
                    }
                    else
                    {
                        // If the active particle range wraps past the end of the queue
                        // back to the start, we must split them over two draw calls.
                        device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0,
                                                     FirstActiveParticle * 4, (Settings.MaxParticles - FirstActiveParticle) * 4,
                                                     FirstActiveParticle * 6, (Settings.MaxParticles - FirstActiveParticle) * 2);
                        if (FirstFreeParticle > 0)
                        {
                            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0,
                                                         0, FirstFreeParticle * 4,
                                                         0, FirstFreeParticle * 2);
                        }
                    }
                }
                // Reset some of the renderstates that we changed,
                // so as not to mess up any other subsequent drawing.
                device.DepthStencilState = DepthStencilState.Default;
            }
            DrawCounter++;
        }

        /// <summary>
        /// Helper for uploading new particles from our managed
        /// array to the GPU vertex buffer.
        /// </summary>
        void AddNewParticlesToVertexBuffer()
        {
            int stride = ParticleVertex.SizeInBytes;
            if (FirstNewParticle < FirstFreeParticle)
            {
                // If the new particles are all in one consecutive range,
                // we can upload them all in a single call.
                VertexBuffer.SetData(FirstNewParticle * stride * 4, ArrayParticles,
                                     FirstNewParticle * 4,
                                     (FirstFreeParticle - FirstNewParticle) * 4,
                                     stride, SetDataOptions.NoOverwrite);
            }
            else
            {
                // If the new particle range wraps past the end of the queue
                // back to the start, we must split them over two upload calls.
                VertexBuffer.SetData(FirstNewParticle * stride * 4, ArrayParticles,
                                     FirstNewParticle * 4,
                                     (Settings.MaxParticles - FirstNewParticle) * 4,
                                     stride, SetDataOptions.NoOverwrite);
                if (FirstFreeParticle > 0)
                {
                    VertexBuffer.SetData(0, ArrayParticles,
                                         0, FirstFreeParticle * 4,
                                         stride, SetDataOptions.NoOverwrite);
                }
            }
            // Move the particles we just uploaded from the new to the active queue.
            FirstNewParticle = FirstFreeParticle;
        }

        public void AddParticle(Vector3 Position)
        {
            Position += Settings.ComputeOffset();

            // Figure out where in the circular queue to allocate the new particle.
            int nextFreeParticle = FirstFreeParticle + 1;
            if (nextFreeParticle >= Settings.MaxParticles)
                nextFreeParticle = 0;
            // If there are no free particles, we just have to give up.
            if (nextFreeParticle == FirstRetiredParticle)
                return;

            float EndTime = CurrentTime + Settings.ComputeDuration();
            Vector2 Scale = Settings.ComputeScale();
            Vector3 Rotation = Settings.ComputeRotation();
            Vector3 Speed = Settings.ComputeSpeed();

            // Fill in the particle vertex structure.
            for (int i = 0; i < 4; i++)
            {
                ArrayParticles[FirstFreeParticle * 4 + i].CreationTime = CurrentTime;
                ArrayParticles[FirstFreeParticle * 4 + i].EndTime = EndTime;
                ArrayParticles[FirstFreeParticle * 4 + i].Scale = Scale;
                ArrayParticles[FirstFreeParticle * 4 + i].Rotation = Rotation;
                ArrayParticles[FirstFreeParticle * 4 + i].Speed = Speed;

                ArrayParticles[FirstFreeParticle * 4 + i].Position = Position;
            }

            FirstFreeParticle = nextFreeParticle;
        }

        public void ClearParticles()
        {
            FirstActiveParticle = 0;
            FirstNewParticle = 0;
            FirstFreeParticle = 0;
            FirstRetiredParticle = 0;
            CurrentTime = 0;
            DrawCounter = 0;
        }
    }
}
