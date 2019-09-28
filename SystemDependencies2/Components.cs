using System.Collections.Generic;

namespace SystemDependencies2
{
    public class Components
    {
        public string Name { get; set; }
        public List<Components> Dependants { get; set; } = new List<Components>();
    }
}
