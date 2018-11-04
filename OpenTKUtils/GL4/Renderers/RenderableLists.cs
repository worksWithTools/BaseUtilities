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

using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;

namespace OpenTKUtils.GL4
{
    public class GLRenderableItem : IDisposable            // all renderables inherit from this.
    {
        public int DrawCount { get; private set; }                                  // Draw count
        public int InstanceCount { get; private set; }                              // Instances
        public PrimitiveType PrimitiveType { get; private set; }   // Draw type
        public IGLVertexArray VertexArray { get; private set; }                // may be null - if so no vertex data
        public IGLInstanceData InstanceData { get; private set; }             // may be null - no instance data

        public GLRenderableItem(PrimitiveType pt, int drawcount, IGLVertexArray va = null, IGLInstanceData id = null, int ic = 1)
        {
            PrimitiveType = pt;
            DrawCount = drawcount;
            VertexArray = va;
            InstanceData = id;
            InstanceCount = ic;
        }

        public GLRenderableItem(PrimitiveType pt, IGLVertexArray va, IGLInstanceData id = null, int ic = 1)
        {
            PrimitiveType = pt;
            DrawCount = va.Count;
            VertexArray = va;
            InstanceData = id;
            InstanceCount = ic;
        }

        public void Bind(IGLProgramShader shader)
        {
            VertexArray?.Bind(shader);
            InstanceData?.Bind(shader);
        }

        public void Render()
        {
            GL.DrawArrays(PrimitiveType, 0, DrawCount);
        }

        public void Dispose()
        {
            VertexArray?.Dispose();
            InstanceData?.Dispose();
        }
    }

    // List to hold named renderables against programs, and a Render function to send the lot to GL - issued in Program ID order, then Add order

    public class GLRenderProgramSortedList : IDisposable
    {
        private BaseUtils.DictionaryOfDictionaries<IGLProgramShader, string, GLRenderableItem> renderables;
        private int unnamed = 0;

        public GLRenderProgramSortedList()
        {
            renderables = new BaseUtils.DictionaryOfDictionaries<IGLProgramShader, string, GLRenderableItem>();
        }

        public void Add(IGLProgramShader prog, string name, GLRenderableItem r)
        {
            renderables.Add(prog, name, r);
        }

        public void Add(IGLProgramShader prog, GLRenderableItem r)
        {
            Add(prog, "Unnamed_" + (unnamed++), r);
        }

        public GLRenderableItem this[string key] {  get { return renderables[key]; } }

        public bool Contains(string key) { return renderables.ContainsKey(key); }

        public void Render(Common.MatrixCalc c)
        {
            foreach (var d in renderables)
            {
                d.Key.Start(c);       // start the program
                d.Key.StartAction?.Invoke(d.Key);       // optional bind

                foreach (GLRenderableItem g in d.Value.Values)
                {
                    g.Bind(d.Key);
                    g.Render();
                }

                d.Key.Finish();
            }

            GL.UseProgram(0);           // final clean up
            GL.BindProgramPipeline(0);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var d in renderables)
                {
                    foreach (GLRenderableItem g in d.Value.Values)
                        g.Dispose();
                }

                renderables.Clear();
            }
        }
    }
}
