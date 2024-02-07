using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;

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
                return true;
            }
            else
            {
                return false;
            }
        }

        public void AddVote()
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


            //--------------writting data into the database system
            string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=VotingSystem;Integrated Security=True;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
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

            //------------------writting data into the file stream
            Voter v = new Voter(n, c, "");
            string jsonString = JsonSerializer.Serialize(v);
            using (StreamWriter sw = new StreamWriter("Voters.txt", append: true))
            {
                sw.WriteLine(jsonString);
            }
            Console.WriteLine("Voter added successfully:");
        }
        public void DisplayVoters()
        {
            Console.WriteLine("------------------------------------------------");
            Console.WriteLine("Display Voters");
            Console.WriteLine("------------------------------------------------");
            Console.WriteLine("List of Voters");
            string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=VotingSystem;Integrated Security=True;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "select * from voter";
                SqlCommand cmd = new SqlCommand(query, connection);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Console.Write("Cnic: ");
                    Console.WriteLine(reader.GetString(0));
                    Console.Write("Name: ");
                    Console.WriteLine(reader.GetString(1));
                    if (!reader.IsDBNull(2))
                    {
                        Console.Write("Selected Party: ");
                        Console.WriteLine(reader.GetString(2));
                    }
                    else
                    {
                        Console.Write("Selected Party: ");
                        Console.WriteLine("Party didn't selected yet");
                    }

                }
            }
        }
        public void InsertCandidate(Candidate c)
        {
            candidates.Add(c);
            string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=VotingSystem;Integrated Security=True;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "insert into candidate(id,name,party,votes) values(@id,@name,@party,@votes)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", c.CandidateID);
                    command.Parameters.AddWithValue("@name", c.Name);
                    command.Parameters.AddWithValue("@party", c.Party);
                    command.Parameters.AddWithValue("@votes", c.Votes);
                    command.ExecuteNonQuery();
                }
            }

            using (StreamWriter sw = new StreamWriter("candidate.txt", append: true))
            {
                string jsonString = JsonSerializer.Serialize(c);
                sw.WriteLine(jsonString);
            }

            Console.WriteLine("Candidate Added Successfully");

        }

        public void ReadCandidate(int id)
        {
            Console.WriteLine("-------------------------------------------");
            Console.WriteLine("Read Candidate");
            Console.WriteLine("-------------------------------------------");


            Console.WriteLine("Reading data from the database: \n\n");
            string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=VotingSystem;Integrated Security=True;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "select * from candidate where id = @id";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        // Check if there are rows returned
                        if (reader.HasRows)
                        {
                            // Read the first row
                            reader.Read();

                            // Access the values using the reader's methods (e.g., GetString, GetInt32, etc.)
                            string candidateName = reader.GetString(reader.GetOrdinal("name"));
                            string party = reader.GetString(reader.GetOrdinal("party"));
                            int votes = reader.GetInt32(reader.GetOrdinal("votes"));

                            // Use the retrieved data as needed
                            Console.WriteLine($"Candidate ID: {id}");
                            Console.WriteLine($"Name: {candidateName}");
                            Console.WriteLine($"Party: {party}");
                            Console.WriteLine($"Votes: {votes}");
                        }
                        else
                        {
                            Console.WriteLine($"Candidate with ID {id} not found.");
                        }
                    }
                }

                Console.WriteLine("\nReading data from the File: \n");
                using (StreamReader sr = new StreamReader("candidate.txt"))
                {
                    while (!sr.EndOfStream)
                    {
                        string jsonStr = sr.ReadLine();
                        Candidate cff = JsonSerializer.Deserialize<Candidate>(jsonStr);
                        // Check if the candidate ID matches the desired ID
                        if (cff.CandidateID == id)
                        {
                            Console.WriteLine($"Candidate ID: {id}");
                            Console.WriteLine($"Name: {cff.Name}");
                            Console.WriteLine($"Party: {cff.Party}");
                            Console.WriteLine($"Votes: {cff.Votes}");
                            return; // Found the candidate, exit the loop
                        }
                    }

                    Console.WriteLine($"Candidate with ID {id} not found in the file.");
                }
            }

        }

        public void displayCandidates()
        {
            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine("Display Candidates");
            Console.WriteLine("-------------------------------------------------");

            Console.WriteLine("ID\tNAME\t\tPARTY\t\tVOTES");

            string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=VotingSystem;Integrated Security=True;";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = "select * from candidate";
                SqlCommand cmd = new SqlCommand(query, con);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int candidateID = reader.GetInt32(reader.GetOrdinal("id"));
                        string candidateName = reader.GetString(reader.GetOrdinal("name"));
                        string party = reader.GetString(reader.GetOrdinal("party"));
                        int votes = reader.GetInt32(reader.GetOrdinal("votes"));

                        // Format and display the candidate information
                        Console.WriteLine($"{candidateID}\t{candidateName}\t\t{party}\t\t{votes}");
                    }
                }
            }
        }


    }
}
