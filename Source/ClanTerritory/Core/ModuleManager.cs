using System.Collections.Generic;
using ClanTerritory.Abstractions;

namespace ClanTerritory.Core
{
    internal sealed class ModuleManager
    {
        private readonly List<object> _modules = new List<object>();

        public void Register(object module)
        {
            if (module == null)
                return;

            _modules.Add(module);
        }

        public void InitializeAll()
        {
            for (int i = 0; i < _modules.Count; i++)
            {
                IInitializable initializable = _modules[i] as IInitializable;

                if (initializable != null)
                    initializable.Initialize();
            }
        }

        public void ShutdownAll()
        {
            for (int i = _modules.Count - 1; i >= 0; i--)
            {
                IDisposableModule disposable = _modules[i] as IDisposableModule;

                if (disposable != null)
                    disposable.Shutdown();
            }
        }
    }
}