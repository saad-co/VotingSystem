using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace VotingSystem
{
    internal class Voter
    {
        public string VoterName { get; set; }

        public string Cnic { get; set; }

        public string SelectedPartyName { get; set; }



        //------------- following are the methods of the class ---------------
        public Voter()
        {
            VoterName = string.Empty;
            Cnic = string.Empty;
            SelectedPartyName = string.Empty;
        }
        public Voter(string n, string id, string p_n = " ")
        {
            VoterName = n;
            Cnic = id;
            SelectedPartyName = p_n;
        }

        public bool hasVoted(string cnic)
        {
            string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=VotingSystem;Integrated Security=True;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string text = "SELECT TOP 1 * FROM voter WHERE VoterId = @cnic";
                using (SqlCommand cmd = new SqlCommand(text, connection))
                {
                    cmd.Parameters.AddWithValue("@cnic", cnic);

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        // Check if any rows exist
                        if (dr.Read())
                        {
                            // If the query returns at least one row, the voter has voted
                            return true;
                        }
                        else
                        {
                            // No rows were returned, indicating the voter has not voted
                            return false;
                        }
                    }
                }
            }
        }

    }
}
