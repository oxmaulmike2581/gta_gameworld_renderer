﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace GTAWorldRenderer.Rendering
{
   /// <summary>
   /// Реализация панели, на которой будет выводиться текстовая информация.
   /// Выводится FPS + пользовательские данные, которые задаются в виде словаря (свойство Data)
   /// </summary>
   class TextInfoPanel : IRenderer
   {
      private const int LINE_HEIGHT = 20;

      public Dictionary<string, object> Data { get; set; }

      private int fps = 0;
      private int frameCounter = 0;
      private TimeSpan elapsedTime = TimeSpan.Zero;
      private readonly TimeSpan oneSecond = TimeSpan.FromSeconds(1);

      private SpriteFont font;
      private SpriteBatch spriteBatch;

      ContentManager contentManager;

      public TextInfoPanel(ContentManager contentManager)
      {
         this.contentManager = contentManager;
         Data = new Dictionary<string, object>();
      }


      public void Initialize()
      {
         spriteBatch = new SpriteBatch(GraphicsDeviceHolder.Device);
         font = contentManager.Load<SpriteFont>("font");
      }


      /// <summary>
      /// Обновляет счётчик кадров.
      /// Этот метод должен вызываться из Draw (т.е. при отрисовке каждого кадра)
      /// </summary>
      /// <param name="gameTime">Игровое время</param>
      private void updateFPS(GameTime gameTime)
      {
         ++frameCounter;
         elapsedTime += gameTime.ElapsedGameTime;
         if (elapsedTime >= oneSecond)
         {
            elapsedTime -= oneSecond;
            fps = frameCounter;
            frameCounter = 0;
         }
      }


      public virtual void Update(GameTime gameTime)
      {
         // TODO :: плохо!!!
      }


      /// <summary>
      /// Отрисовывает панель на экране
      /// </summary>
      /// <param name="gameTime">Игровое время</param>
      public void Draw(GameTime gameTime)
      {
         updateFPS(gameTime);
         GraphicsDeviceHolder.Device.RenderState.DepthBufferEnable = false;
         spriteBatch.Begin();

         int y = 5;
         spriteBatch.DrawString(font, String.Format("FPS: {0}", fps), new Vector2(5, y), Color.Yellow);

         foreach(var item in Data)
         {
            y += LINE_HEIGHT;
            spriteBatch.DrawString(font, String.Format("{0}: {1}", item.Key, item.Value), new Vector2(5, y), Color.Yellow);
         }

         spriteBatch.End();
         GraphicsDeviceHolder.Device.RenderState.DepthBufferEnable = true;
      }
   }
}
