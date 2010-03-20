﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTAWorldRenderer.Logging;
using GTAWorldRenderer.Scenes.ArchivesCommon;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace GTAWorldRenderer.Scenes
{
   partial class SceneLoader
   {
      /// <summary>
      /// Различные тесты.
      /// Нужны только временно, на этапе отладки.
      /// Потом либо будут приведены в порядок, либо нафиг удалены
      /// </summary>
      class LoadersTests
      {

         /// <summary>
         /// Распаковывает TXD архив.
         /// В случае ошибки НЕ кидает дальше исключение, выводит ErrorMessage в лог.
         /// </summary>
         /// <param name="txdPath">TXD архив, который нужно распаковать</param>
         /// <param name="outputPathPrefix">Папка, в которую проихводится распаковка. Должна существовать!</param>
         public static void UnpackTxd(string txdPath, string outputPathPrefix)
         {
            try
            {
               if (!outputPathPrefix.EndsWith(Path.DirectorySeparatorChar.ToString()))
                  outputPathPrefix += Path.DirectorySeparatorChar;

               IEnumerable<ArchiveEntry> entries; ;
               TXDArchive archive = new TXDArchive(txdPath);
               entries = archive.Load();

               using (BinaryReader reader = new BinaryReader(new FileStream(txdPath, FileMode.Open)))
               {
                  foreach (var entry in entries)
                  {
                     reader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);
                     byte[] data = reader.ReadBytes(entry.Size);

                     if (entry.Name.Contains('/')) // имя текстуры в TXD может иметь вид <имя TXD-файла>/<имя текстуры>.gtatexture
                     {
                        string dir = entry.Name.Substring(0, entry.Name.LastIndexOf('/'));
                        if (!Directory.Exists(outputPathPrefix + dir))
                           Directory.CreateDirectory(outputPathPrefix + dir);
                     }
                     string path = outputPathPrefix + entry.Name;
                     while (File.Exists(path))
                     {
                        int sep = path.LastIndexOf('.');
                        path = path.Substring(0, sep) + "_" + path.Substring(sep);
                     }

                     using (FileStream fout = new FileStream(path, FileMode.CreateNew))
                        fout.Write(data, 0, data.Length);
                  }
               }
            } catch (Exception er)
            {
               Log.Instance.Print("Failed to unpack TXD. Exception occured: " + er.ToString());
            }
         }

         /// <summary>
         /// Распаковывает IMG архив.
         /// В случае ошибки НЕ кидает дальше исключение, выводит ErrorMessage в лог.
         /// 
         /// Все распакованные TXD файлы сначала копируются в outputPathPrefix/___txds/,
         /// потом распаковываются в outputPathPrefix
         /// </summary>
         /// <param name="imgPath">Путь к IMG архиву</param>
         /// <param name="outputPathPrefix">Папка, в которую проихводится распаковка. Должна существовать!</param>
         public static void UnpackImg(string imgPath, string outputPathPrefix)
         {
            try
            {
            if (!outputPathPrefix.EndsWith(Path.DirectorySeparatorChar.ToString()))
               outputPathPrefix += Path.DirectorySeparatorChar;

            if (!Directory.Exists(outputPathPrefix + @"\___txds\\"))
               Directory.CreateDirectory(outputPathPrefix + @"\___txds\\");

            using (Log.Instance.EnterStage("Unpacking IMG: " + imgPath))
            {
               IMGArchive archive = new IMGArchive(imgPath, GtaVersion.III);
               IEnumerable<ArchiveEntry> entries = archive.Load();

               using (BinaryReader reader = new BinaryReader(new FileStream(imgPath, FileMode.Open)))
               {
                  foreach (var entry in entries)
                  {
                     reader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);
                     byte[] data = reader.ReadBytes(entry.Size);
                     if (entry.Name.EndsWith(".txd"))
                     {
                        string path = outputPathPrefix + @"\___txds\\" + entry.Name;
                        using (FileStream fout = new FileStream(path, FileMode.Create))
                           fout.Write(data, 0, data.Length);
                        UnpackTxd(path, outputPathPrefix);
                     }
                     else
                     {
                        string path = outputPathPrefix + entry.Name;
                        using (FileStream fout = new FileStream(path, FileMode.Create))
                           fout.Write(data, 0, data.Length);
                     }
                  }
               }
            }
            }
            catch (Exception er)
            {
               Log.Instance.Print("Failed to unpack IMG. Exception occured: " + er.ToString());
            }
         }


         /// <summary>
         /// Распаковывает все найденные TXD и IMG архивы, найденные в directoryPath (поиск рекурсивный).
         /// </summary>
         /// <param name="directoryPath">Директория с исходными архивами (к примеру, папка с игрой GTA)</param>
         /// <param name="outptuPathPrefix">Папка, в которую проихводится распаковка. Должна существовать!</param>
         public static void UnpackAllArchivesInDirectory(string directoryPath, string outptuPathPrefix)
         {
            foreach (var path in Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories))
            {
               if (path.EndsWith(".img"))
                  UnpackImg(path, outptuPathPrefix);
               else if (path.EndsWith(".txd"))
                  UnpackTxd(path, outptuPathPrefix);
            }
         }


         /// <summary>
         /// Распаковывает все текстуры (*.gtatexture) в directoryPath (и подкаталогах).
         /// Распакованные изображения сохраняются рядом в файл с таким же именем и одним из графических расширений.
         /// </summary>
         public static void UnpackAllTextures(string directoryPath)
         {
            int success = 0, fail = 0;
            using (Log.Instance.EnterStage("Unpacking all textures in directory " + directoryPath))
            {
               foreach (var file in Directory.GetFiles(directoryPath, "*.gtatexture", SearchOption.AllDirectories))
               {
                  try
                  {
                     using (Log.Instance.EnterStage("Unpacking texture: " + file.Substring(directoryPath.Length)))
                     {
                        GTATextureLoader textureLoader = new GTATextureLoader(new BinaryReader(new FileStream(file, FileMode.Open)));
                        Texture2D texture = textureLoader.Load();
                        texture.Save(file.Substring(0, file.LastIndexOf('.')) + ".png", ImageFileFormat.Png);

                        ++success;
                        Log.Instance.Print("success!");
                     }
                  } 
                  catch (Exception er)
                  {
                     Log.Instance.Print("Failed to load texture. Exception: " + er.Message, MessageType.Error);
                     ++fail;
                  }
               }
            }
            Log.Instance.Print(String.Format("Finished textures processing. Successes: {0}, failes: {1}", success, fail));
         }

      }

   }
}