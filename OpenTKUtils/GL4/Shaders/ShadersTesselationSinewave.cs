﻿/*
 * Copyright © 2015 - 2018 EDDiscovery development team
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software distributed under
 * the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF
 * ANY KIND, either express or implied. See the License for the specific language
 * governing permissions and limitations under the License.
 * 
 * EDDiscovery is not affiliated with Frontier Developments plc.
 */

using System;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKUtils.GL4
{
    // Shader, with tesselation, and Y change in amp using sin

    public class GLTesselationShaderSinewave : GLShaderProgramBase
    {
        string vert =
        @"
#version 450 core
layout (location = 0) in vec4 position;

void main(void)
{
	gl_Position =position;       
}
";

        string TCS(float tesselation)
        {
            return
        @"
#version 450 core

layout (vertices = 4) out;

void main(void)
{
    float tess = " + tesselation.ToString() + @";

    if ( gl_InvocationID == 0 )
    {
        gl_TessLevelInner[0] =  tess;
        gl_TessLevelInner[1] =  tess;
        gl_TessLevelOuter[0] =  tess;
        gl_TessLevelOuter[1] =  tess;
        gl_TessLevelOuter[2] =  tess;
        gl_TessLevelOuter[3] =  tess;
    }

    gl_out[gl_InvocationID].gl_Position = gl_in[gl_InvocationID].gl_Position;
}
";
        }

        string TES(float amplitude)
        {
            return

@"
#version 450 core

layout (quads) in; 

layout (location = 20) uniform  mat4 projectionmodel;
layout (location = 22) uniform  mat4 transform;
layout (location = 26) uniform  float phase;

out vec2 vs_textureCoordinate;

void main(void)
{
    float amp = " + amplitude + @";
    vs_textureCoordinate = vec2(gl_TessCoord.x,1.0-gl_TessCoord.y);

    vec4 p1 = mix(gl_in[0].gl_Position, gl_in[1].gl_Position, gl_TessCoord.x);  
    vec4 p2 = mix(gl_in[2].gl_Position, gl_in[3].gl_Position, gl_TessCoord.x); 
    vec4 pos = mix(p1, p2, gl_TessCoord.y);                                     

    pos.y += amp*sin((phase+gl_TessCoord.x)*3.142*2);

    gl_Position = projectionmodel * transform * pos;

}
";
        }

        string frag =

@"
#version 450 core
in vec2 vs_textureCoordinate;
out vec4 color;
layout (binding=1) uniform sampler2D textureObject;

void main(void)
{
    color = texture(textureObject, vs_textureCoordinate);       // vs_texture coords normalised 0 to 1.0f
}
";
        private bool nocull = false;

        public float Phase { get; set; } = 0;                   // set to animate.

        public GLTesselationShaderSinewave(float tesselation,float amplitude, bool nocullface)
        {
            Compile(vertex: vert, tcs: TCS(tesselation), tes: TES(amplitude), frag: frag);
            nocull = nocullface;
        }

        public override void Start(Common.MatrixCalc c)
        {
            GL.UseProgram(Id);           // use this
            Matrix4 projmodel = c.ProjectionModelMatrix;
            GL.ProgramUniformMatrix4(Id, 20, false, ref projmodel);
            GL.ProgramUniform1(Id, 26, Phase);
            GLStatics.PatchSize(4);
            GL4Statics.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            if ( nocull)
                GL.Disable(EnableCap.CullFace);
        }

        public override void Finish()
        {
            base.Finish();
            if ( nocull )
                GL.Enable(EnableCap.CullFace);
        }
    }
}
