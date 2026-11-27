using System.Collections.Generic;

namespace Fulliron.Alchemy
{
    public class Placement
    {
        public AlchemicalElement anchorElement;
        public AlchemicalItem item;

        public Placement(AlchemicalElement anchorElement, AlchemicalItem item)
        {
            this.anchorElement = anchorElement;
            this.item = item;
        }
    }

    public class ReactionRequest
    {
        public string symbolId;
        public List<Placement> placements = new List<Placement>();
    }
}
