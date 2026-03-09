using System.Runtime.InteropServices;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Shaders.Types;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Rulesets.Space.Objects.Drawables
{
    public partial class NoteGlowPiece : Drawable
    {
        private float innerPortion = 0.7f;

        public float InnerPortion
        {
            get => innerPortion;
            set
            {
                if (innerPortion == value) return;
                innerPortion = value;
                Invalidate(Invalidation.DrawNode);
            }
        }

        private float glowCornerRadius = 0.15f;

        public float GlowCornerRadius
        {
            get => glowCornerRadius;
            set
            {
                if (glowCornerRadius == value) return;
                glowCornerRadius = value;
                Invalidate(Invalidation.DrawNode);
            }
        }

        private IShader shader = null!;
        private Texture texture = null!;

        [BackgroundDependencyLoader]
        private void load(IRenderer renderer, ShaderManager shaders)
        {
            shader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, "NoteGlow");
            texture = renderer.WhitePixel;
            Blending = BlendingParameters.Additive;
        }

        protected override DrawNode CreateDrawNode() => new NoteGlowDrawNode(this);

        private class NoteGlowDrawNode : DrawNode
        {
            protected new NoteGlowPiece Source => (NoteGlowPiece)base.Source;

            private IShader shader = null!;
            private Texture texture = null!;
            private float innerPortion;
            private float glowCornerRadius;
            private Quad screenSpaceQuad;
            private ColourInfo drawColour;

            public NoteGlowDrawNode(NoteGlowPiece source)
                : base(source)
            {
            }

            public override void ApplyState()
            {
                base.ApplyState();

                shader = Source.shader;
                texture = Source.texture;
                innerPortion = Source.innerPortion;
                glowCornerRadius = Source.glowCornerRadius;
                screenSpaceQuad = Source.ScreenSpaceDrawQuad;
                drawColour = DrawColourInfo.Colour;
            }

            private IUniformBuffer<NoteGlowParameters>? parametersBuffer;

            protected override void Draw(IRenderer renderer)
            {
                base.Draw(renderer);

                if (texture?.Available != true)
                    return;

                parametersBuffer ??= renderer.CreateUniformBuffer<NoteGlowParameters>();
                parametersBuffer.Data = new NoteGlowParameters
                {
                    InnerPortion = innerPortion,
                    CornerRadius = glowCornerRadius,
                };

                renderer.SetBlend(BlendingParameters.Additive);

                shader.Bind();
                shader.BindUniformBlock("m_NoteGlowParameters", parametersBuffer);

                renderer.DrawQuad(texture, screenSpaceQuad, drawColour);

                shader.Unbind();
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);
                parametersBuffer?.Dispose();
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private record struct NoteGlowParameters
        {
            public UniformFloat InnerPortion;
            public UniformFloat CornerRadius;
            private readonly UniformPadding8 pad;
        }
    }
}
