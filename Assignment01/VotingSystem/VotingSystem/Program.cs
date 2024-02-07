using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VotingSystem
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Candidate c = new Candidate("nawaz shareef", "PMLN");
            VotingMachine machine = new VotingMachine();
            machine.displayCandidates();
        }
    }
}
