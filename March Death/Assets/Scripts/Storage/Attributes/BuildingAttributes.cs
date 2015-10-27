using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Storage
{
    public class BuildingAttributes : EntityAttributes
    {
        public EntityResources sellValue;
        
        public int timeToBuild;
        public float repairSpeed; // Auto repair speed
    }
}