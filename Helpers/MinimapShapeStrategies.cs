using UnityEngine;
using Small_Corner_Map.Main; // Added to reference MinimapContent

namespace Small_Corner_Map.Helpers
{
    public interface IMinimapShapeStrategy
    {
        void CreateMinimap(GameObject parent, float size);
    }

    public class CircularMinimapStrategy : IMinimapShapeStrategy
    {
        public void CreateMinimap(GameObject parent, float size)
        {
            // Create the circular minimap mask
            var (maskObject, maskImage) = MinimapUIFactory.CreateMask(parent, size);

            // Create the border for the circular minimap
            MinimapUIFactory.CreateBorder(parent, size);
        }
    }

    public class SquareMinimapStrategy : IMinimapShapeStrategy
    {
        public void CreateMinimap(GameObject parent, float size)
        {
            // Create the square minimap frame
            var (frameObject, _) = MinimapUIFactory.CreateFrame(parent, size);
        }
    }

    public class MinimapShapeContext
    {
        private IMinimapShapeStrategy _strategy;

        public MinimapShapeContext(IMinimapShapeStrategy strategy)
        {
            _strategy = strategy;
        }

        public void SetStrategy(IMinimapShapeStrategy strategy)
        {
            _strategy = strategy;
        }

        public void CreateMinimap(GameObject parent, float size)
        {
            _strategy.CreateMinimap(parent, size);
        }
    }
}