using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldsBestBars.Services
{
    public interface IServiceResolver
    {
        T GetService<T>() where T : new();
    }
}
