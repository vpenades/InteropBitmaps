﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Microsoft.Xna.Framework.Graphics;

using XY = System.Numerics.Vector2;
using COLOR = System.Drawing.Color;

namespace InteropDrawing
{
    struct _Scene2D : IDrawable2D
    {
        public void DrawTo(IDrawing2D dc)
        {
            dc.DrawCircle((0, 0), 50, COLOR.Red);
            dc.DrawCircle((200, 200), 50, COLOR.White);
            dc.DrawRectangle((175, 175), (50, 50), (COLOR.Transparent, COLOR.Red, 4));

            dc.DrawRectangle((480, 200), (130, 130), (COLOR.Transparent, COLOR.Red, 4), 12);

            dc.DrawRectangle((10, 10), (200, 200), (COLOR.Yellow, 2));

            // DrawFlower(_Drawing2D, new XY(450, 450), 4);

            dc.DrawFont((100, 100), 0.75f, "Hello World!", (COLOR.White, 2));

            // var bee = _CreateBeeModel2D(COLOR.Yellow);
            // _Drawing2D.DrawAsset(System.Numerics.Matrix3x2.CreateRotation(t) * System.Numerics.Matrix3x2.CreateTranslation(600, 350), bee, Color.White);
        }
    }

    class _Sprites2D : IDrawable2D
    {
        private static readonly SpriteAsset[] _Punk = SpriteAsset.CreateGrid("Assets\\PunkRun.png", (256, 256), (128, 128), 8, 8).ToArray();
        private static readonly SpriteAsset[] _Tiles = SpriteAsset.CreateGrid("Assets\\Tiles.png", (16, 16), XY.Zero, 63, 9).ToArray();

        private static readonly BitmapGrid _Map1 = new BitmapGrid(4, 4, _Tiles);

        private static readonly SpriteAsset Beam1 = SpriteAsset.CreateFromBitmap("Assets\\beam1.png", (256, 32), (16, 16));

        private static readonly System.Diagnostics.Stopwatch _Timer = System.Diagnostics.Stopwatch.StartNew();

        private float _Time => (float)_Timer.Elapsed.TotalSeconds;

        public void DrawTo(IDrawing2D dc)
        {
            var x =
                System.Numerics.Matrix3x2.CreateScale(0.5f)
                * System.Numerics.Matrix3x2.CreateRotation(_Time)
                   * System.Numerics.Matrix3x2.CreateTranslation(400, 300);

            var image = SpriteAsset.CreateFromBitmap("Assets\\hieroglyph_sprites_by_asalga.png", (192, 192), (96, 96)).WithScale(3);

            dc.DrawSprite(x, image);



            // rect.DrawSprite(kk, image);

            var idx = (int)(_Time * 25);

            dc.DrawSprite(System.Numerics.Matrix3x2.CreateTranslation(400, 300), _Punk[idx % _Punk.Length]);
            dc.DrawSprite(System.Numerics.Matrix3x2.CreateTranslation(200, 300), (_Punk[idx % _Punk.Length], COLOR.Red.WithAlpha(128), true, false));

            dc.DrawSprite(System.Numerics.Matrix3x2.CreateTranslation(50, 300), _Punk[idx % _Punk.Length]);

            dc.DrawSprite(System.Numerics.Matrix3x2.CreateTranslation(10, 20), _Tiles[1]);
            dc.DrawSprite(System.Numerics.Matrix3x2.CreateTranslation(10 + 16, 20), _Tiles[2]);

            x =
                System.Numerics.Matrix3x2.CreateScale(1.4224f)
                * System.Numerics.Matrix3x2.CreateRotation(_Time)
                   * System.Numerics.Matrix3x2.CreateTranslation(400, 500);

            _Map1.DrawTo(dc, x);


            dc.DrawLine((20, 100), (300, 150), 30, Beam1);
            dc.DrawLine((20, 100), (25, 150), 30, Beam1);

            dc.DrawLine((20, 100), (300, 150), 1, COLOR.Black);
            dc.DrawLine((20, 100), (25, 150), 1, COLOR.Black);

            // rect.Bounds.DrawTo(_Drawing2D, (Color.Red, 1));
        }
    }

    class _DynamicTexture : IDrawable2D
    {
        public _DynamicTexture(GraphicsDevice device)
        {
            _Device = device;
        }

        private readonly GraphicsDevice _Device;

        private SpriteAsset _DynAsset;
        private Texture2D _DynTexture;
        private readonly Byte[] _DynData = new byte[64 * 64 * 4];

        private static readonly Random _Randomizer = new Random();

        public void DrawTo(IDrawing2D dc)
        {
            if (_DynTexture == null)
            {
                _DynTexture = new Texture2D(_Device, 64, 64);
                _DynAsset = new SpriteAsset(_DynTexture, (0, 0), (64, 64), (32, 32));
            }

            _Randomizer.NextBytes(_DynData);
            _DynTexture.SetData(_DynData);
            dc.DrawSprite(System.Numerics.Matrix3x2.CreateTranslation(400, 50), _DynAsset);
        }
    }

    class BitmapGrid
    {
        #region lifecycle

        public BitmapGrid(int width, int height, SpriteAsset[] templates)
        {
            _Sprites = templates;
            _Width = width;
            _Height = height;
            _Tiles = new int[_Width * _Height];

            _Tiles[2] = 5;
            _Tiles[5] = 7;
        }

        #endregion

        #region data

        private readonly SpriteAsset[] _Sprites;

        private readonly int _Width;
        private readonly int _Height;

        private readonly int[] _Tiles;

        #endregion

        #region API

        public void DrawTo(IDrawing2D target, System.Numerics.Matrix3x2 transform)
        {
            var tmp = new SpriteAsset();

            for (int y = 0; y < _Height; ++y)
            {
                for (int x = 0; x < _Width; ++x)
                {
                    var offset = new XY(x * 16, y * 16);

                    var idx = _Tiles[y * _Width + x];

                    _Sprites[idx].CopyTo(tmp, -offset);

                    target.DrawSprite(transform, tmp);
                }
            }
        }

        #endregion
    }
}
