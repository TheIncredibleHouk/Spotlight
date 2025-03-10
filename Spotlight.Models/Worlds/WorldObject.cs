﻿using Newtonsoft.Json;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;

namespace Spotlight.Models
{
    public class WorldObject
    {
        public int GameObjectId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        [JsonIgnore]
        public GameObject GameObject { get; set; }

        public Rectangle CalcBoundBox()
        {
            int minX = 1000, minY = 1000;
            int maxX = -1000, maxY = -1000;

            List<Sprite> visibleSprites = GameObject.Sprites.ToList();

            if (visibleSprites.Count == 0)
            {
                minY = minX = 0;
                maxX = maxY = 16;
            }
            else
            {
                foreach (var sprite in visibleSprites)
                {
                    minX = sprite.X < minX ? sprite.X : minX;
                    maxX = sprite.X + 8 > maxX ? sprite.X + 8 : maxX;
                    minY = sprite.Y < minY ? sprite.Y : minY;
                    maxY = sprite.Y + 16 > maxY ? sprite.Y + 16 : maxY;
                }
            }

            BoundRectangle = new Rectangle(X * 16 + minX, Y * 16 + minY, maxX - minX, maxY - minY);
            return BoundRectangle;
        }

        public Rectangle CalcVisualBox(bool withOverlays)
        {
            int minX = 1000, minY = 1000;
            int maxX = -1000, maxY = -1000;

            List<Sprite> visibleSprites = GameObject.Sprites;

            if (visibleSprites.Count == 0)
            {
                minY = minX = 0;
                maxX = maxY = 16;
            }
            else
            {
                foreach (var sprite in visibleSprites)
                {
                    minX = sprite.X < minX ? sprite.X : minX;
                    maxX = sprite.X + 8 > maxX ? sprite.X + 8 : maxX;
                    minY = sprite.Y < minY ? sprite.Y : minY;
                    maxY = sprite.Y + 16 > maxY ? sprite.Y + 16 : maxY;
                }
            }

            if (minX % 16 != 0)
            {
                if (minX < 0)
                {
                    minX = ((minX / 16) - 1) * 16;
                }
                else
                {
                    minX = (minX / 16) * 16;
                }
            }

            if (minY % 16 != 0)
            {
                if (minY < 0)
                {
                    minY = ((minY / 16) - 1) * 16;
                }
                else
                {
                    minY = (minY / 16) * 16;
                }
            }

            if (maxX % 16 != 0)
            {
                maxX = ((maxX / 16) + 1) * 16;
            }

            if (maxY % 16 != 0)
            {
                maxY = ((maxY / 16) + 1) * 16;
            }

            VisualRectangle = new Rectangle(X * 16 + minX, Y * 16 + minY, maxX - minX, maxY - minY);
            return VisualRectangle;
        }

        [JsonIgnore]
        public Rectangle BoundRectangle { get; private set; }

        [JsonIgnore]
        public Rectangle VisualRectangle { get; private set; }
    }
}