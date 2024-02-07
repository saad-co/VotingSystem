using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VotingSystem
{
    internal class VotingMachine
    {
        //------------data members of the class with setter/getter-----------
        private List<Candidate> candidates;

        public List<Candidate> Candidate
        {
            get { return candidates; }
            set { candidates = value; }
        }

        //-------------following are the methods of this class---------

        public VotingMachine()
        {
            candidates = new List<Candidate>();
        }

        public bool castVote(Candidate c, Voter v)
        {
            if (!v.hasVoted(v.CNIC))
            {
                c.IncrementVotes();
                addVote();
                return true;
            }
            else
            {
                return false;
            }
        }

        public void addVote()
        {
            Console.WriteLine("---------------------------------------");
            Console.WriteLine("1: Add Voter");
            Console.WriteLine("---------------------------------------");

            Console.Write("Enter your Name: ");
            string n = Console.ReadLine();
            Console.Write('\n');

            Console.Write("Enter your Cnic: ");
            string c = Console.ReadLine();
            Console.Write('\n');

            //string p = "";

            string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=VotingSystem;Integrated Security=True;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                //while (true)
                //{
                //    try
                //    {
                //        Console.Write("Enter Party Name: ");
                //        p = Console.ReadLine();
                //        Console.Write('\n');
                //        if (!CheckParty(connection, p))
                //        {
                //            throw new Exception("Selected Party does not exist. Enter again");
                //        }
                //        else
                //        {
                //            break;
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        Console.WriteLine(ex.Message);
                //    }
                //}

                // Use parameterized query to prevent SQL injection
                string commandText = "INSERT INTO voter (VoterId, VoterName) VALUES (@id, @name)";
                using (SqlCommand cmd = new SqlCommand(commandText, connection))
                {
                    // Add parameters to the query
                    cmd.Parameters.AddWithValue("@id", c);
                    cmd.Parameters.AddWithValue("@name", n);
                    //cmd.Parameters.AddWithValue("@party", p);
                    cmd.ExecuteNonQuery();
                }
            }

            Console.WriteLine("Voter added successfully:");
        }

        private bool CheckParty(SqlConnection connection, string prty_name)
        {
            string text = "SELECT TOP 1 * FROM voter WHERE LOWER(Party) = LOWER(@prty_name)";
            using (SqlCommand cmd = new SqlCommand(text, connection))
            {
                cmd.Parameters.AddWithValue("@prty_name", prty_name);

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    // Check if any rows exist
                    return dr.HasRows;
                }
            }
        }


    }
}
