﻿using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DSharpDXRastertek.Tut13.Graphics.TextFont
{
    public class DTextClass                 // 276 lines
    {
        // Structs
        [StructLayout(LayoutKind.Sequential)]
        public struct DSentence
        {
            public SharpDX.Direct3D11.Buffer VertexBuffer;
            public SharpDX.Direct3D11.Buffer IndexBuffer;
            public int VertexCount;
            public int IndexCount;
            public int MaxLength;
            public float red;
            public float green;
            public float blue;
            public string sentenceText;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct DVertex
        {
            public Vector3 position;
            public Vector2 texture;
        }

        // Properties
        public DFont Font;
        public DFontShader FontShader;
        public int ScreenWidth;
        public int ScreenHeight;
        public Matrix BaseViewMatrix;
        public DSentence[] sentences = new DSentence[3];

        // Methods
        public bool Initialize(SharpDX.Direct3D11.Device device, DeviceContext deviceContext, IntPtr windowHandle, int screanWidth, int screenHeight, Matrix baseViewMatrix)
        {
            // Store the screen width and height.
            ScreenWidth = screanWidth;
            ScreenHeight = screenHeight;

            // Store the base view matrix.
            BaseViewMatrix = baseViewMatrix;

            // Create the font object.
            Font = new DFont();

            // Initialize the font object.
            if (!Font.Initialize(device, "fontdata.txt", "font.bmp"))
                return false;

            // Create the font shader object.
            FontShader = new DFontShader();

            // Initialize the font shader object.
            if (!FontShader.Initialize(device, windowHandle))
                return false;

            // Initialize the first sentence.
            if (!InitializeSentence(out sentences[0], 16, device))
                return false;

            // Now update the sentence vertex buffer with the new string information.
            if (!UpdateSentece(ref sentences[0], "Hello", 100, 100, 1, 1, 1, deviceContext))
                return false;

            // Initialize the second sentence.
            if (!InitializeSentence(out sentences[1], 16, device))
                return false;

            // Now update the sentence vertex buffer with the new string information.
            if (!UpdateSentece(ref sentences[1], "Goodbye", 100, 200, 1, 1, 0, deviceContext))
                return false;

             // Initialize the Third sentence.
             if (!InitializeSentence(out sentences[2], 32, device))
                return false;

             // Now update the sentence vertex buffer with the new string information.
             if (!UpdateSentece(ref sentences[2], "User Enterntered: ", 20, 80, 0, 1, 1, deviceContext))
                 return false;

            return true;
        }
        public void Shutdown()
        {
            // Release all sentances however many there may be.
            foreach (DSentence sentance in sentences)
                ReleaseSentences(sentance);

            // Release the font shader object.
            FontShader?.Shuddown();
            FontShader = null;
            // Release the font object.
            Font?.Shutdown();
            Font = null;
        }
        public bool Render(DeviceContext deviceContext, Matrix worldMatrix, Matrix orthoMatrix)
        {
            // Render all Sentances however many there mat be.
            foreach (DSentence sentance in sentences)
            {
                if (!RenderSentence(deviceContext, sentance, worldMatrix, orthoMatrix))
                    return false;
            }

            return true;
        }
        public bool SetMousePosition(int mouseX, int mouseY, DeviceContext deviceContext)
        {
            string mouseString;

            // Setup the mouseX string.
            mouseString = "Mouse X: " + mouseX.ToString();
            
            // Update the sentence vertex buffer with the new string information.
            if (!UpdateSentece(ref sentences[0], mouseString, 20, 20, 1, 1, 1, deviceContext))
                return false;

            // Setup the mouseY string.
            mouseString = "Mouse Y: " + mouseY.ToString();

            // Update the sentence vertex buffer with the new string information.
            if (!UpdateSentece(ref sentences[1], mouseString, 20, 40, 1, 1, 1, deviceContext))
                return false;

            return true;
        }
        public bool SetNewSenctenceData(string passedInText, DeviceContext deviceContext)
        {
            int newTextLength = passedInText.Length;
            int sentenceTextLength = sentences[2].sentenceText.Length;
            int resultLength = sentenceTextLength - newTextLength;
            string test = sentences[2].sentenceText.Substring(resultLength);

            // Dont repeat the same character thats already added.
            if (passedInText.Length == 0 || test.Equals(passedInText)) 
                return false;

            string mouseString;
            
            // Setup the mouseX string.
            mouseString = sentences[2].sentenceText + passedInText;
            // Update the sentence vertex buffer with the new string information.
            if (!UpdateSentece(ref sentences[2], mouseString, 20, 80, 0, 1, 1, deviceContext))
                return false;

            return true;
        }
        private bool InitializeSentence(out DSentence sentence, int maxLength, SharpDX.Direct3D11.Device device)
        {
            // Create a new sentence object.
            sentence = new DSentence();

            // Initialize the sentence buffers to null;
            sentence.VertexBuffer = null;
            sentence.IndexBuffer = null;

            // Set the maximum length of the sentence.
            sentence.MaxLength = maxLength;

            // Set the number of vertices in vertex array.
            sentence.VertexCount = 6 * maxLength;
            // Set the number of vertices in the vertex array.
            sentence.IndexCount = sentence.VertexCount;

            // Create the vertex array.
            var vertices = new DTextClass.DVertex[sentence.VertexCount];
            // Create the index array.
            var indices = new int[sentence.IndexCount];

            // Initialize the index array.
            for (var i = 0; i < sentence.IndexCount; i++)
                indices[i] = i;

            // Set up the description of the dynamic vertex buffer.
            var vertexBufferDesc = new BufferDescription()
            {
                Usage = ResourceUsage.Dynamic,
                SizeInBytes = Utilities.SizeOf<DTextClass.DVertex>() * sentence.VertexCount,
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.Write,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0
            };

            // Create the vertex buffer.
            sentence.VertexBuffer = SharpDX.Direct3D11.Buffer.Create(device, vertices, vertexBufferDesc);

            // Create the index buffer.
            sentence.IndexBuffer = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.IndexBuffer, indices);

            return true;
        }
        private bool UpdateSentece(ref DSentence sentence, string text, int positionX, int positionY, float red, float green, float blue, DeviceContext deviceContext)
        {
            // Store the Text to update the given sentence.
            sentence.sentenceText = text;

            // Store the color of the sentence.
            sentence.red = red;
            sentence.green = green;
            sentence.blue = blue;

            // Get the number of the letter in the sentence.
            var numLetters = text.Length;

            // Check for possible buffer overflow.
            if (numLetters > sentence.MaxLength)
                return false;

            // Calculate the X and Y pixel position on screen to start drawing to.
            var drawX = -(ScreenWidth >> 1) + positionX;
            var drawY = (ScreenHeight >> 1) - positionY;

            // Use the font class to build the vertex array from the sentence text and sentence draw location.
            List<DTextClass.DVertex> vertices;
            Font.BuildVertexArray(out vertices, text, drawX, drawY);

            DataStream mappedResource;

            #region Vertex Buffer 
            // Lock the vertex buffer so it can be written to.
            deviceContext.MapSubresource(sentence.VertexBuffer, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out mappedResource);

            // Copy the data into the vertex buffer.
            mappedResource.WriteRange<DTextClass.DVertex>(vertices.ToArray());

            // Unlock the vertex buffer.
            deviceContext.UnmapSubresource(sentence.VertexBuffer, 0);
            #endregion

            return true;
        }
        private void ReleaseSentences(DSentence sentence)
        {
            // Release the sentence vertex buffer.
            sentence.VertexBuffer?.Dispose();
            sentence.VertexBuffer = null;
            // Release the sentence index buffer.
            sentence.IndexBuffer?.Dispose();
            sentence.IndexBuffer = null;
        }
        private bool RenderSentence(DeviceContext deviceContext, DSentence sentence, Matrix worldMatrix, Matrix orthoMatrix)
        {
            // Set vertex buffer stride and offset.
            var stride = Utilities.SizeOf<DTextClass.DVertex>();
            var offset = 0;

            // Set the vertex buffer to active in the input assembler so it can be rendered.
            deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(sentence.VertexBuffer, stride, offset));
            
            // Set the index buffer to active in the input assembler so it can be rendered.
            deviceContext.InputAssembler.SetIndexBuffer(sentence.IndexBuffer, Format.R32_UInt, 0);
            
            // Set the type of the primitive that should be rendered from this vertex buffer, in this case triangles.
            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            
            // Create a pixel color vector with the input sentence color.
            var pixelColor = new Vector4(sentence.red, sentence.green, sentence.blue, 1);
            
            // Render the text using the font shader.
            if (!FontShader.Render(deviceContext, sentence.IndexCount, worldMatrix, BaseViewMatrix, orthoMatrix, Font.Texture.TextureResource, pixelColor))
                return false;

            return true;
        }
    }
}