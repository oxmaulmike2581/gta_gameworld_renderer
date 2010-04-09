﻿using System;
using System.Collections.Generic;
using GTAWorldRenderer.Logging;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GTAWorldRenderer.Scenes.Loaders
{

   class SceneLoader
   {
      private static Log Logger = Log.Instance;


      public Scene LoadScene()
      {
         using (Logger.EnterTimingStage("Loading scene"))
         {
            var sceneObjectsLoader = new SceneObjectsLoader();
            return sceneObjectsLoader.LoadScene();
         }
      }


      void PrintMemoryUsed(Scene scene)
      {
         int totalIndexBufferBytes = 0;
         int totalVertexBufferBytes = 0;

         foreach (var obj in scene.SceneObjects)
         {
            int curVertSize, curIndSize;
            obj.Model.GetMemoryUsed(out curVertSize, out curIndSize);
            totalIndexBufferBytes += curIndSize;
            totalVertexBufferBytes += curVertSize;
         }

         Action<string, int> PrintInfo = delegate(string msg, int bytes)
         {
            double mb = bytes / (1024.0 * 11024.0);
            Logger.Print(String.Format("... {0}: {1} bytes ({2:f2} MegaBytes)", msg, bytes, mb));
         };

         Logger.Print("Memory used:");
         PrintInfo("vertex buffers", totalVertexBufferBytes);
         PrintInfo("index buffers", totalIndexBufferBytes);
         PrintInfo("textures", TexturesStorage.Instance.GetMemoryUsed());
      }



   }

}
