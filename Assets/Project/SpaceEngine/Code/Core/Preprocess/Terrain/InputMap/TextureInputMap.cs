﻿using UnityEngine;

namespace SpaceEngine.Core.Preprocess.Terrain
{
    public class TextureInputMap : InputMap
    {
        public Texture2D Texture;

        [SerializeField]
        private bool FlipX = false;

        [SerializeField]
        private bool FlipY = false;

        public override int Width { get { return Texture.width; } }

        public override int Height { get { return Texture.height; } }

        public override int Channels { get { return 4; } }

        protected override void Start()
        {
            base.Start();
        }

        public override Vector4 GetValue(int x, int y)
        {
            if (FlipX) x = Width - x - 1;
            if (FlipY) y = Height - y - 1;

            return Texture.GetPixel(x, y);
        }
    }
}