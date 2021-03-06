﻿using OpenTK.Graphics.OpenGL;
using System;

namespace JStudio.OpenGL
{
    public enum ShaderAttributeIds
    {
        Position = 0,
        Normal = 1,
        Color0 = 2,
        Color1 = 3,
        Tex0 = 4,
        Tex1 = 5,
        Tex2 = 6,
        Tex3 = 7,
        Tex4 = 8,
        Tex5 = 9,
        Tex6 = 10,
        Tex7 = 11,
        SkinIndices = 12,
        SkinWeights = 13
    }

    public enum ShaderUniformBlockIds
    {
        LightBlock = 0,
        PixelShaderBlock = 1,
    }

    public class Shader : IDisposable
    {
        public readonly string Name;

        public int UniformLightBlock { get; private set; }
        public int UniformPSBlock { get; private set; }
        public int[] UniformTextureSamplers { get; private set; }

        public int Program { get { return m_programAddress; } }
        public int PSBlockUBO { get { return m_psBlockAddress; } }

        private int m_vertexAddress = -1;
        private int m_fragmentAddress = -1;
        private int m_programAddress = -1;
        private int m_psBlockAddress;

        // To detect redundant calls
        private bool m_hasBeenDisposed = false; 

        public Shader(string name)
        {
            Name = name;
        }

        public void Bind()
        {
            GL.UseProgram(m_programAddress);
        }

        public bool CompileSource(string code, ShaderType type)
        {
            // Generate a new shader and clean up the old shader with a warning if they forgot to link before trying to compile again.
            int shaderAddress = -1;
            switch (type)
            {
                case ShaderType.FragmentShader:
                    if (m_fragmentAddress >= 0)
                    {
                        Console.WriteLine("Shader \"{0}\" called CompileSource for ShaderType: {1} twice before linking! Disposing old shader.", Name, type);
                        GL.DeleteShader(m_fragmentAddress);
                    }
                    m_fragmentAddress = GL.CreateShader(type);
                    shaderAddress = m_fragmentAddress;
                    break;
                case ShaderType.VertexShader:
                    if (m_vertexAddress >= 0)
                    {
                        Console.WriteLine("Shader \"{0}\" called CompileSource for ShaderType: {1} twice before linking! Disposing old shader.", Name, type);
                        GL.DeleteShader(m_fragmentAddress);
                    }
                    m_vertexAddress = GL.CreateShader(type);
                    shaderAddress = m_vertexAddress;
                    break;
            }

            GL.ShaderSource(shaderAddress, code);
            GL.CompileShader(shaderAddress);

            int compileStatus;
            GL.GetShader(shaderAddress, ShaderParameter.CompileStatus, out compileStatus);

            if (compileStatus != 1)
            {
                Console.WriteLine("Failed to compile {0} shader {1}. Log:\n{2}", type, Name, GL.GetShaderInfoLog(shaderAddress));
                return false;
            }

            return true;
        }

        public bool LinkShader()
        {
            // If the program was previously linked, we need to dispose of the old
            // program. We're going to create a new one.
            if (m_programAddress >= 0)
            {
                Console.WriteLine("Shader \"{0}\" called LinkShader for already linked shader! Disposing old program.", Name);
                GL.DeleteProgram(m_programAddress);
            }

            // Error out if a shader step is missing.
            if (m_fragmentAddress < 0 || m_vertexAddress < 0)
            {
                throw new Exception("Shader does not have both a Vertex and Fragment shader!");
            }

            m_programAddress = GL.CreateProgram();

            GL.AttachShader(m_programAddress, m_vertexAddress);
            GL.AttachShader(m_programAddress, m_fragmentAddress);

            GL.LinkProgram(m_programAddress);

            // Make sure the program linked correctly.
            GL.GetProgram(m_programAddress, GetProgramParameterName.LinkStatus, out int linkStatus);
            if (linkStatus != 1)
            {
                Console.WriteLine("Error linking shader. Result: {0}", GL.GetProgramInfoLog(m_programAddress));
                return false;
            }

            // Now that the program is linked, bind to our uniform locations.
            UniformLightBlock = GL.GetUniformBlockIndex(m_programAddress, "LightBlock");
            UniformPSBlock = GL.GetUniformBlockIndex(m_programAddress, "PSBlock");

            // Initialize the texture samplers.
            UniformTextureSamplers = new int[8];
            for (int i = 0; i < UniformTextureSamplers.Length; i++)
                UniformTextureSamplers[i] = GL.GetUniformLocation(m_programAddress, string.Format("Texture[{0}]", i));

            // Now that we've (presumably) set both a vertex and a fragment shader and linked them to the program,
            // we're going to clean up the reference to the shaders as the Program now keeps its own reference.
            GL.DeleteShader(m_vertexAddress);
            GL.DeleteShader(m_fragmentAddress);
            m_vertexAddress = -1;
            m_fragmentAddress = -1;

            m_psBlockAddress = GL.GenBuffer();
            return true;
        }

        public override string ToString()
        {
            return Name;
        }

        #region IDisposable Support
        ~Shader()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!m_hasBeenDisposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // Free unmanaged resources
                if (m_vertexAddress >= 0)
                    GL.DeleteShader(m_vertexAddress);
                if (m_fragmentAddress >= 0)
                    GL.DeleteShader(m_fragmentAddress);

                if (m_psBlockAddress >= 0)
                    GL.DeleteBuffer(m_psBlockAddress);

                if (m_programAddress >= -1)
                    GL.DeleteProgram(m_programAddress);
                
                m_hasBeenDisposed = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
