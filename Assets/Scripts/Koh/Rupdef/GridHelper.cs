using JetBrains.Annotations;
using UnityEngine;

namespace Koh.Rupdef
{
    public class GridHelper : MonoBehaviour
    {
        public static GridHelper Instance;

        public Transform BottomLeft;
        public Vector2 TileSize;

        public int TileColumns;
        public int TileRows;

        public float GridWidth { get { return TileSize.x * TileColumns; } }
        public float GridHeight { get { return TileSize.y * TileRows; } }

        private void Awake()
        {
            Instance = this;
        }

        public Vector2 Align(Vector2 position)
        {
            position -= (Vector2)BottomLeft.position;

            // Get tile coords as float
            var coordX = position.x / TileSize.x;
            var coordY = position.y / TileSize.y;
            
            // Round in order to get nearest coord
            coordX = Mathf.Round(coordX);
            coordY = Mathf.Round(coordY);

            // Multiply back by tilesize to get world space
            coordX *= TileSize.x;
            coordY *= TileSize.y;

            return new Vector2(coordX, coordY) + (Vector2) BottomLeft.position;
        }
        

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
