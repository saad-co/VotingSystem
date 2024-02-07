using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VotingSystem
{
    internal class Voter
    {
        //-----------------data members of the class------------
        private string voterName;

        public string VoterName
        {
            get { return voterName; }
            set { voterName = value; }
        }

        private string cnic;

        public string CNIC
        {
            get { return cnic; }
            set { cnic = value; }
        }

        private string selectedPartyName;

        public string SelectedPartyName
        {
            get { return selectedPartyName; }
        }



        //------------- following are the methods of the class ---------------
        public Voter(string name, string identity, string par_name)
        {
            this.voterName = name;
            this.CNIC = identity;
            this.selectedPartyName = par_name;
        }

        public bool hasVoted(string cnic)
        {
            string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=VotingSystem;Integrated Security=True;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string text = "SELECT TOP 1 * FROM voter WHERE id = @cnic";
                using (SqlCommand cmd = new SqlCommand(text, connection))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        // Check if any rows exist
                        if (dr[2] == null)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }
        }
    }
}
