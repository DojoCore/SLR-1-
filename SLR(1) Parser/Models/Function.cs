using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLR_1__Parser.Models
{
    public class Function
    {
        public State From { get; set; }
        public State To { get; set; }
        public Symbol By { get; set; }

        public override string ToString()
        {
            return From.Name + " --" + By.Value + "-> " + To.Name;
        }
    }
}
