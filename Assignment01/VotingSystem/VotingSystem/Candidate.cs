using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace VotingSystem
{
    internal class Candidate
    {
        private int candidateID;

        public int CandidateID
        {
            get { return candidateID; }
        }

        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private string party;

        public string Party
        {
            get { return party; }
            set { party = value; }
        }

        private int votes;

        public int Votes
        {
            get { return votes; }
        }

        //--------------- following are all the methods of the class -------------
        private int GenerateCandidateID()
        {
            Random random = new Random();

            // Generating a random integer for the candidate ID
            this.candidateID = random.Next();

            return this.CandidateID;
        }

        public Candidate(string name, string party)
        {
            this.Name = name;
            this.party = party;
            this.votes = 0;
            this.candidateID = GenerateCandidateID();
        }

        public void IncrementVotes()
        {
            this.votes = this.votes + 1;
        }

        public override string ToString()
        {
            return $"Candidate ID: {this.CandidateID}\nCandidate Name: {Name}\nCandidate Party: {party}\nTotal Votes: {Votes}";
        }
    }
}
