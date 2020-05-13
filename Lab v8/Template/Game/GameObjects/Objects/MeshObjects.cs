using System;
using System.Collections;
using System.Collections.Generic;

namespace Template.Graphics
{
    public class MeshObjects : IDisposable, IEnumerable<MeshObject>
    {
        private List<MeshObject> _objects;
        public int Count { get => _objects.Count; }
        public MeshObject this[int index] { get => _objects[index]; set => _objects[index] = value; }
        public MeshObject this[string name]
        {
            get
            {
                foreach (MeshObject meshObject in _objects)
                {
                    if (meshObject.Name == name) return meshObject;
                }
                return null;
            }
        }

        public MeshObjects()
        {
            _objects = new List<MeshObject>(4);
        }

        public void Add(MeshObject meshObject)
        {
            _objects.Add(meshObject);
            //meshObject.Index = _objects.Count - 1;
        }

        public void AddRange(List<MeshObject> meshObjects)
        {
            _objects.AddRange(meshObjects);
        }

        public bool IsEmpty()
        {
            return _objects.Count == 0;
        }

        public void Dispose()
        {
            for (int i = _objects.Count - 1; i >= 0; --i)
            {
                MeshObject meshObject = _objects[i];
                _objects.RemoveAt(i);
                meshObject.Dispose();
            }
        }

        public IEnumerator<MeshObject> GetEnumerator()
        {
            foreach(MeshObject meshObject in _objects)
            {
                yield return meshObject;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
