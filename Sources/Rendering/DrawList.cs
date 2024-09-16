using System.Collections.Generic;

namespace GLSample.Rendering
{
    public class DrawList
    {
        public string Name { get; }

        private readonly List<GraphicObject> _objects;

        public DrawList(string name)
        {
            Name = name;
            _objects = new List<GraphicObject>(16);
        }

        public void Clear()
        {
            _objects.Clear();
        }

        public void Add(GraphicObject obj)
        {
            _objects.Add(obj);
        }

        public void Draw(DrawingSettings settings)
        {
            foreach (var obj in _objects)
            {
                obj.Draw(settings);
            }
        }
    }
}
