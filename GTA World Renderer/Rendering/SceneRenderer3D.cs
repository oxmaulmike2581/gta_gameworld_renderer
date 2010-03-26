﻿using GTAWorldRenderer.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace GTAWorldRenderer.Rendering
{
   class SceneRenderer3D : CompositeRenderer
   {
      private const float rotationSpeed = 0.3f;
      private const float slowMoveSpeed = 50.0f;
      private const float fastMoveSpeed = 500.0f;

      public Scene SceneContent { get; set; }

      GraphicsDevice device;
      Camera camera;
      InfoPanelFor3Dview textInfoPanel;
      Effect effect;
      Matrix projectionMatrix;

      KeyboardState oldKeyboardState = Keyboard.GetState();
      MouseState originalMouseState;
      bool usingMouse = true;


      /*
       * TODO :: 
       * в C# 4.0 появятся параметры по умолчанию.
       * Заменить набор конструкторов и метод Construct на единый конструктор с параметрами по умолчанию.
       */

      public SceneRenderer3D(ContentManager contentManager)
         : base(contentManager)
      {
         Initialize();
      }


      public SceneRenderer3D(ContentManager contentManager, Scene scene)
         : base(contentManager)
      {
         SceneContent = scene;
         Initialize();
      }


      public void Initialize()
      {
         device = GraphicsDeviceHolder.Device;
         camera = new Camera();

         textInfoPanel = new InfoPanelFor3Dview(Content);
         textInfoPanel.Camera = camera;
         AddSubRenderer(textInfoPanel);

         projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, device.Viewport.AspectRatio, 0.1f, 5000.0f);
         effect = Content.Load<Effect>("effect");

         Mouse.SetPosition(device.Viewport.Width / 2, device.Viewport.Height / 2);
         originalMouseState = Mouse.GetState();
      }


      public override void DoUpdate(GameTime gameTime)
      {
         float timeDifference = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;

         ProcessMouse(gameTime, timeDifference);
         ProcessKeyboard(gameTime, timeDifference);
      }


      private void ProcessMouse(GameTime gameTime, float amount)
      {
         if (!usingMouse)
            return;

         MouseState currentMouseState = Mouse.GetState();
         if (currentMouseState.LeftButton == ButtonState.Pressed && currentMouseState != originalMouseState)
         {
            float xDifference = -currentMouseState.X + originalMouseState.X;
            float yDifference = -currentMouseState.Y + originalMouseState.Y;
            float leftrightRot = rotationSpeed * xDifference * amount;
            float updownRot = rotationSpeed * yDifference * amount;
            camera.UpdateRotation(leftrightRot, updownRot);
         }
         Mouse.SetPosition(device.Viewport.Width / 2, device.Viewport.Height / 2);
      }


      private void ProcessKeyboard(GameTime gameTime, float amount)
      {
         Vector3 moveVector = new Vector3(0, 0, 0);
         KeyboardState keyState = Keyboard.GetState();

         Func<Keys, bool> KeyDown = key => keyState.IsKeyDown(key);
         Func<Keys, bool> KeyPressed = key => keyState.IsKeyDown(key) && !oldKeyboardState.IsKeyDown(key);

         // Перемещение камеры
         if (KeyDown(Keys.Up))
            moveVector += Vector3.Forward;
         if (KeyDown(Keys.Down))
            moveVector += Vector3.Backward;
         if (KeyDown(Keys.Right))
            moveVector += Vector3.Right;
         if (KeyDown(Keys.Left))
            moveVector += Vector3.Left;
         if (KeyDown(Keys.PageUp))
            moveVector += Vector3.Up;
         if (KeyDown(Keys.PageDown))
            moveVector += Vector3.Down;

         // Ускоренное перемещениеи камеры
         bool fast = keyState.IsKeyDown(Keys.LeftShift) || keyState.IsKeyDown(Keys.RightShift);

         if (KeyPressed(Keys.Escape))
         {
            GTAWorldRenderer.Logging.Log.Instance.Print("Escape pressed!");
            usingMouse = !usingMouse;
            if (usingMouse)
               Mouse.SetPosition(device.Viewport.Width / 2, device.Viewport.Height / 2);
         }

         oldKeyboardState = keyState;

         camera.UpdatePosition(moveVector * amount * (fast? fastMoveSpeed : slowMoveSpeed));
      }


      public override void DoDraw(GameTime gameTime)
      {
         device.Clear(Color.Black);

         if (SceneContent == null)
            return;

         effect.Parameters["xView"].SetValue(camera.ViewMatrix);
         effect.Parameters["xProjection"].SetValue(projectionMatrix);

         foreach (var obj in SceneContent.SceneObjects)
            obj.Model.Draw(effect, obj.WorldMatrix);
      }
   }
}
