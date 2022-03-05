using System;
using UnityEngine;

namespace UnitySymexCrawler
{
    public abstract class InputSimulator : IDisposable
    {
        public abstract void SimulateDown(KeyCode keyCode);
        public abstract void SimulateUp(KeyCode keyCode);
        public abstract void Dispose();
    }
}