using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldsBestBars.Services
{
    internal class DefaultServiceResolver : IServiceResolver
    {
        public T GetService<T>() where T : new()
        {
            return new T();
        }
    }
}
