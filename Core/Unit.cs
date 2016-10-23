using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;

namespace MyGameInfrastructure
{
    public abstract class Unit
    {

    }

    public class InfantryMan : Unit
    {
        public Node Node { get; set; }
        public StaticModel Model { get; set; }
        public bool IsDead { get; set; }
    }

    public class InfantryPlatoon : Unit
    {
        public InfantryMan[,] InfantryMen = new InfantryMan[3, 12];

    }
}
